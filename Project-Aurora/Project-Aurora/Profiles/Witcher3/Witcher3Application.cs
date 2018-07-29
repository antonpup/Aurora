using Aurora.Settings;
using Aurora.Profiles.Witcher3.Layers;
using System;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using Newtonsoft.Json;
using Aurora.Settings.Layers;
using System.Collections.Generic;

namespace Aurora.Profiles.Witcher3
{
    public class Witcher3 : Application
    {
        public Witcher3()
            : base(new LightEventConfig
            {
                Name = "The Witcher 3",
                ID = "Witcher3",
                ProcessNames = new[] { "Witcher3.exe" },
                SettingsType = typeof(FirstTimeApplicationSettings),
                ProfileType = typeof(Witcher3Profile),
                OverviewControlType = typeof(Control_Witcher3),
                GameStateType = typeof(GSI.GameState_Witcher3),
                Event = new GameEvent_Witcher3(),
                IconURI = "Resources/Witcher3_256x256.png"
            })
        {

            var extra = new List<LayerHandlerEntry>
            {
                new LayerHandlerEntry("Witcher3Background", "Witcher 3 Background Layer", typeof(Witcher3BackgroundLayerHandler)),
            };

            Global.LightingStateManager.RegisterLayerHandlers(extra, false);

            foreach (var entry in extra)
            {
                Config.ExtraAvailableLayers.Add(entry.Key);
            }
        }
    }
}
