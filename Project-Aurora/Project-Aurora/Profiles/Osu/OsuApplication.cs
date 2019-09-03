namespace Aurora.Profiles.Osu {

    public class Osu : Application {

        public Osu() : base(new LightEventConfig {
            Name = "Osu!",
            ID = "osu",
            ProcessNames = new[] { "osu!.exe" },
            ProfileType = typeof(OsuProfile),
            GameStateType = typeof(GSI.GameState_Osu),
            Event = new GameEvent_Generic(),
            IconURI = "Resources/osu_256x256.png"
        }) { }
    }
}
