namespace Aurora.Profiles.TModLoader;

public class TModLoader : Application {
    public TModLoader() : base(new LightEventConfig
    {
        Name = "TModLoader",
        ID = "tmodloader",
        AppID = "1281930",
        ProcessNames = new[] { "tModLoader.exe", "tModLoader64bit.exe" },
        ProfileType = typeof(TModLoaderProfile),
        OverviewControlType = typeof(Control_TModLoader),
        GameStateType = typeof(GSI.GameState_TModLoader),
        IconURI = "Resources/tmodloader.png"
    })
    {
    }
}