using Aurora.Profiles.RocketLeague.GSI;
using Aurora.Profiles.RocketLeague.Layers;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

            App.Core.LightingStateManager.RegisterLayerHandlers(extra, false);

            foreach (var entry in extra)
            {
                Config.ExtraAvailableLayers.Add(entry.Key);
            }
        }
    }
}
