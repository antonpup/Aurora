using Aurora.Profiles.ETS2.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.ETS2 {

    public class ETS2 : Application {

        public ETS2() : base(new LightEventConfig {
            Name = "Euro Truck Simulator 2",
            ID = "ets2",
            AppID = "227300",
            ProcessNames = new[] { "eurotrucks2.exe" },
            SettingsType = typeof(Aurora.Settings.FirstTimeApplicationSettings),
            ProfileType = typeof(ETS2Profile),
            OverviewControlType = typeof(Control_ETS2),
            GameStateType = typeof(GSI.GameState_ETS2),
            Event = new GameEvent_ETS2("eurotrucks2"),
            IconURI = "Resources/ets2_64x64.png"
        }) {

            List<LayerHandlerEntry> ets2Layers = new List<LayerHandlerEntry> {
                new LayerHandlerEntry("ETS2BlinkerIndicator", "ETS2 Blinker", typeof(ETS2BlinkerLayerHandler)),
                new LayerHandlerEntry("ETS2Beacon", "ETS2 Beacon", typeof(ETS2BeaconLayerHandler))
            };

            Global.LightingStateManager.RegisterLayerHandlers(ets2Layers, false);
            foreach (var layer in ets2Layers)
                Config.ExtraAvailableLayers.Add(layer.Key);
        }
    }
}
