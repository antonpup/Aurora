using Aurora.EffectsEngine;
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
using Aurora.Profiles.EliteDangerous.GSI;
using Aurora.Profiles.EliteDangerous.GSI.Nodes;

namespace Aurora.Profiles.EliteDangerous.Layers
{
    public class EliteDangerousBackgroundHandlerProperties : LayerHandlerProperties2Color<EliteDangerousBackgroundHandlerProperties>
    {
        public Color? _CombatModeColor { get; set; }

        public Color CombatModeColor { get { return Logic._CombatModeColor ?? _CombatModeColor ?? Color.Empty; } }

        public Color? _DiscoveryModeColor { get; set; }

        public Color DiscoveryModeColor { get { return Logic._DiscoveryModeColor ?? _DiscoveryModeColor ?? Color.Empty; } }

        public EliteDangerousBackgroundHandlerProperties() : base() { }

        public EliteDangerousBackgroundHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            this._CombatModeColor = Color.FromArgb(61, 19, 0);
            this._DiscoveryModeColor = Color.FromArgb(0, 38, 61);
        }
    }
    public class EliteDangerousBackgroundLayerHandler : LayerHandler<EliteDangerousBackgroundHandlerProperties>
    {
        protected override UserControl CreateControl()
        {
            return new Control_EliteDangerousBackgroundLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer bg_layer = new EffectLayer("Elite: Dangerous - Background");
            
            GameState_EliteDangerous gameState = state as GameState_EliteDangerous;

            Color combat_bg_color = gameState.Status.IsFlagSet(Flag.HUD_DISCOVERY_MODE) ? this.Properties.DiscoveryModeColor : this.Properties.CombatModeColor;
            bg_layer.Fill(combat_bg_color);

            return bg_layer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_EliteDangerousBackgroundLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}
