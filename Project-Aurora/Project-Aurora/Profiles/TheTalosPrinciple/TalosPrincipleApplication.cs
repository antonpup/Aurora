using Aurora.Settings;
using Aurora.Settings.Layers;

namespace Aurora.Profiles.TheTalosPrinciple
{
    public class TalosPrinciple : Application
    {
        public TalosPrinciple()
            : base(new LightEventConfig {
                Name = "The Talos Principle",
                ID = "the_talos_principle",
                ProcessNames = new string[] { "talos.exe", "talos_unrestricted.exe" },
                SettingsType = typeof(FirstTimeApplicationSettings),
                ProfileType = typeof(TalosPrincipleProfile),
                OverviewControlType = typeof(Control_TalosPrinciple),
                GameStateType = typeof(GameState_Wrapper),
                Event = new GameEvent_Generic(),
                IconURI = "Resources/talosprinciple_64x64.png"
            })
        {
            AllowLayer<WrapperLightsLayerHandler>();
        }
    }
}
