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
    public class CSGODeathLayerHandlerProperties : LayerHandlerProperties2Color<CSGODeathLayerHandlerProperties>
    {
        public Color? _DeathColor { get; set; }

        [JsonIgnore]
        public Color DeathColor { get { return Logic._DeathColor ?? _DeathColor ?? Color.Empty; } }

        public int? _FadeOutAfter { get; set; }

        [JsonIgnore]
        public int FadeOutAfter { get { return Logic._FadeOutAfter ?? _FadeOutAfter ?? 5; } }

        public CSGODeathLayerHandlerProperties() : base() { }

        public CSGODeathLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            this._DeathColor = Color.Red;
            this._FadeOutAfter = 3;
        }

    }

    public class CSGODeathLayerHandler : LayerHandler<CSGODeathLayerHandlerProperties>
    {
        private bool _isDead;
        private int _fadeAlpha = 255;
        private long _lastTimeMillis;
        private readonly SolidBrush _solidBrush = new(Color.Empty);

        public CSGODeathLayerHandler(): base("CSGO - Death Effect")
        {
        }

        protected override UserControl CreateControl()
        {
            return new Control_CSGODeathLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            if (state is not GameState_CSGO gameState) return EffectLayer.EmptyLayer;
            var deathColor = Properties.DeathColor;

            // Confirm if CS:GO Player is correct
            if (!gameState.Provider.SteamID.Equals(gameState.Player.SteamID)) return EffectLayer.EmptyLayer;

            // Are they dead?
            if (!_isDead && gameState.Player.State.Health <= 0 && gameState.Previously.Player.State.Health > 0)
            {
                _isDead = true;
                _lastTimeMillis = Utils.Time.GetMillisecondsSinceEpoch();
                _fadeAlpha = 255;
            }

            if (!_isDead)
            {
                return EffectLayer.EmptyLayer;
            }

            // If so...
            var fadeAlpha = GetFadeAlpha();

            if (fadeAlpha == 0)
            {
                _isDead = false;
                return EffectLayer.EmptyLayer;
            }

            _solidBrush.Color = Color.FromArgb(fadeAlpha, deathColor.R, deathColor.G, deathColor.B);
            EffectLayer.Fill(_solidBrush);
            return EffectLayer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_CSGODeathLayer).SetProfile(profile);
            base.SetApplication(profile);
        }

        private int GetFadeAlpha()
        {
            var t = Utils.Time.GetMillisecondsSinceEpoch() - _lastTimeMillis;
            _lastTimeMillis = Utils.Time.GetMillisecondsSinceEpoch();
            _fadeAlpha -= (int)(t / 10);
            return _fadeAlpha = _fadeAlpha < 0 ? 0 : _fadeAlpha;
        }
    }
}