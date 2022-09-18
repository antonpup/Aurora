using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Profiles.Dota_2.GSI;
using Aurora.Profiles.Dota_2.GSI.Nodes;
using Aurora.Settings.Layers;
using Aurora.Utils;
using Newtonsoft.Json;

namespace Aurora.Profiles.Dota_2.Layers
{
    public class Dota2AbilityLayerHandlerProperties : LayerHandlerProperties2Color<Dota2AbilityLayerHandlerProperties>
    {
        public Color? _CanCastAbilityColor { get; set; }

        [JsonIgnore]
        public Color CanCastAbilityColor => Logic._CanCastAbilityColor ?? _CanCastAbilityColor ?? Color.Empty;

        public Color? _CanNotCastAbilityColor { get; set; }

        [JsonIgnore]
        public Color CanNotCastAbilityColor => Logic._CanNotCastAbilityColor ?? _CanNotCastAbilityColor ?? Color.Empty;

        public List<DeviceKeys> _AbilityKeys { get; set; }

        [JsonIgnore]
        public List<DeviceKeys> AbilityKeys => Logic._AbilityKeys ?? _AbilityKeys ?? new List<DeviceKeys>();

        public Dota2AbilityLayerHandlerProperties()
        { }

        public Dota2AbilityLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            _CanCastAbilityColor = Color.FromArgb(0, 255, 0);
            _CanNotCastAbilityColor = Color.FromArgb(255, 0, 0);
            _AbilityKeys = new List<DeviceKeys> { DeviceKeys.Q, DeviceKeys.W, DeviceKeys.E, DeviceKeys.D, DeviceKeys.F, DeviceKeys.R };
        }
    }

    public class Dota2AbilityLayerHandler : LayerHandler<Dota2AbilityLayerHandlerProperties>
    {
        protected override UserControl CreateControl()
        {
            return new Control_Dota2AbilityLayer(this);
        }

        private readonly List<string> _ignoredAbilities = new() { "seasonal", "high_five" };

        public Dota2AbilityLayerHandler(): base("Dota 2 - Abilities")
        {
        }

        private bool _empty = true;
        public override EffectLayer Render(IGameState state)
        {
            if (state is not GameState_Dota2 dota2State) return EffectLayer.EmptyLayer;
            if (dota2State.Map.GameState != DOTA_GameState.DOTA_GAMERULES_STATE_PRE_GAME &&
                dota2State.Map.GameState != DOTA_GameState.DOTA_GAMERULES_STATE_GAME_IN_PROGRESS)
                return EffectLayer.EmptyLayer;
            for (var index = 0; index < dota2State.Abilities.Count; index++)
            {
                var ability = dota2State.Abilities[index];
                if (_ignoredAbilities.Any(ignoredAbilityName => ability.Name.Contains(ignoredAbilityName)))
                    continue;

                _empty = false;

                if (index >= Properties.AbilityKeys.Count) continue;
                var key = Properties.AbilityKeys[index];

                if (ability.CanCast && ability.Cooldown == 0 && ability.Level > 0)
                    EffectLayer.Set(key, Properties.CanCastAbilityColor);
                else if (ability.Cooldown <= 5 && ability.Level > 0)
                    EffectLayer.Set(key, ColorUtils.BlendColors(Properties.CanCastAbilityColor, Properties.CanNotCastAbilityColor, ability.Cooldown / 5.0));
                else
                    EffectLayer.Set(key, Properties.CanNotCastAbilityColor);
            }
            return EffectLayer;

        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_Dota2AbilityLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}
