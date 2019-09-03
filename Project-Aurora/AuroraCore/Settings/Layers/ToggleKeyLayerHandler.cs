using Aurora.EffectsEngine;
using Aurora.Profiles;
using Newtonsoft.Json;

namespace Aurora.Settings.Layers
{

    public class ToggleKeyLayerHandlerProperties : LayerHandlerProperties2Color<ToggleKeyLayerHandlerProperties>
    {

        public ToggleKeyLayerHandlerProperties() : base() { }
        public ToggleKeyLayerHandlerProperties(bool assign_default) : base(assign_default) { }

        public Keybind[] _TriggerKeys { get; set; }
        [JsonIgnore]
        public Keybind[] TriggerKeys { get { return Logic._TriggerKeys ?? _TriggerKeys ?? new Keybind[] { }; } }

        public override void Default()
        {
            base.Default();
            _TriggerKeys = new Keybind[] { };
        }

    }

    public class ToggleKeyLayerHandler : LayerHandler<ToggleKeyLayerHandlerProperties>
    {

        private bool state = false;

        public ToggleKeyLayerHandler() : base()
        {
            _ID = "ToggleKey";

            AuroraCore.Instance.InputEvents.KeyDown += InputEvents_KeyDown;
        }

        public override void Dispose()
        {
            base.Dispose();
            AuroraCore.Instance.InputEvents.KeyDown -= InputEvents_KeyDown;
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer layer = new EffectLayer();
            layer.Set(Properties.Sequence, state ? Properties.SecondaryColor : Properties.PrimaryColor);
            return layer;
        }

        private void InputEvents_KeyDown(object sender, SharpDX.RawInput.KeyboardInputEventArgs e)
        {
            foreach (var kb in Properties.TriggerKeys)
                if (kb.IsPressed())
                    state = !state;
        }
    }
}
