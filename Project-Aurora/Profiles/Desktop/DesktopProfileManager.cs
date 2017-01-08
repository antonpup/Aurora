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
            : base("Desktop", "desktop", "", typeof(DesktopSettings), typeof(Control_Desktop), new Event_Desktop())
        {
            IconURI = "Resources/desktop_icon.png";
        }
    }
}
