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
            : base("Dota 2", "dota2", "dota2.exe", typeof(Dota2Settings), new GameEvent_Dota2())
        {
            AvailableLayers.Add(LayerType.Dota2Background);
            AvailableLayers.Add(LayerType.Dota2Respawn);
            AvailableLayers.Add(LayerType.Dota2Abilities);
            AvailableLayers.Add(LayerType.Dota2Items);
            AvailableLayers.Add(LayerType.Dota2HeroAbiltiyEffects);
            AvailableLayers.Add(LayerType.Dota2Killstreak);
        }

        public override UserControl GetUserControl()
        {
            if(Control == null)
                Control = new Control_Dota2();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/dota2_64x64.png", UriKind.Relative));

            return Icon;
        }
    }
}
