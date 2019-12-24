//using Aurora.Profiles.Subnautica.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Subnautica {

    public class Subnautica : Application {

        public Subnautica() : base(new LightEventConfig
        {
            Name = "Subnautica",
            ID = "subnautica",
            AppID = "264710",
            ProcessNames = new[] { "Subnautica.exe" },
            ProfileType = typeof(SubnauticaProfile),
            OverviewControlType = typeof(Control_Subnautica),
            GameStateType = typeof(GSI.GameState_Subnautica),
            Event = new GameEvent_Generic(),
            IconURI = "Resources/Subnautica.png"
        })
        {

            List<LayerHandlerEntry> SubnauticaLayers = new List<LayerHandlerEntry> {
                //    new LayerHandlerEntry("SubnauticaNotificationLayer", "Subnautica Notification Layer", typeof(SubnauticaNotificationLayerHandler))
            };
        
           // Global.LightingStateManager.RegisterLayerHandlers(SubnauticaLayers, false);
           // foreach (var layer in SubnauticaLayers)
           //     Config.ExtraAvailableLayers.Add(layer.Key);
        }
        
    }
}
