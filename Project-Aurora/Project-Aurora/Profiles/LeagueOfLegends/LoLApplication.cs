using Aurora.Profiles.LeagueOfLegends.Layers;

namespace Aurora.Profiles.LeagueOfLegends
{
    public class LoL : Application
    {
        public LoL()
            : base(new LightEventConfig { 
                Name = "League of Legends",
                ID = "league_of_legends",
                ProcessNames = new[] { "league of legends.exe" },
                ProfileType = typeof(LoLGSIProfile),
                OverviewControlType = typeof(Control_LoL),
                GameStateType = typeof(GSI.GameState_LoL),
                Event = new GameEvent_LoL(),
                IconURI = "Resources/leagueoflegends_48x48.png"
            })
        {
            AllowLayer<LoLBackgroundLayerHandler>();
        }
    }
}
