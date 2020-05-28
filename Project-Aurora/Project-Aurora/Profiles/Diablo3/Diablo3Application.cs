using Aurora.Settings.Layers;

namespace Aurora.Profiles.Diablo3
{
    public class Diablo3 : Application
    {
        public Diablo3()
            : base(new LightEventConfig {
                Name = "Diablo III",
                ID = "Diablo3",
                ProcessNames = new[] { "Diablo III.exe", "Diablo III64.exe" },
                ProfileType = typeof(WrapperProfile),
                OverviewControlType = typeof(Control_Diablo3),
                GameStateType = typeof(GameState_Wrapper),
                Event = new GameEvent_Generic(),
                IconURI = "Resources/diablo3_120x120.png"
            })
        {
            AllowLayer<WrapperLightsLayerHandler>();
        }
    }
}
