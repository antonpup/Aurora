using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System.Drawing;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Profiles.ETS2.GSI;
using Common.Devices;

namespace Aurora.Profiles.ETS2.Layers {
    public class ETS2BlinkerLayerHandlerProperties : LayerHandlerProperties2Color<ETS2BlinkerLayerHandlerProperties> {

        public Color? _BlinkerOffColor { get; set; }
        public Color? _BlinkerOnColor { get; set; }

        public KeySequence _LeftBlinkerSequence { get; set; }
        public KeySequence _RightBlinkerSequence { get; set; }

        [JsonIgnore]
        public Color BlinkerOffColor { get { return Logic._BlinkerOffColor ?? _BlinkerOffColor ?? Color.Empty; } }
        [JsonIgnore]
        public Color BlinkerOnColor { get { return Logic._BlinkerOnColor ?? _BlinkerOnColor ?? Color.Empty; } }

        [JsonIgnore]
        public KeySequence LeftBlinkerSequence { get { return Logic._LeftBlinkerSequence ?? _LeftBlinkerSequence ?? new KeySequence(); } }
        [JsonIgnore]
        public KeySequence RightBlinkerSequence { get { return Logic._RightBlinkerSequence ?? _RightBlinkerSequence ?? new KeySequence(); } }

        public ETS2BlinkerLayerHandlerProperties() : base() { }
        public ETS2BlinkerLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default() {
            base.Default();

            _LeftBlinkerSequence = new KeySequence(new DeviceKeys[] { DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4 });
            _RightBlinkerSequence = new KeySequence(new DeviceKeys[] { DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12 });
            _BlinkerOffColor = Color.Empty;
            _BlinkerOnColor = Color.FromArgb(255, 127, 0);
        }
    }

    public class ETS2BlinkerLayerHandler : LayerHandler<ETS2BlinkerLayerHandlerProperties> {

        protected override UserControl CreateControl() {
            return new Control_ETS2BlinkerLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate) {
            EffectLayer blinker_layer = new EffectLayer("ETS2 - Blinker Layer");
            if (gamestate is GameState_ETS2) {
                // Left blinker
                Color trgColor = ((GameState_ETS2)gamestate).Truck.blinkerLeftOn ? Properties.BlinkerOnColor : Properties.BlinkerOffColor;
                blinker_layer.Set(Properties.LeftBlinkerSequence, trgColor);

                // Right blinker
                trgColor = ((GameState_ETS2)gamestate).Truck.blinkerRightOn ? Properties.BlinkerOnColor : Properties.BlinkerOffColor;
                blinker_layer.Set(Properties.RightBlinkerSequence, trgColor);
            }
            return blinker_layer;
        }
    }
}
