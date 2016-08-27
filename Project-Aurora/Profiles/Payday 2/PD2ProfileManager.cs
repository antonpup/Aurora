using Aurora.Profiles.Payday_2.GSI;
using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Payday_2
{
    public class PD2ProfileManager : ProfileManager
    {
        public PD2ProfileManager()
            : base("Payday 2", "pd2", "payday2_win32_release.exe", typeof(PD2Settings), new GameEvent_PD2(), typeof(GameState_PD2))
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_PD2();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/pd2_64x64.png", UriKind.Relative));

            return Icon;
        }
    }
}
