using Aurora.Settings;
using System;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using Newtonsoft.Json;
using Aurora.Settings.Layers;

namespace Aurora.Profiles.Dota_2
{
    public class Dota2ProfileManager : ProfileManager
    {
        public Dota2ProfileManager()
            : base("Dota 2", "dota2", "dota2.exe", typeof(Dota2Settings), typeof(Control_Dota2), new GameEvent_Dota2())
        {
            AvailableLayers.Add(LayerType.Dota2Background);
            AvailableLayers.Add(LayerType.Dota2Respawn);
            AvailableLayers.Add(LayerType.Dota2Abilities);
            AvailableLayers.Add(LayerType.Dota2Items);
            AvailableLayers.Add(LayerType.Dota2HeroAbiltiyEffects);
            AvailableLayers.Add(LayerType.Dota2Killstreak);
            IconURI = "Resources/dota2_64x64.png";
        }
    }
}
