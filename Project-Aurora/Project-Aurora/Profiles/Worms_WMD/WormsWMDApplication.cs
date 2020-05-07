using Aurora.Settings.Layers;

namespace Aurora.Profiles.WormsWMD
{
    public class WormsWMD : Application
    {
        public WormsWMD()
            : base(new LightEventConfig {
                Name = "Worms W.M.D",
                ID = "worms_wmd",
                ProcessNames = new[] { "Worms W.M.D.exe" },
                ProfileType = typeof(WrapperProfile),
                OverviewControlType = typeof(Control_WormsWMD),
                GameStateType = typeof(GameState_Wrapper),
                Event = new GameEvent_Generic(),
                IconURI = "Resources/worms_wmd.png"
            })
        {
            AllowLayer<WrapperLightsLayerHandler>();
        }
    }
}
