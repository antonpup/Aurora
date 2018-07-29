using Aurora.EffectsEngine;
using Aurora.Profiles.Witcher3.GSI;
using Aurora.Profiles.Witcher3.GSI.Nodes;
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

namespace Aurora.Profiles.Witcher3.Layers
{
    public class Witcher3BackgroundLayerHandlerProperties : LayerHandlerProperties2Color<Witcher3BackgroundLayerHandlerProperties>
    {
        public Color? _DefaultColor { get; set; }

        public Color DefaultColor { get { return Logic._DefaultColor ?? _DefaultColor ?? Color.Empty; } }

        public Color? _QuenColor { get; set; }

        public Color QuenColor { get { return Logic._QuenColor ?? _QuenColor ?? Color.Empty; } }

        public Color? _IgniColor { get; set; }

        public Color IgniColor { get { return Logic._IgniColor ?? _IgniColor ?? Color.Empty; } }

        public Color? _AardColor { get; set; }

        public Color AardColor { get { return Logic._AardColor ?? _AardColor ?? Color.Empty; } }

        public Color? _YrdenColor { get; set; }

        public Color YrdenColor { get { return Logic._YrdenColor ?? _YrdenColor ?? Color.Empty; } }

        public Color? _AxiiColor { get; set; }

        public Color AxiiColor { get { return Logic._AxiiColor ?? _AxiiColor ?? Color.Empty; } }


        public Witcher3BackgroundLayerHandlerProperties() : base() { }

        public Witcher3BackgroundLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            this._DefaultColor = Color.Gray;
            this._QuenColor = Color.Yellow;
            this._IgniColor = Color.Red;
            this._AardColor = Color.Blue;
            this._YrdenColor = Color.Purple;
            this._AxiiColor = Color.Green;
        }
    }

    public class Witcher3BackgroundLayerHandler : LayerHandler<Witcher3BackgroundLayerHandlerProperties>
    {


        public Witcher3BackgroundLayerHandler() : base()
        {
            _ID = "Witcher3Background";
        }

        protected override UserControl CreateControl()
        {
            return new Control_Witcher3BackgroundLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer bg_layer = new EffectLayer("Witcher3 - Background");

            if (state is GameState_Witcher3)
            {
                GameState_Witcher3 Witcher3state = state as GameState_Witcher3;

                Color bg_color = this.Properties.DefaultColor;

                switch (Witcher3state.Player.ActiveSign)
                {
                    case WitcherSign.Aard:
                        bg_color = this.Properties.AardColor;
                        break;
                    case WitcherSign.Igni:
                        bg_color = this.Properties.IgniColor;
                        break;
                    case WitcherSign.Quen:
                        bg_color = this.Properties.QuenColor;
                        break;
                    case WitcherSign.Yrden:
                        bg_color = this.Properties.YrdenColor;
                        break;
                    case WitcherSign.Axii:
                        bg_color = this.Properties.AxiiColor;
                        break;
                }
                bg_layer.Fill(bg_color);
            }

            return bg_layer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_Witcher3BackgroundLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}