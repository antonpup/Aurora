using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.HotlineMiami
{
    public class HMProfileManager : ProfileManager
    {
        public HMProfileManager()
            : base("Hotline Miami", "hotline_miami", "hotlinegl.exe", typeof(HMSettings), new GameEvent_HM())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_HM();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/hotline_32x32.png", UriKind.Relative));

            return Icon;
        }
    }
}
