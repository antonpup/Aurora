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
            : base(new LightEventConfig { Name=Path.GetFileNameWithoutExtension(process_name), ID=process_name, ProcessNames= new[] { process_name }, SettingsType= typeof(GenericApplicationSettings), OverviewControlType= typeof(Control_GenericApplication), GameStateType= typeof(GameState), Event= new Event_GenericApplication(), IsDeletable = true })
        {
        }

        public override string GetProfileFolderPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "AdditionalProfiles", Config.ID);
        }

        public override ImageSource GetIcon()
        {
            if (icon == null)
            {
                string icon_path = Path.Combine(GetProfileFolderPath(), "icon.png");

                if (System.IO.File.Exists(icon_path))
                {
                    var memStream = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(icon_path));
                    BitmapImage b = new BitmapImage();
                    b.BeginInit();
                    b.StreamSource = memStream;
                    b.EndInit();

                    icon = b;
                }
                else
                    icon = new BitmapImage(new Uri(@"Resources/unknown_app_icon.png", UriKind.Relative));
            }

            return icon;
        }

        public override void LoadProfiles()
        {
            string profilesPath = GetProfileFolderPath();

            if (Directory.Exists(profilesPath))
            {
                this.LoadScripts(profilesPath);

                foreach (string profile in Directory.EnumerateFiles(profilesPath, "*.json", SearchOption.TopDirectoryOnly))
                {
                    string profileFilename = Path.GetFileNameWithoutExtension(profile);

                    if (profileFilename.Equals("app_info"))
                        continue;

                    ProfileSettings profileSettings = LoadProfile(profile);

                    if (profileSettings != null)
                    {
                        this.InitalizeScriptSettings(profileSettings);

                        if (profileFilename.Equals("default"))
                            Settings = profileSettings;
                        else
                        {
                            Profiles.Add(profileSettings);
                        }
                    }
                }
            }
            else
            {
                Global.logger.LogLine(string.Format("Profiles directory for {0} does not exist.", Config.Name), Logging_Level.Info, false);
            }
        }

        public override void Delete()
        {
            string path = GetProfileFolderPath();
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
    }
}
