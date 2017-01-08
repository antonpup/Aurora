using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.ShadowOfMordor
{
    public class ShadowOfMordorProfileManager : ProfileManager
    {
        public ShadowOfMordorProfileManager()
            : base("Middle-earth: Shadow of Mordor", "ShadowOfMordor", "shadowofmordor.exe", typeof(ShadowOfMordorSettings), typeof(Control_ShadowOfMordor), new GameEvent_ShadowOfMordor())
        {
            IconURI = "Resources/shadow_of_mordor_64x64.png";
        }
    }
}
