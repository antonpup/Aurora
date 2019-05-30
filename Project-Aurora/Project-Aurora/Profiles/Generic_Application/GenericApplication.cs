using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Generic_Application
{
    public class GenericApplication : Application
    {
        public override ImageSource Icon
        {
            get
            {
                if (icon == null)
                {
                    string icon_path = Path.Combine(GetProfileFolderPath(), "icon.png");

                    if (File.Exists(icon_path))
                    {
                        var memStream = new MemoryStream(File.ReadAllBytes(icon_path));
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
        }

        public GenericApplication(string process_name)
            : base(new LightEventConfig { Name="Generic Application", ID=process_name, ProcessNames= new[] { process_name }, SettingsType = typeof(GenericApplicationSettings), ProfileType= typeof(GenericApplicationProfile), OverviewControlType= typeof(Control_GenericApplication), GameStateType= typeof(GameState_Wrapper), Event= new Event_GenericApplication() })
        {
            Config.ExtraAvailableLayers.Add("WrapperLights");
        }

        public override string GetProfileFolderPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "AdditionalProfiles", Config.ID);
        }

        protected override ApplicationProfile CreateNewProfile(string profileName)
        {
            ApplicationProfile profile = (ApplicationProfile)Activator.CreateInstance(Config.ProfileType);
            profile.ProfileName = profileName;
            profile.ProfileFilepath = Path.Combine(GetProfileFolderPath(), GetUnusedFilename(GetProfileFolderPath(), profile.ProfileName) + ".json");
            return profile;
        }
    }
}
