using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Aurora.Profiles.EliteDangerous.GSI;
using Aurora.Profiles.EliteDangerous.GSI.Nodes;
using Aurora.Profiles.EliteDangerous.Helpers;
using Aurora.Profiles.EliteDangerous.Journal;
using Aurora.Utils;
using CSScripting;
using Newtonsoft.Json;

namespace Aurora.Profiles.EliteDangerous;

public class GameEvent_EliteDangerous : GameEvent_Generic
{
    private readonly DelayedMethodCaller _delayedFileRead = new(1);
    private FileSystemWatcher? _bindWatcher, _newJournalWatcher;

    private string? _currentBindFile, _currentJournalFile;
    private readonly FileWatcher _statusFileWatcher;
    private FileWatcher? _journalFileWatcher;

    private readonly JsonSerializerSettings _journalSerializerSettings = new()
    {
        Converters = new List<JsonConverter> {new JournalEventJsonConverter()}
    };

    public GameEvent_EliteDangerous()
    {
        _statusFileWatcher = new FileWatcher(EliteConfig.StatusFile, FileWatcher.ReadMode.FULL, StatusReadCallback);
    }

    private void StatusReadCallback(int lineNumber, string lineValue)
    {
        if (string.IsNullOrEmpty(lineValue)) return;

        var trimmed = lineValue.Split(Environment.NewLine).FirstOrDefault();
        if (trimmed == null) return;
        var status = JsonConvert.DeserializeObject<Status>(trimmed);
        if (status == null) return;

        UpdateStatus(status);
    }

    private void UpdateStatus(Status newStatus)
    {
        var status = (_game_state as GameState_EliteDangerous).Status;

        status.timestamp = newStatus.timestamp;
        status.@event = newStatus.@event;
        status.Flags = newStatus.Flags;
        status.Pips = newStatus.Pips;
        status.FireGroup = newStatus.FireGroup;
        status.GuiFocus = newStatus.GuiFocus;
        status.Fuel = newStatus.Fuel;
        status.Cargo = newStatus.Cargo;
    }

    private void JournalReadCallback(int lineNumber, string lineValue)
    {
        try
        {
            var newEvent = JsonConvert.DeserializeObject<JournalEvent>(lineValue, _journalSerializerSettings);
            if (newEvent != null)
            {
                //If the event is known, do something with it
                (_game_state as GameState_EliteDangerous).Journal.ProcessEvent(newEvent);
            }
        }
        catch (JsonSerializationException e)
        {
            Global.logger.Error("Error deserializing Journal event in " + _currentJournalFile + " at line " + lineNumber);
            Global.logger.Error(lineValue);
            Global.logger.Error(e.Message);
            Global.logger.Error(e.StackTrace);
        }
    }

    private void WatchJournalFile()
    {
        StopWatchingJournalFile();
        if (_currentJournalFile == null) return;
        _journalFileWatcher = new FileWatcher(_currentJournalFile, FileWatcher.ReadMode.TAIL_END, JournalReadCallback);
        _journalFileWatcher.Start();
    }

    private void StopWatchingJournalFile()
    {
        if (_journalFileWatcher != null)
        {
            _journalFileWatcher.Stop();
            _journalFileWatcher = null;
        }
    }

    private void ReadAllJournalFiles()
    {
        if (!Directory.Exists(EliteConfig.JournalApiDir))
        {
            return;
        }
            
        (_game_state as GameState_EliteDangerous).Journal.initialJournalRead = true;
        foreach (var logFile in Directory.GetFiles(EliteConfig.JournalApiDir, "*.log")
                     .OrderBy(p => new FileInfo(p).CreationTime))
        {
            _currentJournalFile = logFile;
            FileWatcher.ReadFileLines(logFile, JournalReadCallback);
        }
        (_game_state as GameState_EliteDangerous).Journal.initialJournalRead = false;
    }

