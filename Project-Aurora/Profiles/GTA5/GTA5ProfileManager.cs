using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.GTA5
{
    public class GTA5ProfileManager : ProfileManager
    {
        public GTA5ProfileManager()
            : base("GTA 5", "gta5", "gta5.exe", typeof(GTA5Settings), typeof(Control_GTA5), new GameEvent_GTA5())
        {
            AvailableLayers.Add(LayerType.GTA5Background);
            AvailableLayers.Add(LayerType.GTA5PoliceSiren);

            IconURI = "Resources/gta5_64x64.png";
        }
    }
}
