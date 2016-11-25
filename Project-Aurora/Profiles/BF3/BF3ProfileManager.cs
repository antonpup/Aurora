using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora.Settings;
using Newtonsoft.Json;

namespace Aurora.Profiles.BF3
{
    public class BF3ProfileManager : ProfileManager
    {
        public BF3ProfileManager()
            : base("Battlefield 3", "bf3", "bf3.exe", typeof(BF3Settings), new GameEvent_BF3())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_BF3();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/bf3_64x64.png", UriKind.Relative));

            return Icon;
        }
    }
}
