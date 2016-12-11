using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.TheDivision
{
    public class TheDivisionProfileManager : ProfileManager
    {
        public TheDivisionProfileManager()
            : base("The Division", "the_division", "thedivision.exe", typeof(TheDivisionSettings), typeof(Control_TheDivision), new GameEvent_TheDivision())
        {
            IconURI = "Resources/division_64x64.png";
        }
    }
}