    private void SwitchToNewJournalFile(object? sender, FileSystemEventArgs e)
    {
        if (_currentJournalFile == null || _currentJournalFile.Equals(e.FullPath))
        {
            return;
        }
            
        var currentInfo = new FileInfo(_currentJournalFile);
        var newInfo = new FileInfo(e.FullPath);

        if (newInfo.LastWriteTime <= currentInfo.LastWriteTime) return;
        Global.logger.Information("A newer journal file was created: {Path}", e.FullPath);
        _currentJournalFile = e.FullPath;
        FileWatcher.ReadFileLines(_currentJournalFile, JournalReadCallback);
        WatchJournalFile();
    }
        
    private void OnNewJournalFile(object? sender, FileSystemEventArgs e)
    {
        /*
         * This event can fire multiple times in a row. We need to make sure to read the file only after
         * the last event is fired to avoid running into a locked file
         */
        _delayedFileRead.CallMethod(() => SwitchToNewJournalFile(sender, e));
    }

    private void WatchNewJournalFiles()
    {
        StopWatchingNewJournalFiles();
        if (!Directory.Exists(EliteConfig.JournalApiDir)) return;
        _newJournalWatcher = new FileSystemWatcher
        {
            Path = EliteConfig.JournalApiDir,
            NotifyFilter = NotifyFilters.LastWrite,
            Filter = "*.log",
            EnableRaisingEvents = true
        };
        _newJournalWatcher.Changed += OnNewJournalFile;
    }

    private void StopWatchingNewJournalFiles()
    {
        if (_newJournalWatcher != null)
        {
            _newJournalWatcher.Dispose();
            _newJournalWatcher = null;
        }
    }

    private void WatchBindFiles()
    {
        StopWatchingBindFiles();
        if (!Directory.Exists(EliteConfig.BindingsDir)) return;
        _bindWatcher = new FileSystemWatcher
        {
            Path = EliteConfig.BindingsDir,
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };
        _bindWatcher.Changed += OnBindsFileChanged;
    }

    private void StopWatchingBindFiles()
    {
        if (_bindWatcher == null) return;
        _bindWatcher.Dispose();
        _bindWatcher = null;
    }

    private void OnBindsFileChanged(object? sender, FileSystemEventArgs e)
    {
        /*
         * This event can fire multiple times in a row. We need to make sure to read the file only after
         * the last event is fired to avoid running into a locked file
         */
        _delayedFileRead.CallMethod(ReadBindFiles);
    }

    private void ReadBindFiles()
    {
        if (!File.Exists(EliteConfig.BindingsPresetFile)) return;
        string[] currentBindPrefix = File.ReadAllText(EliteConfig.BindingsPresetFile).GetLines();
        _currentBindFile = SearchForBindsFile(EliteConfig.BindingsDir, currentBindPrefix[1].Trim());

        if (_currentBindFile == null)
        {
            Global.logger.Error("Custom binds not found. Should check default directory");
            var currentGamePath = ((EliteDangerousSettings) Application.Settings).GamePath;

            var defaultBindsDirectory = GetDefaultBindsDirectoryFromGamePath(currentGamePath);
            if (defaultBindsDirectory == null)
            {
                currentGamePath = null;
            }

            if (currentGamePath == null)
            {
                var active = GetActiveProcess();
                currentGamePath = active.MainModule?.FileName;
                defaultBindsDirectory = GetDefaultBindsDirectoryFromGamePath(currentGamePath);
                        
                Global.logger.Information("Game process path: {Path}", currentGamePath);
                Global.logger.Information("Directory from process path: {Directory}", defaultBindsDirectory);
            }

            if (defaultBindsDirectory != null)
            {
                ((EliteDangerousSettings) Application.Settings).GamePath = currentGamePath;
                _currentBindFile = SearchForBindsFile(defaultBindsDirectory, currentBindPrefix[1]);
                if (_currentBindFile != null)
                {
                    Global.logger.Information("Found default binds file: {File}", _currentBindFile);
                }
            }
        }

        if (_currentBindFile != null)
        {
            ParseBindFile();
        }
        else
        {
            Global.logger.Error("Binds not found: {Prefix}", currentBindPrefix);
        }
    }

