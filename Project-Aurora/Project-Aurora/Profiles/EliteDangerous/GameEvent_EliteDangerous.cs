using System;
using System.IO;
using System.Xml;

namespace Aurora.Profiles.EliteDangerous
{
    public class GameEvent_EliteDangerous : GameEvent_Generic
    {

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
        
        private void OnBindsFileChanged(object sender, FileSystemEventArgs e)
        {
            // This is a fix for multiple change events being triggered
            if (bindWatcher != null) bindWatcher.EnableRaisingEvents = false;
            ReadBindFiles();
            if (bindWatcher != null) bindWatcher.EnableRaisingEvents = true;
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
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(currentBindFile);
                XmlNodeList binds = doc.DocumentElement.ChildNodes;
                foreach(XmlNode bind in binds)
                {
                    foreach (XmlNode mapping in bind.ChildNodes)
                    {
                        if(mapping.Name == "Primary" || mapping.Name == "Secondary") {
                            // This is a bind
                            Global.logger.Error(bind.Name + " " + mapping.Name + ": " + mapping.Attributes["Device"].Value + " " + mapping.Attributes["Key"].Value);
                        }
                    }
                }

            }
            catch (System.IO.FileNotFoundException)
            {
                Global.logger.Error("Error loading binds file: " + currentBindFile);      
            }
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