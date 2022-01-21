using Aurora.Settings.Layers;

namespace Aurora.Profiles.Rust
{
    public class Rust : Application
    {
        public Rust()
            : base(new LightEventConfig {
                Name = "Rust",
                ID = "rust",
                ProcessNames = new[] { "RustClient.exe" },
                ProfileType = typeof(RazerChromaProfile),
                OverviewControlType = typeof(Control_Rust),
                GameStateType = typeof(GameState_Wrapper),
                Event = new GameEvent_Generic(),
                IconURI = "Resources/rust_icon.png"
            })
        {
            AllowLayer<WrapperLightsLayerHandler>();
        }
    }
}
