using Aurora.Profiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aurora.Settings
{
    public class ProfileManager
    {
        public string Name { get; set; }
        public string InternalName { get; set; }
        public string[] ProcessNames { get; set; }
        internal ImageSource Icon { get; set; }
        internal UserControl Control { get; set; }
        public ProfileSettings Settings { get; set; }
        public GameEvent Event { get; set; }
        public Dictionary<string, ProfileSettings> Profiles { get; set; } //Profile name, Profile Settings

        public ProfileManager(string name, string internal_name, string process_name, ProfileSettings settings, GameEvent game_event)
        {
            Name = name;
            InternalName = internal_name;
            ProcessNames = new string[] { process_name };
            Icon = null;
            Control = null;
            Settings = settings;
            Event = game_event;
            Profiles = new Dictionary<string, ProfileSettings>();
            LoadProfiles();
        }

        public ProfileManager(string name, string internal_name, string[] process_names, ProfileSettings settings, GameEvent game_event)
        {
            Name = name;
            InternalName = internal_name;
            ProcessNames = process_names;
            Icon = null;
            Control = null;
            Settings = settings;
            Event = game_event;
            Profiles = new Dictionary<string, ProfileSettings>();
            LoadProfiles();
        }

        public virtual UserControl GetUserControl()
        {
            return null;
        }

        public virtual ImageSource GetIcon()
        {
            return null;
        }

        private string GetValidFilename(string filename)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, '_');
            }

            return filename;
        }

        internal virtual ProfileSettings LoadProfile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    string profile_content = File.ReadAllText(path, Encoding.UTF8);

                    if (!String.IsNullOrWhiteSpace(profile_content))
                        return JsonConvert.DeserializeObject<ProfileSettings>(profile_content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
                }
            }
            catch(Exception exc)
            {
                Global.logger.LogLine(string.Format("Exception Loading Profile: {0}, Exception: {1}", path, exc), Logging_Level.Error);
            }

            return null;
        }

        public void LoadProfiles()
        {
            string profiles_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "Profiles", InternalName);

            if (Directory.Exists(profiles_path))
            {
                foreach (string profile in Directory.EnumerateFiles(profiles_path, "*.json", SearchOption.TopDirectoryOnly))
                {
                    string profile_name = Path.GetFileNameWithoutExtension(profile);
                    ProfileSettings profile_settings = LoadProfile(profile);

                    if(profile_settings != null)
                    {
                        if (profile_name.Equals("default"))
                            Settings = profile_settings;
                        else
                            Profiles.Add(profile_name, profile_settings);
                    }
                }
            }
            else
            {
                Global.logger.LogLine(string.Format("Profiles directory for {0} does not exist.", Name), Logging_Level.Info, false);
            }
        }

        internal void SaveProfile(string path, ProfileSettings profile)
        {
            try
            {
                string content = JsonConvert.SerializeObject(profile, Formatting.Indented);

                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                File.WriteAllText(path, content, Encoding.UTF8);

            }
            catch (Exception exc)
            {
                Global.logger.LogLine(string.Format("Exception Saving Profile: {0}, Exception: {1}", path, exc), Logging_Level.Error);
            }
        }

        public void SaveProfiles()
        {
            try
            {
                string profiles_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "Profiles", InternalName);

                if (!Directory.Exists(profiles_path))
                    Directory.CreateDirectory(profiles_path);

                SaveProfile(Path.Combine(profiles_path, "default.json"), Settings);

                foreach (KeyValuePair<string, ProfileSettings> kvp in Profiles)
                {
                    SaveProfile(Path.Combine(profiles_path, kvp.Key + ".json"), kvp.Value);
                }
            }
            catch(Exception exc)
            {
                Global.logger.LogLine("Exception during SaveProfiles, " + exc, Logging_Level.Error);
            }
        }
    }
}
