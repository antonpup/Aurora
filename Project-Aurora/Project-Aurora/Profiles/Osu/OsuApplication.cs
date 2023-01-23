namespace Aurora.Profiles.Osu;

public class Osu : Application {

    public Osu() : base(new LightEventConfig {
        Name = "Osu!",
        ID = "osu",
        ProcessNames = new[] { "osu!.exe" },
        ProfileType = typeof(OsuProfile),
        OverviewControlType = typeof(Control_Osu),
        GameStateType = typeof(GSI.GameState_Osu),
        IconURI = "Resources/osu_256x256.png"
    }) { }
}