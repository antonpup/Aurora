using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.DiscoDodgeball
{
    public class DiscoDodgeballProfileManager : ProfileManager
    {
        public DiscoDodgeballProfileManager()
            : base("Robot Roller-Derby Disco Dodgeball", "DiscoDodgeball", "disco dodgeball.exe", typeof(DiscoDodgeballSettings), typeof(Control_DiscoDodgeball), new GameEvent_DiscoDodgeball())
        {
            IconURI = "Resources/disco_dodgeball_32x32.png";
        }
    }
}
