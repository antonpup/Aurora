using Aurora.EffectsEngine;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Profiles.CSGO.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.CSGO.Layers
{
    public class CSGOBurningLayerHandlerProperties : LayerHandlerProperties2Color<CSGOBurningLayerHandlerProperties>
    {
        public Color? _BurningColor { get; set; }

        [JsonIgnore]
        public Color BurningColor { get { return Logic._BurningColor ?? _BurningColor ?? Color.Empty; } }

        public bool? _Animated { get; set; }

        [JsonIgnore]
        public bool Animated { get { return Logic._Animated ?? _Animated ?? false; } }

        public CSGOBurningLayerHandlerProperties() : base() { }

        public CSGOBurningLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            this._BurningColor = Color.FromArgb(255, 70, 0);
            this._Animated = true;
        }

    }

    public class CSGOBurningLayerHandler : LayerHandler<CSGOBurningLayerHandlerProperties>
    {
        private Random randomizer = new Random();

        protected override UserControl CreateControl()
        {
            return new Control_CSGOBurningLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer burning_layer = new EffectLayer("CSGO - Burning");

            if (state is GameState_CSGO)
            {
                GameState_CSGO csgostate = state as GameState_CSGO;

                //Update Burning

                if (csgostate.Player.State.Burning > 0)
                {
                    double burning_percent = (double)csgostate.Player.State.Burning / 255.0;
                    Color burncolor = Properties.BurningColor;

                    if (Properties.Animated)
                    {
                        int red_adjusted = (int)(burncolor.R + (Math.Cos((Utils.Time.GetMillisecondsSinceEpoch() + randomizer.Next(75)) / 75.0) * 0.15 * 255));
                        byte red = 0;

                        if (red_adjusted > 255)
                            red = 255;
                        else if (red_adjusted < 0)
                            red = 0;
                        else
                            red = (byte)red_adjusted;

                        int green_adjusted = (int)(burncolor.G + (Math.Sin((Utils.Time.GetMillisecondsSinceEpoch() + randomizer.Next(150)) / 75.0) * 0.15 * 255));
                        byte green = 0;

                        if (green_adjusted > 255)
                            green = 255;
                        else if (green_adjusted < 0)
                            green = 0;
                        else
                            green = (byte)green_adjusted;

                        int blue_adjusted = (int)(burncolor.B + (Math.Cos((Utils.Time.GetMillisecondsSinceEpoch() + randomizer.Next(225)) / 75.0) * 0.15 * 255));
                        byte blue = 0;

                        if (blue_adjusted > 255)
                            blue = 255;
                        else if (blue_adjusted < 0)
                            blue = 0;
                        else
                            blue = (byte)blue_adjusted;

                        burncolor = Color.FromArgb(csgostate.Player.State.Burning, red, green, blue);
                    }

                    burning_layer.Fill(burncolor);
                }
            }

            return burning_layer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_CSGOBurningLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}