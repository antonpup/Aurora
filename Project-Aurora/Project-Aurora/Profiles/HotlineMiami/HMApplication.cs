using Aurora.Settings;
using Aurora.Settings.Layers;

namespace Aurora.Profiles.HotlineMiami
{
    public class HotlineMiami : Application
    {
        public HotlineMiami()
            : base(new LightEventConfig {
                Name = "Hotline Miami",
                ID = "hotline_miami",
                ProcessNames = new[] { "hotlinegl.exe" },
                SettingsType = typeof(FirstTimeApplicationSettings),
                ProfileType = typeof(HMProfile),
                OverviewControlType = typeof(Control_HM),
                GameStateType = typeof(GameState_Wrapper),
                Event = new GameEvent_Generic(),
                IconURI = "Resources/hotline_32x32.png"
            })
        {
            AllowLayer<WrapperLightsLayerHandler>();
        }
    }
}
