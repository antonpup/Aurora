using Aurora.Profiles.RocketLeague.GSI;
using Aurora.Profiles.RocketLeague.Layers;
using System.Collections.Generic;

namespace Aurora.Profiles.RocketLeague
{
    public class RocketLeague : Application
    {
        public RocketLeague()
            : base(new LightEventConfig { Name = "Rocket League", ID = "rocketleague", ProcessNames = new[] { "rocketleague.exe" }, ProfileType = typeof(RocketLeagueProfile), GameStateType = typeof(GameState_RocketLeague), Event = new GameEvent_RocketLeague(), IconURI = "Resources/rocketleague_256x256.png" })
        {
            Utils.PointerUpdateUtils.MarkAppForUpdate("RocketLeague");

            var extra = new List<LayerHandlerEntry>
            {
                new LayerHandlerEntry("RocketLeagueBackground", "Rocket League Layer", typeof(RocketLeagueBackgroundLayerHandler)),
            };

            AuroraCore.Instance.LightingStateManager.RegisterLayerHandlers(extra, false);

            foreach (var entry in extra)
            {
                Config.ExtraAvailableLayers.Add(entry.Key);
            }
        }
    }
}
