using Aurora.Profiles.Dishonored.GSI;
using System;

namespace Aurora.Profiles.Dishonored;

public class Dishonored : Application
{
    public Dishonored()
        : base(new LightEventConfig(new Lazy<LightEvent>(() => new GameEvent_Dishonored()))
        {
            Name = "Dishonored",
            ID = "Dishonored",
            ProcessNames = new[] { "Dishonored.exe" },
            ProfileType = typeof(DishonoredProfile),
            OverviewControlType = typeof(Control_Dishonored),
            GameStateType = typeof(GameState_Dishonored),
            IconURI = "Resources/dh_128x128.png"
        })
    {
        Utils.PointerUpdateUtils.MarkAppForUpdate("Dishonored");
    }
}