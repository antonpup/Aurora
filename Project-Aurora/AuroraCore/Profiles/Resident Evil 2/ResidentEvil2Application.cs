using Aurora.Profiles.ResidentEvil2.GSI;
using Aurora.Profiles.ResidentEvil2.Layers;
using System.Collections.Generic;

namespace Aurora.Profiles.ResidentEvil2
{
    public class ResidentEvil2 : Application
    {
        public ResidentEvil2()
            : base(new LightEventConfig { Name = "Resident Evil 2", ID = "residentevil2", ProcessNames = new[] { "re2.exe" }, ProfileType = typeof(ResidentEvil2Profile), GameStateType = typeof(GameState_ResidentEvil2), Event = new GameEvent_ResidentEvil2(), IconURI = "Resources/re2_256x256.png" })
        {
            Utils.PointerUpdateUtils.MarkAppForUpdate("ResidentEvil2");

            var extra = new List<LayerHandlerEntry>
            {
                new LayerHandlerEntry("ResidentEvil2Health", "RE2 Status Layer", typeof(ResidentEvil2HealthLayerHandler)),
                new LayerHandlerEntry("ResidentEvil2Rank", "RE2 Rank Layer", typeof(ResidentEvil2RankLayerHandler))
            };

            AuroraCore.Instance.LightingStateManager.RegisterLayerHandlers(extra, false);

            foreach (var entry in extra)
            {
                Config.ExtraAvailableLayers.Add(entry.Key);
            }
        }
    }
}
