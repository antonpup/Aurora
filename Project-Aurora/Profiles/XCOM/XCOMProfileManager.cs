using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.XCOM
{
    public class XCOMProfileManager : ProfileManager
    {
        public XCOMProfileManager()
            : base("XCOM: Enemy Unknown", "XCOM", "xcomgame.exe", typeof(XCOMSettings), new GameEvent_XCOM())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_XCOM();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/xcom_64x64.png", UriKind.Relative));

            return Icon;
        }
    }
}
