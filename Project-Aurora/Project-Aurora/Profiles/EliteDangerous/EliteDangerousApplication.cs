using Aurora.Settings;
using Aurora.Profiles.EliteDangerous.Layers;
using System.Collections.Generic;
using CSScriptLibrary;

namespace Aurora.Profiles.EliteDangerous
{
    public class EliteDangerous : Application
    {
        public EliteDangerous()
            : base(new LightEventConfig
            {
                Name = "Elite: Dangerous",
                ID = "EliteDangerous",
                ProcessNames = new[] { "EliteDangerous64.exe" },
                UpdateInterval = 16,
                SettingsType = typeof(EliteDangerousSettings),
                ProfileType = typeof(EliteDangerousProfile),
                OverviewControlType = typeof(Control_EliteDangerous),
                GameStateType = typeof(GSI.GameState_EliteDangerous),
                Event = new GameEvent_EliteDangerous(),
                IconURI = "Resources/elite_dangerous_256x256.png"
            })
        {

            var extra = new List<LayerHandlerEntry>
            {
                new LayerHandlerEntry("EliteDangerousBackground", "Elite: Dangerous Background Layer", typeof(EliteDangerousBackgroundLayerHandler)),
                new LayerHandlerEntry("EliteDangerousKeyBinds", "Elite: Dangerous Key Binds Layer", typeof(EliteDangerousKeyBindsLayerHandler)),
                new LayerHandlerEntry("EliteDangerousAnimations", "Elite: Dangerous Animation Layer", typeof(EliteDangerousAnimationLayerHandler)),
            };

            Global.LightingStateManager.RegisterLayerHandlers(extra, false);

            foreach (var entry in extra)
            {
                Config.ExtraAvailableLayers.Add(entry.Key);
            }
        }
    }
}