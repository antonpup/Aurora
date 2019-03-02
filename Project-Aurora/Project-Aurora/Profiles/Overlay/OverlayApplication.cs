using System.Collections.Generic;

namespace Aurora.Profiles.Overlay {

    public class Overlay : Application {

        public Overlay() : base(new LightEventConfig {
            Name = "Overlay",
            ID = "overlay",
            ProfileType = typeof(Settings.ApplicationProfile),
            OverviewControlType = typeof(Control_Overlay),
            GameStateType = typeof(GameState),
            Event = new GameEvent_Generic(),
            IconURI = "Resources/overlays_icon.png",
            Type = LightEventType.Overlay
        }) { }
    }
}
