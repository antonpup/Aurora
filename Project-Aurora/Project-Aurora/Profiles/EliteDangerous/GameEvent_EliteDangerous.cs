using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Xml;
using Aurora.Profiles.EliteDangerous.GSI;
using Aurora.Profiles.EliteDangerous.GSI.Nodes;

namespace Aurora.Profiles.EliteDangerous
{
    public class GameEvent_EliteDangerous : GameEvent_Generic
    {
        readonly DelayedMethodCaller delayedFileRead = new DelayedMethodCaller(1);

        private static readonly string journalFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
            "Saved Games", 
            "Frontier Developments",
            "Elite Dangerous"
        );
        private static readonly string statusFile = Path.Combine(journalFolder, "Status.json");
        
        private static readonly string bindingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Frontier Developments",
            "Elite Dangerous",
            "Options",
            "Bindings"
        );
        private static readonly string bindingsPresetFile = Path.Combine(bindingsFolder, "StartPreset.start");

        private string currentBindFile = null;
        private FileSystemWatcher bindWatcher = null;
        
        public GameEvent_EliteDangerous() : base() {}

        public void WatchBindFiles()
        {
            StopWatchingBindFiles();
            if (Directory.Exists(bindingsFolder))
            {
                bindWatcher = new FileSystemWatcher()
                {
                    Path = bindingsFolder,
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
        
        public class DelayedMethodCaller
        {
            int _delay;
            Timer _timer = new Timer();

            public DelayedMethodCaller(int delay)
            {
                _delay = delay;
            }

            public void CallMethod(Action action)
            {
                if (!_timer.Enabled)
                {
                    _timer = new Timer(_delay)
                    {
                        AutoReset = false
                    };
                    _timer.Elapsed += (object sender, ElapsedEventArgs e) =>
                    {
                        action();
                    };
                    _timer.Start();
                }
                else
                {
                    _timer.Stop();
                    _timer.Start();
                }
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
            if (File.Exists(bindingsPresetFile))
            {
                string currentBindPrefix = File.ReadAllText(bindingsPresetFile).Trim();
                foreach (string bindFile in Directory.GetFiles(bindingsFolder, currentBindPrefix + ".*.binds"))
                {
                    currentBindFile = bindFile;
                }

                if (currentBindFile != null)
                {
                    ParseBindFile();
                }
            }
        }

        private void ParseBindFile()
        {
            //TODO: Parse configuration XML
            Global.logger.Error("Current bind file: " + currentBindFile);
            
            HashSet<string> modifierKeys = new HashSet<string>();

            Dictionary<string, Bind> commandToBind = new Dictionary<string, Bind>();
            Dictionary<Bind, string> bindToCommand = new Dictionary<Bind, string>();
            
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(currentBindFile);
                XmlNodeList commands = doc.DocumentElement.ChildNodes;
                foreach(XmlNode command in commands)
                {
                    Bind bind = new Bind(command.Name);
                    foreach (XmlNode xmlMapping in command.ChildNodes)
                    {
                        if (xmlMapping.Name != "Primary" && xmlMapping.Name != "Secondary") continue;
                        
                        Bind.Mapping mapping = new Bind.Mapping();

                        if (xmlMapping.Attributes["Device"].Value == "Keyboard")
                        {
                            mapping.SetKey(xmlMapping.Attributes["Key"].Value);
                        }
                        
                        foreach (XmlNode property in xmlMapping.ChildNodes)
                        {
                            if (property.Name != "Modifier") continue;
                            
                            if (property.Attributes["Device"].Value == "Keyboard" && !string.IsNullOrEmpty(property.Attributes["Key"].Value))
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
                        commandToBind.Add(command.Name, bind);
                        bindToCommand.Add(bind, command.Name);
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

        public override void OnStart()
        {
            ReadBindFiles();
            WatchBindFiles();
            //TODO: Enable Journal API reading
        }

        public override void OnStop()
        {
            StopWatchingBindFiles();
            //TODO: Disable Journal API reading
        }
    }
}