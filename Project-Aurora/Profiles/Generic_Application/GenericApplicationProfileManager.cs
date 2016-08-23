using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Generic_Application
{
    public class GenericApplicationProfileManager : ProfileManager
    {
        public GenericApplicationProfileManager(string process_name)
            : base("Generic Application", process_name, process_name, typeof(GenericApplicationSettings), new Event_GenericApplication(process_name))
        {
        }

        public override string GetProfileFolderPath()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "AdditionalProfiles", InternalName);
            return path;
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_GenericApplication(ProcessNames[0]);

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
            {
                string icon_path = Path.Combine(GetProfileFolderPath(), "icon.png");

                if (System.IO.File.Exists(icon_path))
                {
                    var memStream = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(icon_path));
                    BitmapImage b = new BitmapImage();
                    b.BeginInit();
                    b.StreamSource = memStream;
                    b.EndInit();

                    Icon = b;
                }
                else
                    Icon = new BitmapImage(new Uri(@"Resources/unknown_app_icon.png", UriKind.Relative));
            }

            return Icon;
        }

        public override void LoadProfiles()
        {
            string profiles_path = GetProfileFolderPath();

            if (Directory.Exists(profiles_path))
            {
                foreach (string profile in Directory.EnumerateFiles(profiles_path, "*.json", SearchOption.TopDirectoryOnly))
                {
                    string profile_name = Path.GetFileNameWithoutExtension(profile);
                    ProfileSettings profile_settings = LoadProfile(profile);

                    if (profile_settings != null)
                    {
                        if (profile_name.Equals("default"))
                            Settings = profile_settings;
                        else if (profile_name.Equals("app_info"))
                            continue;
                        else
                        {
                            if (!Profiles.ContainsKey(profile_name))
                                Profiles.Add(profile_name, profile_settings);
                        }
                    }
                }
            }
            else
            {
                Global.logger.LogLine(string.Format("Profiles directory for {0} does not exist.", Name), Logging_Level.Info, false);
            }
        }

        internal override void SaveProfile(string path, ProfileSettings profile)
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

        public override void SaveProfiles()
        {
            try
            {
                string profiles_path = GetProfileFolderPath();

                if (!Directory.Exists(profiles_path))
                    Directory.CreateDirectory(profiles_path);

                SaveProfile(Path.Combine(profiles_path, "default.json"), Settings);

                foreach (KeyValuePair<string, ProfileSettings> kvp in Profiles)
                {
                    SaveProfile(Path.Combine(profiles_path, kvp.Key + ".json"), kvp.Value);
                }
            }
            catch (Exception exc)
            {
                Global.logger.LogLine("Exception during SaveProfiles, " + exc, Logging_Level.Error);
            }
        }
    }
}
