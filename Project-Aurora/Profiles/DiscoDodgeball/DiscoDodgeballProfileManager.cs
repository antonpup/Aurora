using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.DiscoDodgeball
{
    public class DiscoDodgeballProfileManager : ProfileManager
    {
        public DiscoDodgeballProfileManager()
            : base("Robot Roller-Derby Disco Dodgeball", "DiscoDodgeball", "disco dodgeball.exe", typeof(DiscoDodgeballSettings), new GameEvent_DiscoDodgeball())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_DiscoDodgeball();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/disco_dodgeball_32x32.png", UriKind.Relative));

            return Icon;
        }
    }
}
