using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.TModLoader
{
    public class TModLoader : Application {
        public TModLoader() : base(new LightEventConfig
        {
            Name = "TModLoader",
            ID = "tmodloader",
            AppID = "1281930",
            ProcessNames = new[] { "tModLoader.exe" },
            ProfileType = typeof(TModLoaderProfile),
            OverviewControlType = typeof(Control_TModLoader),
            GameStateType = typeof(GSI.GameState_TModLoader),
            Event = new GameEvent_Generic(),
            IconURI = "Resources/tmodloader.png"
        })
        {
        }
    }
}
