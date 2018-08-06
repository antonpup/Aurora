using Aurora.EffectsEngine;
using Aurora.Profiles.Minecraft.GSI;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.Minecraft.Layers {

    public class MinecraftHealthBarLayerHandlerProperties : LayerHandlerProperties<MinecraftHealthBarLayerHandlerProperties> {

        [JsonIgnore]
        public Color NormalHealthColor => _NormalHealthColor ?? Color.Empty;
        public Color? _NormalHealthColor { get; set; }

        [JsonIgnore]
        public bool EnableAbsorptionHealthColor => _EnableAbsorptionHealthColor ?? false;
        public bool? _EnableAbsorptionHealthColor { get; set; }
        [JsonIgnore]
        public Color AbsorptionHealthColor => _AbsorptionHealthColor ?? Color.Empty;
        public Color? _AbsorptionHealthColor { get; set; }

        [JsonIgnore]
        public bool EnableRegenerationHealthColor => _EnableRegenerationHealthColor ?? false;
        public bool? _EnableRegenerationHealthColor { get; set; }
        [JsonIgnore]
        public Color RegenerationHealthColor => _RegenerationHealthColor ?? Color.Empty;
        public Color? _RegenerationHealthColor { get; set; }

        [JsonIgnore]
        public bool EnablePoisonHealthColor => _EnablePoisonHealthColor ?? false;
        public bool? _EnablePoisonHealthColor { get; set; }
        [JsonIgnore]
        public Color PoisonHealthColor => _PoisonHealthColor ?? Color.Empty;
        public Color? _PoisonHealthColor { get; set; }

        [JsonIgnore]
        public bool EnableWitherHealthColor => _EnableWitherHealthColor ?? false;
        public bool? _EnableWitherHealthColor { get; set; }
        [JsonIgnore]
        public Color WitherHealthColor => _WitherHealthColor ?? Color.Empty;
        public Color? _WitherHealthColor { get; set; }

        [JsonIgnore]
        public Color BackgroundColor => _BackgroundColor ?? Color.Empty;
        public Color? _BackgroundColor { get; set; }

        [JsonIgnore]
        public bool GradualProgress => _GradualProgress ?? false;
        public bool? _GradualProgress { get; set; }


        public MinecraftHealthBarLayerHandlerProperties() : base() { }
        public MinecraftHealthBarLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default() {
            base.Default();
            _NormalHealthColor = Color.Red;
            _AbsorptionHealthColor = Color.FromArgb(255, 210, 0);
            _RegenerationHealthColor = Color.FromArgb(240, 75, 100);
            _PoisonHealthColor = Color.FromArgb(145, 160, 30);
            _WitherHealthColor = Color.FromArgb(70, 5, 5);
            _BackgroundColor = Color.Transparent;
            _EnableAbsorptionHealthColor = _EnableRegenerationHealthColor = _EnablePoisonHealthColor = _EnableWitherHealthColor = true;
            _GradualProgress = false;
        }
    }

    public class MinecraftHealthBarLayerHandler : LayerHandler<MinecraftHealthBarLayerHandlerProperties> {

        public MinecraftHealthBarLayerHandler() {
            _ID = "MinecraftHealthBarLayer";
        }

        protected override UserControl CreateControl() {
            return new Control_MinecraftHealthBarLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate) {
            // Ensure the gamestate is for Minecraft, and store a casted reference to it
            if (!(gamestate is GameState_Minecraft)) return new EffectLayer();
            GameState_Minecraft state = gamestate as GameState_Minecraft;

            // Choose the main healthbar's color depending on whether the player is withered/poisoned/regen/normal.
            Color barColor = Properties.NormalHealthColor; // Default normal color
            if (Properties.EnableWitherHealthColor && state.Player.PlayerEffects.HasWither) // Wither takes priority over others
                barColor = Properties.WitherHealthColor;
            else if (Properties.EnablePoisonHealthColor && state.Player.PlayerEffects.HasPoison) // Poison 2nd priority
                barColor = Properties.PoisonHealthColor;
            else if (Properties.EnableRegenerationHealthColor && state.Player.PlayerEffects.HasRegeneration) // Regen 3rd priority
                barColor = Properties.RegenerationHealthColor;

            // Render the main healthbar, with the color decided above.
            EffectLayer layer = new EffectLayer()
                .PercentEffect(barColor, Properties.BackgroundColor, Properties.Sequence, state.Player.Health, state.Player.HealthMax, Settings.PercentEffectType.Progressive);

            // If absorption is enabled, overlay the absorption display on the top of the original healthbar
            if (Properties.EnableAbsorptionHealthColor)
                layer.PercentEffect(Properties.AbsorptionHealthColor, Properties.BackgroundColor, Properties.Sequence, state.Player.Absorption, state.Player.AbsorptionMax, Properties.GradualProgress ? Settings.PercentEffectType.Progressive_Gradual : Settings.PercentEffectType.Progressive);

            return layer;
        }
    }
}
