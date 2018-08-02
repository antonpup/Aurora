using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Minecraft {

    public class Minecraft : Application {

        public Minecraft() : base(new LightEventConfig {
            Name = "Minecraft",
            ID = "minecraft",
            ProcessNames = new[] { "java.exe" },
            ProfileType = typeof(MinecraftProfile),
            OverviewControlType = typeof(Control_Minecraft),
            GameStateType = typeof(GSI.GameState_Minecraft),
            Event = new GameEvent_Generic(),
            IconURI = "Resources/minecraft_128x128.png"
        }) { }
    }
}
