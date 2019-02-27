using Aurora.EffectsEngine;
using Aurora.Profiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    public class BreathingLayerHandlerProperties : LayerHandlerProperties2Color<BreathingLayerHandlerProperties>
    {
        public bool? _RandomPrimaryColor { get; set; }

        [JsonIgnore]
        public bool RandomPrimaryColor { get { return Logic._RandomPrimaryColor ?? _RandomPrimaryColor ?? false; } }

        public bool? _RandomSecondaryColor { get; set; }

        [JsonIgnore]
        public bool RandomSecondaryColor { get { return Logic._RandomSecondaryColor ?? _RandomSecondaryColor ?? false; } }

        [Overrides.LogicOverridable("Effect Speed")]
        public float? _EffectSpeed { get; set; }

        [JsonIgnore]
        public float EffectSpeed { get { return Logic._EffectSpeed ?? _EffectSpeed ?? 0.0f; } }

        public BreathingLayerHandlerProperties() : base() { }

        public BreathingLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            this._RandomPrimaryColor = false;
            this._RandomSecondaryColor = false;
            this._EffectSpeed = 1.0f;
        }
    }

    public class BreathingLayerHandler : LayerHandler<BreathingLayerHandlerProperties>
    {
        private float current_sine = 0.0f;

        private Color current_primary_color = Color.Transparent;
        private Color current_secondary_color = Color.Transparent;

        public BreathingLayerHandler()
        {
            _ID = "Breathing";
        }

        protected override UserControl CreateControl()
        {
            return new Control_BreathingLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            current_sine = (float)Math.Pow(Math.Sin((double)((Utils.Time.GetMillisecondsSinceEpoch() % 10000L) / 10000.0f) * 2 * Math.PI * Properties.EffectSpeed), 2);

            if (current_sine <= 0.0025f * Properties.EffectSpeed && Properties.RandomSecondaryColor)
                current_secondary_color = Utils.ColorUtils.GenerateRandomColor();
            else if(!Properties.RandomSecondaryColor)
                current_secondary_color = Properties.SecondaryColor;

            if (current_sine >= 1.0f - (0.0025f * Properties.EffectSpeed) && Properties.RandomPrimaryColor)
                current_primary_color = Utils.ColorUtils.GenerateRandomColor();
            else if (!Properties.RandomPrimaryColor)
                current_primary_color = Properties.PrimaryColor;

            EffectLayer breathing_layer = new EffectLayer();
            breathing_layer.Set(Properties.Sequence, Utils.ColorUtils.BlendColors(current_primary_color, current_secondary_color, current_sine));

            return breathing_layer;
        }
    }
}