using Aurora.Settings;
using Aurora.Settings.Layers;

namespace Aurora.Profiles.Evolve
{
    public class Evolve : Application
    {
        public Evolve()
            : base(new LightEventConfig {
                Name = "Evolve Stage 2",
                ID = "Evolve",
                ProcessNames = new[] { "evolve.exe" },
                SettingsType = typeof(FirstTimeApplicationSettings),
                ProfileType = typeof(EvolveProfile),
                OverviewControlType = typeof(Control_Evolve),
                GameStateType = typeof(GameState_Wrapper),
                Event = new GameEvent_Generic(),
                IconURI = "Resources/evolve_48x48.png"
            })
        {
            AllowLayer<WrapperLightsLayerHandler>();
        }
    }
}
