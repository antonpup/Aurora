using Aurora.Profiles.GTA5.Layers;
using System.Collections.Generic;

namespace Aurora.Profiles.GTA5
{
    public class GTA5 : Application
    {
        public GTA5()
            : base(new LightEventConfig { Name = "GTA 5", ID = "gta5", ProcessNames = new[] { "gta5.exe" }, ProfileType = typeof(GTA5Profile), GameStateType = typeof(GSI.GameState_GTA5), Event = new GameEvent_Generic(), IconURI = "Resources/gta5_64x64.png" })
        {
            var extra = new List<LayerHandlerEntry>
            {
                new LayerHandlerEntry("GTA5Background", "GTA 5 Background Layer", typeof(GTA5BackgroundLayerHandler)),
                new LayerHandlerEntry("GTA5PoliceSiren", "GTA 5 Police Siren Layer", typeof(GTA5PoliceSirenLayerHandler)),
            };

            AuroraCore.Instance.LightingStateManager.RegisterLayerHandlers(extra, false);

            foreach (var entry in extra)
            {
                Config.ExtraAvailableLayers.Add(entry.Key);
            }
        }
    }
}
