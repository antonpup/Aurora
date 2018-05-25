using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Newtonsoft.Json;

namespace Aurora.Settings.Layers {

    public class ConditionalLayerProperties : LayerHandlerProperties2Color<ConditionalLayerProperties> {

        public string _ConditionPath { get; set; }

        [JsonIgnore]
        public string ConditionPath { get { return Logic._ConditionPath ?? _ConditionPath ?? string.Empty; } }

        public ConditionalLayerProperties() : base() { }
        public ConditionalLayerProperties(bool assign_default = false) : base(assign_default) { }
    }

    public class ConditionalLayerHandler : LayerHandler<ConditionalLayerProperties> {

        public ConditionalLayerHandler() {
            _ID = "Conditional";
        }

        protected override UserControl CreateControl() {
            return new Control_ConditionalLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate) {
            EffectLayer layer = new EffectLayer("Conditional Layer");

            bool result = false;
            if (Properties.ConditionPath.Length > 0)
                try {
                    object tmp = Utils.GameStateUtils.RetrieveGameStateParameter(gamestate, Properties.ConditionPath);
                    result = (bool)Utils.GameStateUtils.RetrieveGameStateParameter(gamestate, Properties.ConditionPath);
                } catch { }

            layer.Set(Properties.Sequence, result ? Properties.PrimaryColor : Properties.SecondaryColor);
            return layer;
        }

        public override void SetApplication(Application profile) {
            if (profile != null) {
                if (!string.IsNullOrWhiteSpace(Properties._ConditionPath) && !profile.ParameterLookup.ContainsKey(Properties._ConditionPath))
                    Properties._ConditionPath = string.Empty;
            }
            (Control as Control_ConditionalLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}
