using Aurora.Profiles.CSGO.Layers;
using System.Collections.Generic;

namespace Aurora.Profiles.CSGO
{
    public class CSGO : Application
    {
        public CSGO()
            : base(new LightEventConfig { Name = "CS:GO", ID = "csgo", AppID = "730", ProcessNames = new[] { "csgo.exe" }, SettingsType = typeof(FirstTimeApplicationSettings), ProfileType = typeof(CSGOProfile), GameStateType = typeof(GSI.GameState_CSGO), Event = new GameEvent_Generic(), IconURI = "Resources/csgo_64x64.png" })
        {
            //UpdateInterval = 33;

            var extra = new List<LayerHandlerEntry> {
                new LayerHandlerEntry("CSGOBackground", "CSGO Background Layer", typeof(CSGOBackgroundLayerHandler)),
                new LayerHandlerEntry("CSGOBomb", "CSGO Bomb Layer", typeof(CSGOBombLayerHandler)),
                new LayerHandlerEntry("CSGOKillsIndicator", "CSGO Kills Indicator Layer", typeof(CSGOKillIndicatorLayerHandler)),
                new LayerHandlerEntry("CSGOBurning", "CSGO Burning Effect Layer", typeof(CSGOBurningLayerHandler)),
                new LayerHandlerEntry("CSGOFlashbang", "CSGO Flashbang Layer", typeof(CSGOFlashbangLayerHandler)),
                new LayerHandlerEntry("CSGOTyping", "CSGO Typing Layer", typeof(CSGOTypingIndicatorLayerHandler)),
            };

            AuroraCore.Instance.LightingStateManager.RegisterLayerHandlers(extra, false);

            foreach (var entry in extra)
            {
                Config.ExtraAvailableLayers.Add(entry.Key);
            }
        }
    }
}
