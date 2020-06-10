using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers {

    public class BinaryCounterLayerHandlerProperties : LayerHandlerProperties<BinaryCounterLayerHandlerProperties> {

        // The var path of the variable to use (set though the UI, cannot be set with overrides)
        public string _VariablePath { get; set; }
        [JsonIgnore]
        public string VariablePath => Logic._VariablePath ?? _VariablePath ?? "";

        // Allows the value to be directly set using the overrides system
        [JsonIgnore, LogicOverridable("Value")]
        public double? _Value { get; set; }

        public BinaryCounterLayerHandlerProperties() : base() { }
        public BinaryCounterLayerHandlerProperties(bool empty) : base(empty) { }

        public override void Default() {
            base.Default();
            _VariablePath = "";
        }
    }


    public class BinaryCounterLayerHandler : LayerHandler<BinaryCounterLayerHandlerProperties> {

        private Control_BinaryCounterLayer control;
        protected override UserControl CreateControl() => control ?? (control = new Control_BinaryCounterLayer(this));

        public override void SetApplication(Application profile) {
            base.SetApplication(profile);
            control?.SetApplication(profile);
        }

        public override EffectLayer Render(IGameState gamestate) {
            // Get the current game state value
            double value = Properties.Logic._Value ?? gamestate.GetNumber(Properties.VariablePath);

            // Set the active key
            var layer = new EffectLayer("BinaryCounterCustomLayer");
            for (var i = 0; i < Properties.Sequence.keys.Count; i++)
                if (((int)value & 1 << i) > 0)
                    layer.Set(Properties.Sequence.keys[i], Properties.PrimaryColor);
            return layer;
        }
    }
}
