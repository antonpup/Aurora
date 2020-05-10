using Aurora.EffectsEngine;
using Aurora.Profiles.Dota_2.GSI;
using Aurora.Profiles.Dota_2.GSI.Nodes;
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

namespace Aurora.Profiles.Dota_2.Layers
{
    public class Dota2KillstreakLayerHandlerProperties : LayerHandlerProperties2Color<Dota2KillstreakLayerHandlerProperties>
    {
        public Color? _NoKillstreakColor { get; set; }

        [JsonIgnore]
        public Color NoKillstreakColor { get { return Logic._NoKillstreakColor ?? _NoKillstreakColor ?? Color.Empty; } }

        public Color? _FirstKillstreakColor { get; set; }

        [JsonIgnore]
        public Color FirstKillstreakColor { get { return Logic._FirstKillstreakColor ?? _FirstKillstreakColor ?? Color.Empty; } }

        public Color? _DoubleKillstreakColor { get; set; }

        [JsonIgnore]
        public Color DoubleKillstreakColor { get { return Logic._DoubleKillstreakColor ?? _DoubleKillstreakColor ?? Color.Empty; } }

        public Color? _TripleKillstreakColor { get; set; }

        [JsonIgnore]
        public Color TripleKillstreakColor { get { return Logic._TripleKillstreakColor ?? _TripleKillstreakColor ?? Color.Empty; } }

        public Color? _QuadKillstreakColor { get; set; }

        [JsonIgnore]
        public Color QuadKillstreakColor { get { return Logic._QuadKillstreakColor ?? _QuadKillstreakColor ?? Color.Empty; } }

        public Color? _PentaKillstreakColor { get; set; }

        [JsonIgnore]
        public Color PentaKillstreakColor { get { return Logic._PentaKillstreakColor ?? _PentaKillstreakColor ?? Color.Empty; } }

        public Color? _HexaKillstreakColor { get; set; }

        [JsonIgnore]
        public Color HexaKillstreakColor { get { return Logic._HexaKillstreakColor ?? _HexaKillstreakColor ?? Color.Empty; } }

        public Color? _SeptaKillstreakColor { get; set; }

        [JsonIgnore]
        public Color SeptaKillstreakColor { get { return Logic._SeptaKillstreakColor ?? _SeptaKillstreakColor ?? Color.Empty; } }

        public Color? _OctaKillstreakColor { get; set; }

        [JsonIgnore]
        public Color OctaKillstreakColor { get { return Logic._OctaKillstreakColor ?? _OctaKillstreakColor ?? Color.Empty; } }

        public Color? _NonaKillstreakColor { get; set; }

        [JsonIgnore]
        public Color NonaKillstreakColor { get { return Logic._NonaKillstreakColor ?? _NonaKillstreakColor ?? Color.Empty; } }

        public Color? _DecaKillstreakColor { get; set; }

        [JsonIgnore]
        public Color DecaKillstreakColor { get { return Logic._DecaKillstreakColor ?? _DecaKillstreakColor ?? Color.Empty; } }

        public Dota2KillstreakLayerHandlerProperties() : base() { }

        public Dota2KillstreakLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            this._NoKillstreakColor = Color.FromArgb(0, 0, 0, 0); //No Streak
            this._FirstKillstreakColor = Color.FromArgb(0, 0, 0, 0); //First kill
            this._DoubleKillstreakColor = Color.FromArgb(255, 255, 255); //Double Kill
            this._TripleKillstreakColor = Color.FromArgb(0, 255, 0); //Killing Spree
            this._QuadKillstreakColor = Color.FromArgb(128, 0, 255); //Dominating
            this._PentaKillstreakColor = Color.FromArgb(255, 100, 100); //Mega Kill
            this._HexaKillstreakColor = Color.FromArgb(255, 80, 0); //Unstoppable
            this._SeptaKillstreakColor = Color.FromArgb(130, 180, 130); //Wicked Sick
            this._OctaKillstreakColor = Color.FromArgb(255, 0, 255); //Monster Kill
            this._NonaKillstreakColor = Color.FromArgb(255, 0, 0); //Godlike
            this._DecaKillstreakColor = Color.FromArgb(255, 80, 0); //Godlike+
        }

    }

    public class Dota2KillstreakLayerHandler : LayerHandler<Dota2KillstreakLayerHandlerProperties>
    {
        private static bool isPlayingKillStreakAnimation = false;
        private double ks_blendamount = 0.0;
        private static long ks_duration = 4000;
        private static long ks_end_time = 0;
        private int current_kill_count = 0;

        protected override UserControl CreateControl()
        {
            return new Control_Dota2KillstreakLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            if (isPlayingKillStreakAnimation && Utils.Time.GetMillisecondsSinceEpoch() >= ks_end_time)
                isPlayingKillStreakAnimation = false;

            EffectLayer killstreak_layer = new EffectLayer("Dota 2 - Killstreak");

            if (state is GameState_Dota2)
            {
                GameState_Dota2 dota2state = state as GameState_Dota2;

                if(current_kill_count < dota2state.Player.Kills)
                {
                    isPlayingKillStreakAnimation = true;

                    ks_end_time = Utils.Time.GetMillisecondsSinceEpoch() + ks_duration;
                }
                current_kill_count = dota2state.Player.Kills;

                if (dota2state.Player.KillStreak >= 2)
                {
                    Color ks_color = getKillStreakColor(dota2state.Player.KillStreak);

                    killstreak_layer.Fill(Utils.ColorUtils.BlendColors(Color.Transparent, ks_color, getKSEffectValue()));
                }
            }

            return killstreak_layer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_Dota2KillstreakLayer).SetProfile(profile);
            base.SetApplication(profile);
        }

        private Color getKillStreakColor(int killstreak_count)
        {
            if (killstreak_count == 0)
                return Properties.NoKillstreakColor;
            else if (killstreak_count == 1)
                return Properties.FirstKillstreakColor;
            else if (killstreak_count == 2)
                return Properties.DoubleKillstreakColor;
            else if (killstreak_count == 3)
                return Properties.TripleKillstreakColor;
            else if (killstreak_count == 4)
                return Properties.QuadKillstreakColor;
            else if (killstreak_count == 5)
                return Properties.PentaKillstreakColor;
            else if (killstreak_count == 6)
                return Properties.HexaKillstreakColor;
            else if (killstreak_count == 7)
                return Properties.SeptaKillstreakColor;
            else if (killstreak_count == 8)
                return Properties.OctaKillstreakColor;
            else if (killstreak_count == 9)
                return Properties.NonaKillstreakColor;
            else if (killstreak_count >= 10)
                return Properties.DecaKillstreakColor;
            else
                return Color.Transparent;
        }

        private double getKSEffectValue()
        {
            if (isPlayingKillStreakAnimation)
            {
                ks_blendamount += 0.15;
                return ks_blendamount = (ks_blendamount > 1.0 ? 1.0 : ks_blendamount);
            }
            else
            {
                ks_blendamount -= 0.15;
                return ks_blendamount = (ks_blendamount < 0.0 ? 0.0 : ks_blendamount);
            }
        }
    }
}