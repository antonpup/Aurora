using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Terraria {
    public class Terraria : Application {
        public Terraria() : base(new LightEventConfig
        {
            Name = "Terraria",
            ID = "terraria",
            AppID = "105600",
            ProcessNames = new[] { "Terraria.exe" },
            ProfileType = typeof(TerrariaProfile),
            OverviewControlType = typeof(Control_Terraria),
            GameStateType = typeof(GSI.GameState_Terraria),
            Event = new GameEvent_Generic(),
            IconURI = "Resources/terraria_512x512.png"
        })
        {
        }
    }
}