    private string? GetDefaultBindsDirectoryFromGamePath(string gamePath)
    {
        string defaultBindsDirectory = null;
        if (File.Exists(gamePath))
        {
            var bindsDirectory = Path.Combine(Path.GetDirectoryName(gamePath), "ControlSchemes");
            if (Directory.Exists(bindsDirectory))
            {
                defaultBindsDirectory = bindsDirectory;
            }
        }

        return defaultBindsDirectory;
    }

    private string? SearchForBindsFile(string directory, string filePrefix)
    {
        string returnBindsFile = null;
        foreach (var bindFile in Directory.GetFiles(directory, filePrefix + "*.binds")
                )
        {
            returnBindsFile = bindFile;
        }

        if (returnBindsFile != null) return returnBindsFile;
        foreach (var bindFile in Directory.GetFiles(directory, filePrefix + ".binds"))
        {
            returnBindsFile = bindFile;
        }

        return returnBindsFile;
    }

    private void ParseBindFile()
    {
        Global.logger.Error("Current bind file: {File}", _currentBindFile);

        HashSet<string> modifierKeys = new();

        Dictionary<string, Bind> commandToBind = new();
        Dictionary<Bind, string> bindToCommand = new();

        var doc = new XmlDocument();
        try
        {
            doc.Load(_currentBindFile);
            var commands = doc.DocumentElement.ChildNodes;
            foreach (XmlNode command in commands)
            {
                var bind = new Bind(command.Name);
                foreach (XmlNode xmlMapping in command.ChildNodes)
                {
                    if (xmlMapping.Name != "Primary" && xmlMapping.Name != "Secondary") continue;
                    if(xmlMapping.Attributes["Key"] == null) continue;

                    var mapping = new Bind.Mapping();
                    if (xmlMapping.Attributes["Device"].Value == "Keyboard")
                    {
                        mapping.SetKey(xmlMapping.Attributes["Key"].Value);
                    }

                    foreach (XmlNode property in xmlMapping.ChildNodes)
                    {
                        if (property.Name != "Modifier") continue;

                        if (property.Attributes["Device"].Value == "Keyboard" &&
                            !string.IsNullOrEmpty(property.Attributes["Key"].Value))
                        {
                            modifierKeys.Add(property.Attributes["Key"].Value);
                            mapping.AddModifier(property.Attributes["Key"].Value);
                        }
                    }

                    if (mapping.HasKey())
                    {
                        bind.AddMapping(mapping);
                    }
                }

                if (!bind.mappings.Any()) continue;
                commandToBind[command.Name] = bind;
                bindToCommand[bind] = command.Name;
            }
        }
        catch (FileNotFoundException)
        {
            Global.logger.Error("Error loading binds file: {File}", _currentBindFile);
        }

        var controls = (_game_state as GameState_EliteDangerous).Controls;
        controls.modifierKeys = modifierKeys;
        controls.commandToBind = commandToBind;
        controls.bindToCommand = bindToCommand;
    }

    private Process GetActiveProcess()
    {
        var handle = User32.GetForegroundWindow();

        User32.GetWindowThreadProcessId(handle, out var pId);

        return Process.GetProcessById((int)pId);
    }

    public override void OnStart()
    {
        ReadBindFiles();
        WatchBindFiles();
        _statusFileWatcher.Start();
        ReadAllJournalFiles();
        WatchJournalFile();
        WatchNewJournalFiles();
    }

    public override void OnStop()
    {
        StopWatchingNewJournalFiles();
        StopWatchingBindFiles();
        StopWatchingJournalFile();
        _statusFileWatcher.Stop();
    }
}