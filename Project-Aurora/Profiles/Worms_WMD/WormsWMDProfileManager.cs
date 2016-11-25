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
            : base("Worms W.M.D", "worms_wmd", "Worms W.M.D.exe", typeof(WormsWMDSettings), new GameEvent_WormsWMD())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_WormsWMD();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/worms_wmd.png", UriKind.Relative));

            return Icon;
        }
    }
}
