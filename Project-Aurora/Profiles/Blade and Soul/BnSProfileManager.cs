using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Blade_and_Soul
{
    public class BnSProfileManager : ProfileManager
    {
        public BnSProfileManager()
            : base("Blade and Soul", "BnS", "client.exe", typeof(BnSSettings), typeof(Control_BnS), new GameEvent_BnS())
        {
            IconURI = "Resources/bns_48x48.png";
        }
    }
}
