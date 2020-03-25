using Aurora.Profiles.Minecraft.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Minecraft {

    public class Minecraft : Application {

        public Minecraft() : base(new LightEventConfig {
            Name = "Minecraft",
            ID = "minecraft",
            ProcessNames = new[] { "javaw.exe" }, // Require the process to a Java application
            ProcessTitles = new[] { @"^Minecraft" }, // Match any window title that starts with Minecraft
            ProfileType = typeof(MinecraftProfile),
            OverviewControlType = typeof(Control_Minecraft),
            GameStateType = typeof(GSI.GameState_Minecraft),
            Event = new GameEvent_Generic(),
            IconURI = "Resources/minecraft_128x128.png"
        }) {

            List<LayerHandlerEntry> minecraftLayers = new List<LayerHandlerEntry> {
                new LayerHandlerEntry("MinecraftHealthBarLayer", "Minecraft Health Bar Layer", typeof(MinecraftHealthBarLayerHandler)),
                new LayerHandlerEntry("MinecraftBackgroundLayer", "Minecraft Background Layer", typeof(MinecraftBackgroundLayerHandler)),
                new LayerHandlerEntry("MinecraftRainLayer", "Minecraft Rain Layer", typeof(MinecraftRainLayerHandler)),
                new LayerHandlerEntry("MinecraftBurningLayer", "Minecraft Burning Layer", typeof(MinecraftBurnLayerHandler)),
                new LayerHandlerEntry("MinecraftKeyConflictLayer", "Minecraft Key Conflict Layer", typeof(MinecraftKeyConflictLayerHandler))
            };

            Global.LightingStateManager.RegisterLayerHandlers(minecraftLayers, false);
            foreach (var layer in minecraftLayers)
                Config.ExtraAvailableLayers.Add(layer.Key);
        }
    }
}
