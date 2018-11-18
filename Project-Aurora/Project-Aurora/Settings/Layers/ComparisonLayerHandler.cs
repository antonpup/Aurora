using Aurora.EffectsEngine;
using Aurora.Profiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers {

    public class ComparisonLayerProperties : LayerHandlerProperties2Color<ComparisonLayerProperties> {

        public string _Operand1Path { get; set; }
        public string _Operand2Path { get; set; }
        public ComparisonOperator? _Operator { get; set; }

        [JsonIgnore]
        public string Operand1Path => Logic._Operand1Path ?? _Operand1Path ?? string.Empty;
        [JsonIgnore]
        public string Operand2Path => Logic._Operand2Path ?? _Operand2Path ?? string.Empty;
        [JsonIgnore]
        public ComparisonOperator Operator => Logic._Operator ?? _Operator ?? ComparisonOperator.EQ;

        public ComparisonLayerProperties() : base() { }
        public ComparisonLayerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default() {
            base.Default();
            _Operand1Path = "";
            _Operand2Path = "";
            _Operator = ComparisonOperator.EQ;
        }
    }

    public class ComparisonLayerHandler : LayerHandler<ComparisonLayerProperties> {
        
        public ComparisonLayerHandler() {
            _ID = "Comparison";
        }

        protected override UserControl CreateControl() {
            return new Control_ComparisonLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate) {
            // Parse the operands
            double op1 = Utils.GameStateUtils.TryGetDoubleFromState(gamestate, Properties.Operand1Path);
            double op2 = Utils.GameStateUtils.TryGetDoubleFromState(gamestate, Properties.Operand2Path);

            // Evaluate the operands
            bool cond = false;
            switch (Properties.Operator) {
                case ComparisonOperator.EQ: cond = op1 == op2; break;
                case ComparisonOperator.NEQ: cond = op1 != op2; break;
                case ComparisonOperator.LT: cond = op1 < op2; break;
                case ComparisonOperator.LTE: cond = op1 <= op2; break;
                case ComparisonOperator.GT: cond = op1 > op2; break;
                case ComparisonOperator.GTE: cond = op1 >= op2; break;
            }

            // Render the correct color
            EffectLayer layer = new EffectLayer("Comparison");
            layer.Set(Properties.Sequence, cond ? Properties.PrimaryColor : Properties.SecondaryColor);
            return layer;
        }

        public override void SetApplication(Application profile) {
            if (profile != null) {
                if (!double.TryParse(Properties._Operand1Path, out double value) && !string.IsNullOrWhiteSpace(Properties._Operand1Path) && !profile.ParameterLookup.ContainsKey(Properties._Operand1Path))
                    Properties._Operand1Path = string.Empty;
                if (!double.TryParse(Properties._Operand2Path, out value) && !string.IsNullOrWhiteSpace(Properties._Operand2Path) && !profile.ParameterLookup.ContainsKey(Properties._Operand2Path))
                    Properties._Operand2Path = string.Empty;
            }
            (Control as Control_ComparisonLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }

    /// <summary>
    /// Enum listing various logic operators.
    /// </summary>
    public enum ComparisonOperator {
        [Description("=")]
        EQ,
        [Description("≠")]
        NEQ,
        [Description("<")]
        LT,
        [Description("≤")]
        LTE,
        [Description(">")]
        GT,
        [Description("≥")]
        GTE
    }
}
