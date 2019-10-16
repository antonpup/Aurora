using Aurora.Profiles.CloneHero.GSI;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.CloneHero
{
    public class CloneHero : Application
    {
        public CloneHero()
            : base(new LightEventConfig { Name = "Clone Hero", ID = "clonehero", ProcessNames = new[] { "Clone Hero.exe" }, ProfileType = typeof(CloneHeroProfile), OverviewControlType = typeof(Control_CloneHero), GameStateType = typeof(GameState_CloneHero), Event = new GameEvent_CloneHero(), IconURI = "Resources/ch_128x128.png" })
        {
            Utils.PointerUpdateUtils.MarkAppForUpdate("CloneHero");
        }
    }
}
