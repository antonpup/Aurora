using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Evolve
{
    public class EvolveProfileManager : ProfileManager
    {
        public EvolveProfileManager()
            : base("Evolve Stage 2", "Evolve", "evolve.exe", typeof(EvolveSettings), new GameEvent_Evolve())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_Evolve();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/evolve_48x48.png", UriKind.Relative));

            return Icon;
        }
    }
}
