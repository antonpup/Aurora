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
            : base("The Division", "the_division", "thedivision.exe", typeof(TheDivisionSettings), new GameEvent_TheDivision())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_TheDivision();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/division_64x64.png", UriKind.Relative));

            return Icon;
        }
    }
}
