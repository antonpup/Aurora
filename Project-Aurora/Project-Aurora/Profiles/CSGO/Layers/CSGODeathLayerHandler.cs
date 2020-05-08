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
        private bool isDead = false;
        private long fadeStartAt = 15;
        private int fadeAlpha = 255;

        protected override UserControl CreateControl()
        {
            return new Control_CSGODeathLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer effectLayer = new EffectLayer("CSGO - Death Effect");

            if (state is GameState_CSGO)
            {
                GameState_CSGO gameState = state as GameState_CSGO;
                Color deathColor = this.Properties.DeathColor;

                // Confirm if CS:GO Player is correct
                if (gameState.Provider.SteamID.Equals(gameState.Player.SteamID))
                {

                    // Are they dead?
                    if (!isDead && gameState.Player.State.Health <= 0 && gameState.Previously.Player.State.Health > 0)
                    {
                        isDead = true;

                        fadeAlpha = 255;
                        fadeStartAt = Utils.Time.GetMillisecondsSinceEpoch() + (long)(this.Properties.FadeOutAfter * 1000D);
                    } else if (gameState.Player.State.Health > 0)
                    {
                        isDead = false;
                        return effectLayer;
                    }

                    // If so...
                    if (isDead)
                    {

                        Global.logger.Info("IsDead");
                        if (fadeStartAt <= Utils.Time.GetMillisecondsSinceEpoch())
                        {
                            int fadeAlpha = getFadeAlpha();
                            Global.logger.Info(fadeAlpha);

                            deathColor = Color.FromArgb(fadeAlpha, deathColor.R, deathColor.G, deathColor.B);

                            if (fadeAlpha == 0)
                            {
                                isDead = false;
                            }
                        }

                        effectLayer.Fill(deathColor);
                    }
                }
            }

            return effectLayer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_CSGODeathLayer).SetProfile(profile);
            base.SetApplication(profile);
        }

        private int getFadeAlpha()
        {
            fadeAlpha -= 15;
            return fadeAlpha = (fadeAlpha < 0 ? 0 : fadeAlpha);
        }
    }
}