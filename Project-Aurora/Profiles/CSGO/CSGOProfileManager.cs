using Aurora.Settings;
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
            : base("CS:GO", "csgo", "csgo.exe", typeof(CSGOSettings), new GameEvent_CSGO())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_CSGO();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/csgo_64x64.png", UriKind.Relative));

            return Icon;
        }
    }
}
