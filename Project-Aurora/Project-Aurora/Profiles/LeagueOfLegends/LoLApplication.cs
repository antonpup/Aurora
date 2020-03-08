using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.LeagueOfLegends
{
    public class LoL : Application
    {
        public LoL()
            : base(new LightEventConfig { 
                Name = "League of Legends",
                ID = "league_of_legends",
                ProcessNames = new[] { "league of legends.exe" },
                ProfileType = typeof(LoLGSIProfile),
                OverviewControlType = typeof(Control_LoL),
                GameStateType = typeof(GSI.GameState_LoL),
                Event = new GameEvent_LoL(),
                IconURI = "Resources/leagueoflegends_48x48.png"
            })
        {
            var extra = new List<LayerHandlerEntry>
            {
                new LayerHandlerEntry("LoLBackgroundLayer", "League of Legends Background Layer", typeof(Layers.LoLBackgroundLayerHandler))
            };

            Global.LightingStateManager.RegisterLayerHandlers(extra, false);

            foreach (var entry in extra)
            {
                Config.ExtraAvailableLayers.Add(entry.Key);
            }
        }
    }
}
