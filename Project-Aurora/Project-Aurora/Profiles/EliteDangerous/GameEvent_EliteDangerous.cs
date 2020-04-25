using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;
using System.Xml;
using Aurora.Profiles.EliteDangerous.GSI;
using Aurora.Profiles.EliteDangerous.GSI.Nodes;
using Aurora.Profiles.EliteDangerous.Helpers;
using Aurora.Profiles.EliteDangerous.Journal;
using Aurora.Profiles.EliteDangerous.Journal.Events;
using Aurora.Utils;
using CSScriptLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurora.Profiles.EliteDangerous
{
    public class GameEvent_EliteDangerous : GameEvent_Generic
    {
        readonly DelayedMethodCaller delayedFileRead = new DelayedMethodCaller(1);
        private FileSystemWatcher bindWatcher = null, newJournalWatcher = null;

        private string currentBindFile, currentJournalFile;
        private FileWatcher statusFileWatcher, journalFileWatcher;

        JsonSerializerSettings journalSerializerSettings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>() {new JournalEventJsonConverter()}
        };

        public GameEvent_EliteDangerous() : base()
        {
            statusFileWatcher = new FileWatcher(EliteConfig.STATUS_FILE, FileWatcher.ReadMode.FULL, StatusReadCallback);
        }

        public void StatusReadCallback(int lineNumber, string lineValue)
        {
            if (string.IsNullOrEmpty(lineValue)) return;

            Status newStatus = JsonConvert.DeserializeObject<Status>(lineValue);

            @UpdateStatus(newStatus);
        }

        public void UpdateStatus(Status newStatus)
        {
            Status status = (_game_state as GameState_EliteDangerous).Status;

            status.timestamp = newStatus.timestamp;
            status.@event = newStatus.@event;
            status.Flags = newStatus.Flags;
            status.Pips = newStatus.Pips;
            status.FireGroup = newStatus.FireGroup;
            status.GuiFocus = newStatus.GuiFocus;
            status.Fuel = newStatus.Fuel;
            status.Cargo = newStatus.Cargo;
        }

        public void JournalReadCallback(int lineNumber, string lineValue)
        {
            try
            {
                JournalEvent newEvent =
                    JsonConvert.DeserializeObject<JournalEvent>(lineValue, journalSerializerSettings);
                if (newEvent != null)
                {
                    //If the event is known, do something with it
                    (_game_state as GameState_EliteDangerous).Journal.ProcessEvent(newEvent);
                }
            }
            catch (JsonSerializationException e)
            {
                Global.logger.Error("Error deserializing Journal event in " + currentJournalFile + " at line " + lineNumber);
                Global.logger.Error(lineValue);
                Global.logger.Error(e.Message);
                Global.logger.Error(e.StackTrace);
            }
        }

        private void WatchJournalFile()
        {
            StopWatchingJournalFile();
            if (currentJournalFile != null)
            {
                journalFileWatcher = new FileWatcher(currentJournalFile, FileWatcher.ReadMode.TAIL_END, JournalReadCallback);
                journalFileWatcher.Start();
            }
        }
        
        public void StopWatchingJournalFile()
        {
            if (journalFileWatcher != null)
            {
                journalFileWatcher.Stop();
                journalFileWatcher = null;
            }
        }

        private void ReadAllJournalFiles()
        {
            if (!Directory.Exists(EliteConfig.JOURNAL_API_DIR))
            {
                return;
            }
            
            (_game_state as GameState_EliteDangerous).Journal.initialJournalRead = true;
            foreach (string logFile in Directory.GetFiles(EliteConfig.JOURNAL_API_DIR, "*.log")
                .OrderBy(p => new FileInfo(p).CreationTime))
            {
                currentJournalFile = logFile;
                FileWatcher.ReadFileLines(logFile, JournalReadCallback);
            }
            (_game_state as GameState_EliteDangerous).Journal.initialJournalRead = false;
        }

        private void SwitchToNewJournalFile(object sender, FileSystemEventArgs e)
        {
            if (currentJournalFile == null || currentJournalFile.Equals(e.FullPath))
            {
                return;
            }
            
            FileInfo currentInfo = new FileInfo(currentJournalFile);
            FileInfo newInfo = new FileInfo(e.FullPath);
                
            if (newInfo.LastWriteTime > currentInfo.LastWriteTime)
            { 
                Global.logger.Info("A newer journal file was created: " + e.FullPath);
                currentJournalFile = e.FullPath;
                FileWatcher.ReadFileLines(currentJournalFile, JournalReadCallback);
                WatchJournalFile();
            }
        }
        
        private void OnNewJournalFile(object sender, FileSystemEventArgs e)
        {
            /*
             * This event can fire multiple times in a row. We need to make sure to read the file only after
             * the last event is fired to avoid running into a locked file
             */
            delayedFileRead.CallMethod(() => SwitchToNewJournalFile(sender, e));
        }
        
        public void WatchNewJournalFiles()
        {
            StopWatchingNewJournalFiles();
            if (Directory.Exists(EliteConfig.JOURNAL_API_DIR))
            {
                newJournalWatcher = new FileSystemWatcher()
                {
                    Path = EliteConfig.JOURNAL_API_DIR,
                    NotifyFilter = NotifyFilters.LastWrite,
                    Filter = "*.log",
                    EnableRaisingEvents = true
                };
                newJournalWatcher.Changed += OnNewJournalFile;
            }
        }

        public void StopWatchingNewJournalFiles()
        {
            if (newJournalWatcher != null)
            {
                newJournalWatcher.Dispose();
                newJournalWatcher = null;
            }
        }

        public void WatchBindFiles()
        {
            StopWatchingBindFiles();
            if (Directory.Exists(EliteConfig.BINDINGS_DIR))
            {
                bindWatcher = new FileSystemWatcher()
                {
                    Path = EliteConfig.BINDINGS_DIR,
                    NotifyFilter = NotifyFilters.LastWrite,
                    EnableRaisingEvents = true
                };
                bindWatcher.Changed += OnBindsFileChanged;
            }
        }

        public void StopWatchingBindFiles()
        {
            if (bindWatcher != null)
            {
                bindWatcher.Dispose();
                bindWatcher = null;
            }
        }

        private void OnBindsFileChanged(object sender, FileSystemEventArgs e)
        {
            /*
             * This event can fire multiple times in a row. We need to make sure to read the file only after
             * the last event is fired to avoid running into a locked file
             */
            delayedFileRead.CallMethod(() => ReadBindFiles());
        }

        private void ReadBindFiles()
        {
            if (File.Exists(EliteConfig.BINDINGS_PRESET_FILE))
            {
                string currentBindPrefix = File.ReadAllText(EliteConfig.BINDINGS_PRESET_FILE).Trim();
                currentBindFile = SearchForBindsFile(EliteConfig.BINDINGS_DIR, currentBindPrefix);

                if (currentBindFile == null)
                {
                    Global.logger.Error("Custom binds not found. Should check default directory.");
                    string currentGamePath = ((EliteDangerousSettings) Application.Settings).GamePath;
                    string defaultBindsDirectory;

                    defaultBindsDirectory = GetDefaultBindsDirectoryFromGamePath(currentGamePath);
                    if (defaultBindsDirectory == null)
                    {
                        currentGamePath = null;
                    }

                    if (currentGamePath == null)
                    {
                        Process active = GetActiveProcess();
                        currentGamePath = active.MainModule?.FileName;
                        defaultBindsDirectory = GetDefaultBindsDirectoryFromGamePath(currentGamePath);
                        
                        Global.logger.Info("Game process path: " + currentGamePath);
                        Global.logger.Info("Directory from process path: " + defaultBindsDirectory);
                    }

                    if (defaultBindsDirectory != null)
                    {
                        ((EliteDangerousSettings) Application.Settings).GamePath = currentGamePath;
                        currentBindFile = SearchForBindsFile(defaultBindsDirectory, currentBindPrefix);
                        if (currentBindFile != null)
                        {
                            Global.logger.Info("Found default binds file: " + currentBindFile);
                        }
                    }
                }

                if (currentBindFile != null)
                {
                    ParseBindFile();
                }
                else
                {
                    Global.logger.Error("Binds not found: " + currentBindPrefix);
                }
            }
        }

        private string GetDefaultBindsDirectoryFromGamePath(string gamePath)
        {
            string defaultBindsDirectory = null;
            if (File.Exists(gamePath))
            {
                string bindsDirectory = Path.Combine(Path.GetDirectoryName(gamePath), "ControlSchemes");
                if (Directory.Exists(bindsDirectory))
                {
                    defaultBindsDirectory = bindsDirectory;
                }
            }

            return defaultBindsDirectory;
        }

        private string SearchForBindsFile(string directory, string filePrefix)
        {
            string returnBindsFile = null;
            foreach (string bindFile in Directory.GetFiles(directory, filePrefix + ".*.binds")
            )
            {
                returnBindsFile = bindFile;
            }

            if (returnBindsFile == null)
            {
                foreach (string bindFile in Directory.GetFiles(directory, filePrefix + ".binds")
                )
                {
                    returnBindsFile = bindFile;
                }
            }

            return returnBindsFile;
        }

        private void ParseBindFile()
        {
            Global.logger.Error("Current bind file: " + currentBindFile);

            HashSet<string> modifierKeys = new HashSet<string>();

            Dictionary<string, Bind> commandToBind = new Dictionary<string, Bind>();
            Dictionary<Bind, string> bindToCommand = new Dictionary<Bind, string>();

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(currentBindFile);
                XmlNodeList commands = doc.DocumentElement.ChildNodes;
                foreach (XmlNode command in commands)
                {
                    Bind bind = new Bind(command.Name);
                    foreach (XmlNode xmlMapping in command.ChildNodes)
                    {
                        if (xmlMapping.Name != "Primary" && xmlMapping.Name != "Secondary") continue;
                        if(xmlMapping.Attributes["Key"] == null) continue;

                        Bind.Mapping mapping = new Bind.Mapping();
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

                    if (bind.mappings.Any())
                    {
                        commandToBind[command.Name] = bind;
                        bindToCommand[bind] = command.Name;
                    }
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                Global.logger.Error("Error loading binds file: " + currentBindFile);
            }

            GSI.Nodes.Controls controls = (_game_state as GameState_EliteDangerous).Controls;
            controls.modifierKeys = modifierKeys;
            controls.commandToBind = commandToBind;
            controls.bindToCommand = bindToCommand;
        }
        
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError=true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        
        public Process GetActiveProcess()
        {
            IntPtr handle = GetForegroundWindow();
            uint pID;
   
            GetWindowThreadProcessId(handle, out pID);

            return Process.GetProcessById((Int32)pID);
        }

        public override void OnStart()
        {
            ReadBindFiles();
            WatchBindFiles();
            statusFileWatcher.Start();
            ReadAllJournalFiles();
            WatchJournalFile();
            WatchNewJournalFiles();
        }

        public override void OnStop()
        {
            StopWatchingNewJournalFiles();
            StopWatchingBindFiles();
            StopWatchingJournalFile();
            statusFileWatcher.Stop();
        }
    }
}