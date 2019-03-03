using Aurora.Profiles.ResidentEvil2.GSI;
using Aurora.Profiles.ResidentEvil2.Layers;
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

namespace Aurora.Profiles.ResidentEvil2
{
    public class ResidentEvil2 : Application
    {
        public ResidentEvil2()
            : base(new LightEventConfig { Name = "Resident Evil 2", ID = "residentevil2", ProcessNames = new[] { "re2.exe" }, ProfileType = typeof(ResidentEvil2Profile), OverviewControlType = typeof(Control_ResidentEvil2), GameStateType = typeof(GameState_ResidentEvil2), Event = new GameEvent_ResidentEvil2(), IconURI = "Resources/re2_256x256.png" })
        {
            var extra = new List<LayerHandlerEntry>
            {
                new LayerHandlerEntry("ResidentEvil2Health", "RE2 Status Layer", typeof(ResidentEvil2HealthLayerHandler)),
                new LayerHandlerEntry("ResidentEvil2Rank", "RE2 Rank Layer", typeof(ResidentEvil2RankLayerHandler))
            };

            Global.LightingStateManager.RegisterLayerHandlers(extra, false);

            foreach (var entry in extra)
            {
                Config.ExtraAvailableLayers.Add(entry.Key);
            }
        }
    }
}
