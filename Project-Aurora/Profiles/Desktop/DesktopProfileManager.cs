using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Desktop
{
    public class DesktopProfileManager : ProfileManager
    {
        public DesktopProfileManager()
            : base("Desktop", "desktop", "", typeof(DesktopSettings), new Event_Desktop())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_Desktop();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/desktop_icon.png", UriKind.Relative));

            return Icon;
        }
    }
}
