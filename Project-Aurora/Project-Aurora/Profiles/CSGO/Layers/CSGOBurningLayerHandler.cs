using System;
using System.Drawing;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Settings.Layers;
using Aurora.Utils;
using Newtonsoft.Json;

namespace Aurora.Profiles.CSGO.Layers
{
    public class CSGOBurningLayerHandlerProperties : LayerHandlerProperties2Color<CSGOBurningLayerHandlerProperties>
    {
        public Color? _BurningColor { get; set; }

        [JsonIgnore]
        public Color BurningColor => Logic._BurningColor ?? _BurningColor ?? Color.Empty;

        public bool? _Animated { get; set; }

        [JsonIgnore]
        public bool Animated => Logic._Animated ?? _Animated ?? false;

        public CSGOBurningLayerHandlerProperties()
        { }

        public CSGOBurningLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            _BurningColor = Color.FromArgb(255, 70, 0);
            _Animated = true;
        }
    }

    public class CSGOBurningLayerHandler : LayerHandler<CSGOBurningLayerHandlerProperties>
    {
        private Random randomizer = new();
        private readonly EffectLayer _burningLayer = new("CSGO - Burning");
        private SolidBrush _solidBrush = new(Color.Empty);

        protected override UserControl CreateControl()
        {
            return new Control_CSGOBurningLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            if (state is not GameState_CSGO csgostate) return _burningLayer;

            //Update Burning

            if (csgostate.Player.State.Burning <= 0) return _burningLayer;
            var burncolor = Properties.BurningColor;

            if (Properties.Animated)
            {
                var redAdjusted = (int)(burncolor.R + (Math.Cos((Time.GetMillisecondsSinceEpoch() + randomizer.Next(75)) / 75.0) * 0.15 * 255));

                byte red = redAdjusted switch
                {
                    > 255 => 255,
                    < 0 => 0,
                    _ => (byte) redAdjusted
                };

                var greenAdjusted = (int)(burncolor.G + (Math.Sin((Time.GetMillisecondsSinceEpoch() + randomizer.Next(150)) / 75.0) * 0.15 * 255));

                byte green = greenAdjusted switch
                {
                    > 255 => 255,
                    < 0 => 0,
                    _ => (byte) greenAdjusted
                };

                var blueAdjusted = (int)(burncolor.B + (Math.Cos((Time.GetMillisecondsSinceEpoch() + randomizer.Next(225)) / 75.0) * 0.15 * 255));

                byte blue = blueAdjusted switch
                {
                    > 255 => 255,
                    < 0 => 0,
                    _ => (byte) blueAdjusted
                };

                burncolor = Color.FromArgb(csgostate.Player.State.Burning, red, green, blue);
            }

            if (_solidBrush.Color == burncolor) return _burningLayer;
            _solidBrush.Color = burncolor;
            _burningLayer.Fill(_solidBrush);
            return _burningLayer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_CSGOBurningLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}