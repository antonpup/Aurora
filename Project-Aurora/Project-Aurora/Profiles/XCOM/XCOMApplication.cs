using Aurora.Settings;
using Aurora.Settings.Layers;

namespace Aurora.Profiles.XCOM
{
    public class XCOM : Application
    {
        public XCOM()
            : base(new LightEventConfig {
                Name = "XCOM: Enemy Unknown",
                ID = "XCOM", ProcessNames = new[] { "xcomgame.exe" },
                SettingsType = typeof(FirstTimeApplicationSettings),
                ProfileType = typeof(XCOMProfile),
                OverviewControlType = typeof(Control_XCOM),
                GameStateType = typeof(GameState_Wrapper),
                Event = new GameEvent_Generic(),
                IconURI = "Resources/xcom_64x64.png"
            })
        {
            AllowLayer<WrapperLightsLayerHandler>();
        }
    }
}
