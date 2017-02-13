using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Profiles.CSGO.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.CSGO
{
    public class CSGOProfileManager : ProfileManager
    {
        public CSGOProfileManager()
            : base(new LightEventConfig { Name = "CS:GO", ID = "csgo", AppID = "730", ProcessNames = new[] { "csgo.exe" }, SettingsType = typeof(CSGOSettings), OverviewControlType = typeof(Control_CSGO), GameStateType = typeof(GSI.GameState_CSGO), Event = new GameEvent_CSGO(), IconURI = "Resources/csgo_64x64.png" })
        {
            //UpdateInterval = 33;

            var extra = new List<ProfilesManager.LayerHandlerEntry> {
                new ProfilesManager.LayerHandlerEntry("CSGOBackground", "CSGO Background Layer", typeof(CSGOBackgroundLayerHandler)),
                new ProfilesManager.LayerHandlerEntry("CSGOBomb", "CSGO Bomb Layer", typeof(CSGOBombLayerHandler)),
                new ProfilesManager.LayerHandlerEntry("CSGOKillsIndicator", "CSGO Kills Indicator Layer", typeof(CSGOKillIndicatorLayerHandler)),
                new ProfilesManager.LayerHandlerEntry("CSGOBurning", "CSGO Burning Effect Layer", typeof(CSGOBurningLayerHandler)),
                new ProfilesManager.LayerHandlerEntry("CSGOFlashbang", "CSGO Flashbang Layer", typeof(CSGOFlashbangLayerHandler)),
                new ProfilesManager.LayerHandlerEntry("CSGOTyping", "CSGO Typing Layer", typeof(CSGOTypingIndicatorLayerHandler)),
            };

            Global.ProfilesManager.RegisterLayerHandlers(extra, false);

            foreach (var entry in extra)
            {
                Config.ExtraAvailableLayers.Add(entry.Key);
            }
        }
    }
}
