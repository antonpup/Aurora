using Aurora.Settings;
using Aurora.Profiles.EliteDangerous.Layers;
using System.Collections.Generic;

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
                SettingsType = typeof(FirstTimeApplicationSettings),
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
            };

            Global.LightingStateManager.RegisterLayerHandlers(extra, false);

            foreach (var entry in extra)
            {
                Config.ExtraAvailableLayers.Add(entry.Key);
            }
        }
    }
}