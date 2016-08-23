using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Metro_Last_Light
{
    public class MetroLLProfileManager : ProfileManager
    {
        public MetroLLProfileManager()
            : base("Metro: Last Light", "MetroLL", "metroll.exe", typeof(MetroLLSettings), new GameEvent_MetroLL())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_MetroLL();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/metro_ll_48x48.png", UriKind.Relative));

            return Icon;
        }
    }
}
