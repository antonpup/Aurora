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
        public Color NoKillstreakColor => Logic._NoKillstreakColor ?? _NoKillstreakColor ?? Color.Empty;

        public Color? _FirstKillstreakColor { get; set; }

        [JsonIgnore]
        public Color FirstKillstreakColor => Logic._FirstKillstreakColor ?? _FirstKillstreakColor ?? Color.Empty;

        public Color? _DoubleKillstreakColor { get; set; }

        [JsonIgnore]
        public Color DoubleKillstreakColor => Logic._DoubleKillstreakColor ?? _DoubleKillstreakColor ?? Color.Empty;

        public Color? _TripleKillstreakColor { get; set; }

        [JsonIgnore]
        public Color TripleKillstreakColor => Logic._TripleKillstreakColor ?? _TripleKillstreakColor ?? Color.Empty;

        public Color? _QuadKillstreakColor { get; set; }

        [JsonIgnore]
        public Color QuadKillstreakColor => Logic._QuadKillstreakColor ?? _QuadKillstreakColor ?? Color.Empty;

        public Color? _PentaKillstreakColor { get; set; }

        [JsonIgnore]
        public Color PentaKillstreakColor => Logic._PentaKillstreakColor ?? _PentaKillstreakColor ?? Color.Empty;

        public Color? _HexaKillstreakColor { get; set; }

        [JsonIgnore]
        public Color HexaKillstreakColor => Logic._HexaKillstreakColor ?? _HexaKillstreakColor ?? Color.Empty;

        public Color? _SeptaKillstreakColor { get; set; }

        [JsonIgnore]
        public Color SeptaKillstreakColor => Logic._SeptaKillstreakColor ?? _SeptaKillstreakColor ?? Color.Empty;

        public Color? _OctaKillstreakColor { get; set; }

        [JsonIgnore]
        public Color OctaKillstreakColor => Logic._OctaKillstreakColor ?? _OctaKillstreakColor ?? Color.Empty;

        public Color? _NonaKillstreakColor { get; set; }

        [JsonIgnore]
        public Color NonaKillstreakColor => Logic._NonaKillstreakColor ?? _NonaKillstreakColor ?? Color.Empty;

        public Color? _DecaKillstreakColor { get; set; }

        [JsonIgnore]
        public Color DecaKillstreakColor => Logic._DecaKillstreakColor ?? _DecaKillstreakColor ?? Color.Empty;

        public Dota2KillstreakLayerHandlerProperties() : base() { }

        public Dota2KillstreakLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            _NoKillstreakColor = Color.FromArgb(0, 0, 0, 0); //No Streak
            _FirstKillstreakColor = Color.FromArgb(0, 0, 0, 0); //First kill
            _DoubleKillstreakColor = Color.FromArgb(255, 255, 255); //Double Kill
            _TripleKillstreakColor = Color.FromArgb(0, 255, 0); //Killing Spree
            _QuadKillstreakColor = Color.FromArgb(128, 0, 255); //Dominating
            _PentaKillstreakColor = Color.FromArgb(255, 100, 100); //Mega Kill
            _HexaKillstreakColor = Color.FromArgb(255, 80, 0); //Unstoppable
            _SeptaKillstreakColor = Color.FromArgb(130, 180, 130); //Wicked Sick
            _OctaKillstreakColor = Color.FromArgb(255, 0, 255); //Monster Kill
            _NonaKillstreakColor = Color.FromArgb(255, 0, 0); //Godlike
            _DecaKillstreakColor = Color.FromArgb(255, 80, 0); //Godlike+
        }

    }

    public class Dota2KillstreakLayerHandler : LayerHandler<Dota2KillstreakLayerHandlerProperties>
    {
        private const long KsDuration = 4000;

        private readonly EffectLayer _killstreakLayer = new("Dota 2 - Killstreak");
        private readonly SolidBrush _solidBrush = new(Color.Empty);

        private bool _isPlayingKillStreakAnimation;
        private double _ksBlendAmount;
        private long _ksEndTime;
        private int _currentKillCount;

        protected override UserControl CreateControl()
        {
            return new Control_Dota2KillstreakLayer(this);
        }

        private bool _empty = true;
        public override EffectLayer Render(IGameState state)
        {
            if (_isPlayingKillStreakAnimation && Utils.Time.GetMillisecondsSinceEpoch() >= _ksEndTime)
            {
                _isPlayingKillStreakAnimation = false;
            }

            if (state is not GameState_Dota2 dota2State) return _killstreakLayer;
            if(_currentKillCount < dota2State.Player.Kills)
            {    //player got a kill
                _isPlayingKillStreakAnimation = true;

                _ksEndTime = Utils.Time.GetMillisecondsSinceEpoch() + KsDuration;
            }
            _currentKillCount = dota2State.Player.Kills;
                
            var ksEffectValue = GetKsEffectValue();
            if (ksEffectValue <= 0 && !_empty)
            {
                _killstreakLayer.Clear();
                _empty = true;
            }

            if (dota2State.Player.KillStreak < 2) return _killstreakLayer;
            var ksColor = GetKillStreakColor(dota2State.Player.KillStreak);

            if (!(ksEffectValue > 0)) return _killstreakLayer;
            _killstreakLayer.Clear();
            _solidBrush.Color = Utils.ColorUtils.BlendColors(Color.Transparent, ksColor, ksEffectValue);
            _killstreakLayer.Fill(_solidBrush);
            _empty = false;

            return _killstreakLayer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_Dota2KillstreakLayer)?.SetProfile(profile);
            base.SetApplication(profile);
        }

        private Color GetKillStreakColor(int killstreakCount)
        {
            return killstreakCount switch
            {
                0 => Properties.NoKillstreakColor,
                1 => Properties.FirstKillstreakColor,
                2 => Properties.DoubleKillstreakColor,
                3 => Properties.TripleKillstreakColor,
                4 => Properties.QuadKillstreakColor,
                5 => Properties.PentaKillstreakColor,
                6 => Properties.HexaKillstreakColor,
                7 => Properties.SeptaKillstreakColor,
                8 => Properties.OctaKillstreakColor,
                9 => Properties.NonaKillstreakColor,
                >= 10 => Properties.DecaKillstreakColor,
                _ => Color.Transparent
            };
        }

        private double GetKsEffectValue()
        {
            if (_isPlayingKillStreakAnimation)
            {
                _ksBlendAmount += 0.15;
                return _ksBlendAmount = (_ksBlendAmount > 1.0 ? 1.0 : _ksBlendAmount);
            }

            _ksBlendAmount -= 0.15;
            return _ksBlendAmount = (_ksBlendAmount < 0.0 ? 0.0 : _ksBlendAmount);
        }
    }
}