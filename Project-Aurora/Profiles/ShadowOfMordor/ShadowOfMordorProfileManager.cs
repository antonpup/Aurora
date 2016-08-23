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
            : base("Middle-earth: Shadow of Mordor", "ShadowOfMordor", "shadowofmordor.exe", typeof(ShadowOfMordorSettings), new GameEvent_ShadowOfMordor())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_ShadowOfMordor();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/shadow_of_mordor_64x64.png", UriKind.Relative));

            return Icon;
        }
    }
}
