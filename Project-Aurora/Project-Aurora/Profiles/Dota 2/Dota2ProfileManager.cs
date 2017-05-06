using Aurora.Settings;
using Aurora.Profiles.Dota_2.Layers;
using System;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using Newtonsoft.Json;
using Aurora.Settings.Layers;
using System.Collections.Generic;

namespace Aurora.Profiles.Dota_2
{
    public class Dota2ProfileManager : ProfileManager
    {
        public Dota2ProfileManager()
            : base(new LightEventConfig {
                Name = "Dota 2",
                ID = "dota2",
                AppID = "570",
                ProcessNames = new[] { "dota2.exe" },
                SettingsType = typeof(Dota2Settings),
                OverviewControlType = typeof(Control_Dota2),
                GameStateType = typeof(GSI.GameState_Dota2),
                Event = new GameEvent_Dota2(),
                IconURI = "Resources/dota2_64x64.png"
            })
        {

            var extra = new List<LayerHandlerEntry>
            {
                new LayerHandlerEntry("Dota2Background", "Dota 2 Background Layer", typeof(Dota2BackgroundLayerHandler)),
                new LayerHandlerEntry("Dota2Respawn", "Dota 2 Respawn Layer", typeof(Dota2RespawnLayerHandler)),
                new LayerHandlerEntry("Dota2Abilities", "Dota 2 Abilities Layer", typeof(Dota2AbilityLayerHandler)),
                new LayerHandlerEntry("Dota2Items", "Dota 2 Items Layer", typeof(Dota2ItemLayerHandler)),
                new LayerHandlerEntry("Dota2HeroAbilityEffects", "Dota 2 Hero Ability Effects Layer", typeof(Dota2HeroAbilityEffectsLayerHandler)),
                new LayerHandlerEntry("Dota2Killstreak", "Dota 2 Killstreak Layer", typeof(Dota2KillstreakLayerHandler)),
            };

            Global.ProfilesManager.RegisterLayerHandlers(extra, false);

            foreach (var entry in extra)
            {
                Config.ExtraAvailableLayers.Add(entry.Key);
            }
        }
    }
}
