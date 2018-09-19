using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Profiles.Minecraft.GSI;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;

namespace Aurora.Profiles.Minecraft.Layers {

    public class MinecraftKeyConflictLayerProperties : LayerHandlerProperties2Color<MinecraftKeyConflictLayerProperties> {

        // PrimaryColor -> No conflict
        // SecondaryColor -> Hard conflict
        // TertiaryColor -> Soft conflict

        [Newtonsoft.Json.JsonIgnore]
        public Color TertiaryColor => _TertiaryColor ?? Color.Empty;
        public Color? _TertiaryColor { get; set; }

        public MinecraftKeyConflictLayerProperties() : base() { }
        public MinecraftKeyConflictLayerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default() {
            base.Default();
            _PrimaryColor = Color.FromArgb(0, 255, 0);
            _SecondaryColor = Color.FromArgb(255, 0, 0);
            _TertiaryColor = Color.FromArgb(255, 127, 0);
        }
    }


    public class MinecraftKeyConflictLayerHandler : LayerHandler<MinecraftKeyConflictLayerProperties> {
        
        public MinecraftKeyConflictLayerHandler() {
            _ID = "MinecraftKeyConflictLayer";
        }

        protected override UserControl CreateControl() {
            return new Control_MinecraftKeyConflictLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate) {
            EffectLayer layer = new EffectLayer("Minecraft Key Conflict Layer");
            if (gamestate is GameState_Minecraft && (gamestate as GameState_Minecraft).Game.ControlsGuiOpen) {
                layer.Fill(Color.Black); // Hide any other layers behind this one
                // Set all keys in use by any binding to be the no-conflict colour
                foreach (var kb in ((GameState_Minecraft)gamestate).Game.KeyBindings)
                    layer.Set(kb.AffectedKeys, Properties.PrimaryColor);

                // Override the keys for all conflicting keys
                foreach (var (key, isHard) in CalculateConflicts((GameState_Minecraft)gamestate))
                    layer.Set(key, isHard ? Properties.SecondaryColor : Properties.TertiaryColor);
            }
            return layer;
        }

        /// <summary>
        /// <para>Method that calculates the key conflicts based on the GameState's Game.KeyBindings property.
        /// Returns an enumerable of all DeviceKeys with a conflict, and whether they are "hard" or "soft".
        /// Forge shows hard conflicts in red and soft in orange on the in-game keys menu.</para>
        /// See <see cref="GSI.Nodes.MinecraftKeyBinding.ConflictsWith(GSI.Nodes.MinecraftKeyBinding)"/> for how conflicts are calculated.
        /// </summary>
        private IEnumerable<(DeviceKeys[] keys, bool isHard)> CalculateConflicts(GameState_Minecraft state) {
            foreach (var bind in state.Game.KeyBindings) { // For every key binding
                foreach (var otherBind in state.Game.KeyBindings) { // Check against every other key binding
                    if (bind == otherBind) continue;

                    var conflict = bind.ConflictsWith(otherBind); // Check for a conflict
                    if (conflict != GSI.Nodes.MinecraftKeyBindingConflict.None) { // Return the key if it is conflicting
                        yield return (bind.AffectedKeys, conflict == GSI.Nodes.MinecraftKeyBindingConflict.Hard);
                        break;
                    }
                }
            }
        }
    }
}
