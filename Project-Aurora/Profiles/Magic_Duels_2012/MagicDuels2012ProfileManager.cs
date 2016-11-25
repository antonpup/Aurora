using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora.Settings;
using Newtonsoft.Json;

namespace Aurora.Profiles.Magic_Duels_2012
{
    public class MagicDuels2012ProfileManager : ProfileManager
    {

        public MagicDuels2012ProfileManager()
            : base("Magic: The Gathering - Duels of the Planeswalkers 2012", "magic_2012", "magic_2012.exe", typeof(MagicDuels2012Settings), new GameEvent_MagicDuels2012())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_MagicDuels2012();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/magic_duels_64x64.png", UriKind.Relative));

            return Icon;
        }
    }
}
