using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Profiles.ETS2.GSI;

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

            this._LeftBlinkerSequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4 });
            this._RightBlinkerSequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.F9, Devices.DeviceKeys.F10, Devices.DeviceKeys.F11, Devices.DeviceKeys.F12 });
            this._BlinkerOffColor = Color.Empty;
            this._BlinkerOnColor = Color.FromArgb(255, 127, 0);
        }
    }

    public class ETS2BlinkerLayerHandler : LayerHandler<ETS2BlinkerLayerHandlerProperties> {

        public ETS2BlinkerLayerHandler() : base() {
            _ID = "ETS2BlinkerIndicator";
        }

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
