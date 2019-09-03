using Aurora.Profiles.ETS2.Layers;
using System.Collections.Generic;

namespace Aurora.Profiles.ETS2
{

    public class ETS2 : Application
    {

        public ETS2() : base(new LightEventConfig
        {
            Name = "Euro Truck Simulator 2",
            ID = "ets2",
            AppID = "227300",
            ProcessNames = new[] { "eurotrucks2.exe" },
            SettingsType = typeof(FirstTimeApplicationSettings),
            ProfileType = typeof(ETS2Profile),
            GameStateType = typeof(GSI.GameState_ETS2),
            Event = new GameEvent_ETS2("eurotrucks2"),
            IconURI = "Resources/ets2_64x64.png"
        })
        {

            List<LayerHandlerEntry> ets2Layers = new List<LayerHandlerEntry> {
                new LayerHandlerEntry("ETS2BlinkerIndicator", "ETS2 Blinker", typeof(ETS2BlinkerLayerHandler)),
                new LayerHandlerEntry("ETS2Beacon", "ETS2 Beacon", typeof(ETS2BeaconLayerHandler))
            };

            AuroraCore.Instance.LightingStateManager.RegisterLayerHandlers(ets2Layers, false);
            foreach (var layer in ets2Layers)
                Config.ExtraAvailableLayers.Add(layer.Key);
        }
    }
}
