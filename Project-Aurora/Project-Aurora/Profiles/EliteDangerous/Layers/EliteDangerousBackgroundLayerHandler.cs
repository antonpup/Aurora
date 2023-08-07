using System.Drawing;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Profiles.EliteDangerous.GSI;
using Aurora.Profiles.EliteDangerous.GSI.Nodes;
using Aurora.Settings.Layers;

namespace Aurora.Profiles.EliteDangerous.Layers
{
    public class EliteDangerousBackgroundHandlerProperties : LayerHandlerProperties2Color<EliteDangerousBackgroundHandlerProperties>
    {
        public Color? _CombatModeColor { get; set; }

        public Color CombatModeColor { get { return Logic._CombatModeColor ?? _CombatModeColor ?? Color.Empty; } }

        public Color? _DiscoveryModeColor { get; set; }

        public Color DiscoveryModeColor { get { return Logic._DiscoveryModeColor ?? _DiscoveryModeColor ?? Color.Empty; } }

        public EliteDangerousBackgroundHandlerProperties()
        { }

        public EliteDangerousBackgroundHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            _CombatModeColor = Color.FromArgb(61, 19, 0);
            _DiscoveryModeColor = Color.FromArgb(0, 38, 61);
        }
    }
    public class EliteDangerousBackgroundLayerHandler : LayerHandler<EliteDangerousBackgroundHandlerProperties>
    {
        private readonly SolidBrush _bg = new(Color.Transparent);

        public EliteDangerousBackgroundLayerHandler() : base("Elite: Dangerous - Background")
        {
        }

        protected override UserControl CreateControl()
        {
            return new Control_EliteDangerousBackgroundLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            GameState_EliteDangerous gameState = state as GameState_EliteDangerous;

            _bg.Color = gameState.Status.IsFlagSet(Flag.HUD_DISCOVERY_MODE) ? Properties.DiscoveryModeColor : Properties.CombatModeColor;
            EffectLayer.FillOver(_bg);

            return EffectLayer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_EliteDangerousBackgroundLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}
