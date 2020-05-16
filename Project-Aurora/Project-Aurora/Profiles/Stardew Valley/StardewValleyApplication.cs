using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.StardewValley
{
    public class StardewValley : Application {
        public StardewValley() : base(new LightEventConfig
        {
            Name = "Stardew Valley SMAPI",
            ID = "stardew_valley",
            AppID = "413150",
            ProcessNames = new[] { "StardewModdingAPI.exe" },
            ProfileType = typeof(StardewValleyProfile),
            OverviewControlType = typeof(Control_StardewValley),
            GameStateType = typeof(GSI.GameState_StardewValley),
            Event = new GameEvent_Generic(),
            IconURI = "Resources/stardew_valley_32x32.png"
        })
        {
        }
    }
}
