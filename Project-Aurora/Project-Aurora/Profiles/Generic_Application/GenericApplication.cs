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
        public GenericApplication(string process_name)
            : base(new LightEventConfig { Name="Generic Application", ID=process_name, ProcessNames= new[] { process_name }, SettingsType = typeof(GenericApplicationSettings), ProfileType= typeof(GenericApplicationProfile), OverviewControlType= typeof(Control_GenericApplication), GameStateType= typeof(GameState), Event= new Event_GenericApplication() })
        {
            Config.ExtraAvailableLayers.Add("WrapperLights");
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

        protected override ApplicationProfile CreateNewProfile(string profileName)
        {
            ApplicationProfile profile = (ApplicationProfile)Activator.CreateInstance(Config.ProfileType);
            profile.ProfileName = profileName;
            profile.ProfileFilepath = Path.Combine(GetProfileFolderPath(), GetValidFilename(profile.ProfileName) + ".json");
            return profile;
        }
    }
}
