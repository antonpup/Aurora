using Aurora.Profiles.Aurora_Wrapper;
using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.WormsWMD
{
    public class WormsWMDProfileManager : ProfileManager
    {
        public WormsWMDProfileManager()
            : base("Worms W.M.D", "worms_wmd", "Worms W.M.D.exe", typeof(WormsWMDSettings), typeof(Control_WormsWMD), new GameEvent_WormsWMD())
        {
            IconURI = "Resources/worms_wmd.png";
        }
    }
}
