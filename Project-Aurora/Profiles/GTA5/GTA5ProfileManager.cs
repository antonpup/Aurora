using Aurora.Settings;
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
            : base("GTA 5", "gta5", "gta5.exe", typeof(GTA5Settings), new GameEvent_GTA5())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_GTA5();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/gta5_64x64.png", UriKind.Relative));

            return Icon;
        }
    }
}
