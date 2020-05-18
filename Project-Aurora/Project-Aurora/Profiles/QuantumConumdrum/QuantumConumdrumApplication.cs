using Aurora.Settings;
using Aurora.Settings.Layers;

namespace Aurora.Profiles.QuantumConumdrum
{
    public class QuantumConumdrum : Application
    {
        public QuantumConumdrum()
            : base(new LightEventConfig {
                Name = "Quantum Conumdrum",
                ID = "QuantumConumdrum",
                ProcessNames = new[] { "TryGame-Win32-Shipping.exe" },
                SettingsType = typeof(FirstTimeApplicationSettings),
                ProfileType = typeof(QuantumConumdrumProfile),
                OverviewControlType = typeof(Control_QuantumConumdrum),
                GameStateType = typeof(GameState_Wrapper),
                Event = new GameEvent_Generic(),
                IconURI = "Resources/qc_256x256.png"
            })
        {
            AllowLayer<WrapperLightsLayerHandler>();
        }
    }
}
