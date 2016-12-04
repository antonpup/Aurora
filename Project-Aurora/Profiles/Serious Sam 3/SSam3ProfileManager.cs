using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Serious_Sam_3
{
    public class SSam3ProfileManager : ProfileManager
    {
        public SSam3ProfileManager()
            : base("Serious Sam 3", "ssam3", new string[] { "sam3.exe", "sam3_unrestricted.exe" }, typeof(SSam3Settings), typeof(Control_SSam3), new GameEvent_SSam3())
        {
            IconURI = "Resources/ssam3_48x48.png";
        }
    }
}
