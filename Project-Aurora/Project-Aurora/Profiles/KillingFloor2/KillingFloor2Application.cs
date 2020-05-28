using Aurora.Settings;
using Aurora.Settings.Layers;

namespace Aurora.Profiles.KillingFloor2
{
    public class KillingFloor2 : Application
    {
        public KillingFloor2()
            : base(new LightEventConfig {
                Name = "Killing Floor 2",
                ID = "KillingFloor2",
                ProcessNames = new[] { "KFGame.exe" },
                SettingsType = typeof(FirstTimeApplicationSettings),
                ProfileType = typeof(WrapperProfile),
                OverviewControlType = typeof(Control_KillingFloor2),
                GameStateType = typeof(GameState_Wrapper),
                Event = new GameEvent_Generic(),
                IconURI = "Resources/kf2.png"
            })
        {
            AllowLayer<WrapperLightsLayerHandler>();
        }
    }
}
