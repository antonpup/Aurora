using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.TheTalosPrinciple
{
    public class TalosPrincipleProfileManager : ProfileManager
    {
        public TalosPrincipleProfileManager()
            : base("The Talos Principle", "the_talos_principle", new string[] { "talos.exe", "talos_unrestricted.exe" }, typeof(TalosPrincipleSettings), new GameEvent_TalosPrinciple())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_TalosPrinciple();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/talosprinciple_64x64.png", UriKind.Relative));

            return Icon;
        }
    }
}
