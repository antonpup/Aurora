using Aurora.Profiles.Minecraft.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Discord
{

    public class Discord : Application {

        public Discord() : base(new LightEventConfig
        {
            Name = "Discord",
            ID = "discord",
            ProcessNames = new[] { "Discord.exe", "DiscordPTB.exe", "DiscordCanary.exe" },
            ProfileType = typeof(DiscordProfile),
            OverviewControlType = typeof(Control_Discord),
            GameStateType = typeof(GSI.GameState_Discord),
            Event = new GameEvent_Generic(),
            IconURI = "Resources/betterdiscord.png",
            EnableByDefault = false
        }) { }
    }
}
