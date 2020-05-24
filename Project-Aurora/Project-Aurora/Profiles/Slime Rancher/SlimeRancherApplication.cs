namespace Aurora.Profiles.Slime_Rancher
{

    public class Slime_Rancher : Application {

        public Slime_Rancher() : base(new LightEventConfig
        {
            Name = "Slime Rancher",
            ID = "slime_rancher",
            AppID = "433340",
            ProcessNames = new[] { "SlimeRancher.exe" },
            ProfileType = typeof(SlimeRancherProfile),
            OverviewControlType = typeof(Control_Slime_Rancher),
            GameStateType = typeof(GSI.GameState_Slime_Rancher),
            Event = new GameEvent_Generic(),
            IconURI = "Resources/Slime_Rancher.png"
        })
        { }        
    }
}
