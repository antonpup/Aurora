using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora.Settings;
using Newtonsoft.Json;

namespace Aurora.Profiles.BF3
{
    public class BF3ProfileManager : ProfileManager
    {
        public BF3ProfileManager()
            : base("Battlefield 3", "bf3", "bf3.exe", typeof(BF3Settings), typeof(Control_BF3), new GameEvent_BF3())
        {
            IconURI = "Resources/bf3_64x64.png";
        }
    }
}
