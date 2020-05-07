using Aurora.EffectsEngine;
using Aurora.Profiles.GTA5.GSI;
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

namespace Aurora.Profiles.GTA5.Layers
{
    public class GTA5BackgroundLayerHandlerProperties : LayerHandlerProperties2Color<GTA5BackgroundLayerHandlerProperties>
    {
        public Color? _DefaultColor { get; set; }

        [JsonIgnore]
        public Color DefaultColor { get { return Logic._DefaultColor ?? _DefaultColor ?? Color.Empty; } }

        public Color? _FranklinColor { get; set; }

        [JsonIgnore]
        public Color FranklinColor { get { return Logic._FranklinColor ?? _FranklinColor ?? Color.Empty; } }

        public Color? _MichaelColor { get; set; }

        [JsonIgnore]
        public Color MichaelColor { get { return Logic._MichaelColor ?? _MichaelColor ?? Color.Empty; } }

        public Color? _TrevorColor { get; set; }

        [JsonIgnore]
        public Color TrevorColor { get { return Logic._TrevorColor ?? _TrevorColor ?? Color.Empty; } }

        public Color? _ChopColor { get; set; }

        [JsonIgnore]
        public Color ChopColor { get { return Logic._ChopColor ?? _ChopColor ?? Color.Empty; } }

        public Color? _OnlineSpectatorColor { get; set; }

        [JsonIgnore]
        public Color OnlineSpectatorColor { get { return Logic._OnlineSpectatorColor ?? _OnlineSpectatorColor ?? Color.Empty; } }

        public Color? _OnlineMissionColor { get; set; }

        [JsonIgnore]
        public Color OnlineMissionColor { get { return Logic._OnlineMissionColor ?? _OnlineMissionColor ?? Color.Empty; } }

        public Color? _OnlineHeistFinaleColor { get; set; }

        [JsonIgnore]
        public Color OnlineHeistFinaleColor { get { return Logic._OnlineHeistFinaleColor ?? _OnlineHeistFinaleColor ?? Color.Empty; } }

        public Color? _RaceGoldColor { get; set; }

        [JsonIgnore]
        public Color RaceGoldColor { get { return Logic._RaceGoldColor ?? _RaceGoldColor ?? Color.Empty; } }

        public Color? _RaceSilverColor { get; set; }

        [JsonIgnore]
        public Color RaceSilverColor { get { return Logic._RaceSilverColor ?? _RaceSilverColor ?? Color.Empty; } }

        public Color? _RaceBronzeColor { get; set; }

        [JsonIgnore]
        public Color RaceBronzeColor { get { return Logic._RaceBronzeColor ?? _RaceBronzeColor ?? Color.Empty; } }

        public GTA5BackgroundLayerHandlerProperties() : base() { }

        public GTA5BackgroundLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            this._DefaultColor = Color.FromArgb(255, 255, 255);
            this._FranklinColor = Color.FromArgb(48, 255, 0);
            this._MichaelColor = Color.FromArgb(48, 255, 255);
            this._TrevorColor = Color.FromArgb(176, 80, 0);
            this._ChopColor = Color.FromArgb(127, 0, 0);
            this._OnlineMissionColor = Color.FromArgb(156, 110, 175);
            this._OnlineHeistFinaleColor = Color.FromArgb(255, 122, 196);
            this._OnlineSpectatorColor = Color.FromArgb(142, 127, 153);
            this._RaceGoldColor = Color.FromArgb(255, 170, 0);
            this._RaceSilverColor = Color.FromArgb(191, 191, 191);
            this._RaceBronzeColor = Color.FromArgb(255, 51, 0);
        }

    }

    public class GTA5BackgroundLayerHandler : LayerHandler<GTA5BackgroundLayerHandlerProperties>
    {

        protected override UserControl CreateControl()
        {
            return new Control_GTA5BackgroundLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer bg_layer = new EffectLayer("GTA 5 - Background");

            if (state is GameState_GTA5)
            {
                GameState_GTA5 gta5state = state as GameState_GTA5;

                Color bg_color;

                switch (gta5state.CurrentState)
                {
                    case PlayerState.PlayingSP_Trevor:
                        bg_color = Properties.TrevorColor;
                        break;
                    case PlayerState.PlayingSP_Franklin:
                        bg_color = Properties.FranklinColor;
                        break;
                    case PlayerState.PlayingSP_Michael:
                        bg_color = Properties.MichaelColor;
                        break;
                    case PlayerState.PlayingSP_Chop:
                        bg_color = Properties.ChopColor;
                        break;
                    case PlayerState.PlayingMP_Mission:
                        bg_color = Properties.OnlineMissionColor;
                        break;
                    case PlayerState.PlayingMP_HeistFinale:
                        bg_color = Properties.OnlineHeistFinaleColor;
                        break;
                    case PlayerState.PlayingMP_Spectator:
                        bg_color = Properties.OnlineSpectatorColor;
                        break;
                    case PlayerState.PlayingRace_Gold:
                        bg_color = Properties.RaceGoldColor;
                        break;
                    case PlayerState.PlayingRace_Silver:
                        bg_color = Properties.RaceSilverColor;
                        break;
                    case PlayerState.PlayingRace_Bronze:
                        bg_color = Properties.RaceBronzeColor;
                        break;
                    default:
                        bg_color = gta5state.StateColor ?? Properties.DefaultColor;
                        break;
                }

                bg_layer.Fill(bg_color);
            }

            return bg_layer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_GTA5BackgroundLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}