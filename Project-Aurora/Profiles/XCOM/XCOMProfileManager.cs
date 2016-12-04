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
            : base("XCOM: Enemy Unknown", "XCOM", "xcomgame.exe", typeof(XCOMSettings), typeof(Control_XCOM), new GameEvent_XCOM())
        {
            IconURI = "Resources/xcom_64x64.png";
        }
    }
}
