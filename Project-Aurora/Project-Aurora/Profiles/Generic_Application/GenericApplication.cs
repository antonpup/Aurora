using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Generic_Application;

public class GenericApplication : Application
{
    public override ImageSource Icon
    {
        get
        {
            if (icon != null) return icon;
            string iconPath = Path.Combine(GetProfileFolderPath(), "icon.png");

            if (File.Exists(iconPath))
            {
                var memStream = new MemoryStream(File.ReadAllBytes(iconPath));
                BitmapImage b = new BitmapImage();
                b.BeginInit();
                b.StreamSource = memStream;
                b.EndInit();

                icon = b;
            }
            else
                icon = new BitmapImage(new Uri(@"Resources/unknown_app_icon.png", UriKind.Relative));

            return icon;
        }
    }

    public GenericApplication(string processName) : base(new LightEventConfig(new Lazy<LightEvent>(() => new Event_GenericApplication())) {
        Name = "Generic Application",
        ID = processName,
        ProcessNames = new[] { processName },
        SettingsType = typeof(GenericApplicationSettings),
        ProfileType = typeof(GenericApplicationProfile),
        OverviewControlType = typeof(Control_GenericApplication),
        GameStateType = typeof(GameState_Wrapper),
    })
    {
        AllowLayer<WrapperLightsLayerHandler>();
    }

    public override string GetProfileFolderPath()
    {
        return Path.Combine(Global.AppDataDirectory, "AdditionalProfiles", Config.ID);
    }

    protected override ApplicationProfile CreateNewProfile(string profileName)
    {
        ApplicationProfile profile = (ApplicationProfile)Activator.CreateInstance(Config.ProfileType);
        profile.ProfileName = profileName;
        profile.ProfileFilepath = Path.Combine(GetProfileFolderPath(), GetUnusedFilename(GetProfileFolderPath(), profile.ProfileName) + ".json");
        return profile;
    }
}