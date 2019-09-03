//using Aurora.Profiles.Subnautica.Layers;
using System.Collections.Generic;

namespace Aurora.Profiles.Subnautica
{

    public class Subnautica : Application
    {

        public Subnautica() : base(new LightEventConfig
        {
            Name = "Subnautica",
            ID = "subnautica",
            AppID = "264710",
            ProcessNames = new[] { "Subnautica.exe" },
            ProfileType = typeof(SubnauticaProfile),
            GameStateType = typeof(GSI.GameState_Subnautica),
            Event = new GameEvent_Generic(),
            IconURI = "Resources/Subnautica.png"
        })
        {

            List<LayerHandlerEntry> SubnauticaLayers = new List<LayerHandlerEntry>
            {
                //    new LayerHandlerEntry("SubnauticaNotificationLayer", "Subnautica Notification Layer", typeof(SubnauticaNotificationLayerHandler))
            };

            // AuroraCore.Instance.LightingStateManager.RegisterLayerHandlers(SubnauticaLayers, false);
            // foreach (var layer in SubnauticaLayers)
            //     Config.ExtraAvailableLayers.Add(layer.Key);
        }

    }
}
