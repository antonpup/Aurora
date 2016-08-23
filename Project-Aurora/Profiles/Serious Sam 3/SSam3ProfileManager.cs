using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Serious_Sam_3
{
    public class SSam3ProfileManager : ProfileManager
    {
        public SSam3ProfileManager()
            : base("Serious Sam 3", "ssam3", new string[] { "sam3.exe", "sam3_unrestricted.exe" }, typeof(SSam3Settings), new GameEvent_SSam3())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_SSam3();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/ssam3_48x48.png", UriKind.Relative));

            return Icon;
        }
    }
}
