using Aurora.Profiles.Aurora_Wrapper;
using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Overwatch
{
    public class OverwatchProfileManager : ProfileManager
    {
        public OverwatchProfileManager()
            : base("Overwatch", "overwatch", "overwatch.exe", typeof(OverwatchSettings), new GameEvent_Overwatch())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_Overwatch();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/overwatch_icon.png", UriKind.Relative));

            return Icon;
        }
    }
}
