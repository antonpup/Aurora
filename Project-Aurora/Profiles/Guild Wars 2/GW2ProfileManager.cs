using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Guild_Wars_2
{
    public class GW2ProfileManager : ProfileManager
    {
        public GW2ProfileManager()
            : base("Guild Wars 2", "GW2", new string[] { "gw2.exe", "gw2-64.exe" }, typeof(GW2Settings), new GameEvent_GW2())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_GW2();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/gw2_48x48.png", UriKind.Relative));

            return Icon;
        }
    }
}
