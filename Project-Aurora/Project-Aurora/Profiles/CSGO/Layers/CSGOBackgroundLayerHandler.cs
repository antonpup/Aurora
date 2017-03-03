﻿using Aurora.EffectsEngine;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Profiles.CSGO.GSI.Nodes;
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

namespace Aurora.Profiles.CSGO.Layers
{
    public class CSGOBackgroundLayerHandlerProperties : LayerHandlerProperties2Color<CSGOBackgroundLayerHandlerProperties>
    {
        public Color? _DefaultColor { get; set; }

        [JsonIgnore]
        public Color DefaultColor { get { return Logic._DefaultColor ?? _DefaultColor ?? Color.Empty; } }

        public Color? _CTColor { get; set; }

        [JsonIgnore]
        public Color CTColor { get { return Logic._CTColor ?? _CTColor ?? Color.Empty; } }

        public Color? _TColor { get; set; }

        [JsonIgnore]
        public Color TColor { get { return Logic._TColor ?? _TColor ?? Color.Empty; } }

        public bool? _DimEnabled { get; set; }

        [JsonIgnore]
        public bool DimEnabled { get { return Logic._DimEnabled ?? _DimEnabled ?? false; } }

        public double? _DimDelay { get; set; }

        [JsonIgnore]
        public double DimDelay { get { return Logic._DimDelay ?? _DimDelay ?? 0.0; } }

        public CSGOBackgroundLayerHandlerProperties() : base() { }

        public CSGOBackgroundLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            this._DefaultColor = Color.FromArgb(158, 205, 255);
            this._CTColor = Color.FromArgb(158, 205, 255);
            this._TColor = Color.FromArgb(221, 99, 33);
            this._DimEnabled = true;
            this._DimDelay = 15;
        }

    }

    public class CSGOBackgroundLayerHandler : LayerHandler<CSGOBackgroundLayerHandlerProperties>
    {
        private bool isDimming = false;
        private double dim_value = 1.0;
        private long dim_bg_at = 15;

        public CSGOBackgroundLayerHandler() : base()
        {
            _ID = "CSGOBackground";
        }

        protected override UserControl CreateControl()
        {
            return new Control_CSGOBackgroundLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer bg_layer = new EffectLayer("CSGO - Background");

            if (state is GameState_CSGO)
            {
                GameState_CSGO csgostate = state as GameState_CSGO;

                if (csgostate.Player.State.Health == 100 && ((csgostate.Previously.Player.State.Health > -1 && csgostate.Previously.Player.State.Health < 100) || (csgostate.Round.WinTeam == RoundWinTeam.Undefined && csgostate.Previously.Round.WinTeam != RoundWinTeam.Undefined)) && csgostate.Provider.SteamID.Equals(csgostate.Player.SteamID))
                {
                    isDimming = false;
                    dim_bg_at = Utils.Time.GetMillisecondsSinceEpoch() + (Profile.Settings as CSGOSettings).bg_dim_after * 1000L;
                    dim_value = 1.0;
                }


                Color bg_color = this.Properties.DefaultColor;

                switch (csgostate.Player.Team)
                {
                    case PlayerTeam.T:
                        bg_color = this.Properties.TColor;
                        break;
                    case PlayerTeam.CT:
                        bg_color = this.Properties.CTColor;
                        break;
                    default:
                        break;
                }

                if (csgostate.Player.Team == PlayerTeam.CT || csgostate.Player.Team == PlayerTeam.T)
                {
                    if (dim_bg_at <= Utils.Time.GetMillisecondsSinceEpoch() || csgostate.Player.State.Health == 0)
                    {
                        isDimming = true;
                        bg_color = Utils.ColorUtils.MultiplyColorByScalar(bg_color, getDimmingValue());
                    }
                    else
                    {
                        isDimming = false;
                        dim_value = 1.0;
                    }
                }

                bg_layer.Fill(bg_color);
            }

            return bg_layer;
        }

        public override void SetProfile(ProfileManager profile)
        {
            (Control as Control_CSGOBackgroundLayer).SetProfile(profile);
        }

        private double getDimmingValue()
        {
            if (isDimming && Properties.DimEnabled)
            {
                dim_value -= 0.02;
                return dim_value = (dim_value < 0.0 ? 0.0 : dim_value);
            }
            else
                return dim_value = 1.0;
        }
    }
}