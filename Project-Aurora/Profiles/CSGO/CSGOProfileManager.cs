using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
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
            : base("CS:GO", "csgo", "csgo.exe", typeof(CSGOSettings), typeof(Control_CSGO), new GameEvent_CSGO())
        {
            AvailableLayers.Add(LayerType.CSGOBackground);
            AvailableLayers.Add(LayerType.CSGOBomb);
            AvailableLayers.Add(LayerType.CSGOKillsIndicator);
            AvailableLayers.Add(LayerType.CSGOBurning);
            AvailableLayers.Add(LayerType.CSGOFlashbang);
            AvailableLayers.Add(LayerType.CSGOTyping);

            IconURI = "Resources/csgo_64x64.png";
        }
    }
}
