using System;
using System.Drawing;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Aurora.Utils;
using Newtonsoft.Json;

namespace Aurora.Settings.Layers
{
    public class BreathingLayerHandlerProperties : LayerHandlerProperties2Color<BreathingLayerHandlerProperties>
    {
        public bool? _RandomPrimaryColor { get; set; }

        [JsonIgnore]
        public bool RandomPrimaryColor => Logic._RandomPrimaryColor ?? _RandomPrimaryColor ?? false;

        public bool? _RandomSecondaryColor { get; set; }

        [JsonIgnore]
        public bool RandomSecondaryColor => Logic._RandomSecondaryColor ?? _RandomSecondaryColor ?? false;

        [LogicOverridable("Effect Speed")]
        public float? _EffectSpeed { get; set; }

        [JsonIgnore]
        public float EffectSpeed => Logic._EffectSpeed ?? _EffectSpeed ?? 0.0f;

        public BreathingLayerHandlerProperties()
        { }

        public BreathingLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            _RandomPrimaryColor = false;
            _RandomSecondaryColor = false;
            _EffectSpeed = 1.0f;
        }
    }

    public class BreathingLayerHandler : LayerHandler<BreathingLayerHandlerProperties>
    {
        private readonly EffectLayer _breathingLayer = new("Breathing Layer");

        private Color _currentPrimaryColor = Color.Transparent;
        private Color _currentSecondaryColor = Color.Transparent;

        protected override UserControl CreateControl()
        {
            return new Control_BreathingLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            var currentSine = (float)Math.Pow(Math.Sin((double)(Time.GetMillisecondsSinceEpoch() % 10000L / 10000.0f) * 2 * Math.PI * Properties.EffectSpeed), 2);

            if (currentSine <= 0.0025f * Properties.EffectSpeed && Properties.RandomSecondaryColor)
                _currentSecondaryColor = ColorUtils.GenerateRandomColor();
            else if(!Properties.RandomSecondaryColor)
                _currentSecondaryColor = Properties.SecondaryColor;

            if (currentSine >= 1.0f - 0.0025f * Properties.EffectSpeed && Properties.RandomPrimaryColor)
                _currentPrimaryColor = ColorUtils.GenerateRandomColor();
            else if (!Properties.RandomPrimaryColor)
                _currentPrimaryColor = Properties.PrimaryColor;

            _breathingLayer.Set(Properties.Sequence, ColorUtils.BlendColors(_currentPrimaryColor, _currentSecondaryColor, currentSine));

            return _breathingLayer;
        }
    }
}