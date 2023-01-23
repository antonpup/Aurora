using System;

namespace Aurora.Profiles.Desktop;

public class Desktop : Application {
    public Desktop() : base(new LightEventConfig(new Lazy<LightEvent>(() => new Event_Desktop())) {
        Name = "Desktop",
        ID = "desktop",
        ProfileType = typeof(DesktopProfile),
        OverviewControlType = typeof(Control_Desktop),
        GameStateType = typeof(EmptyGameState),
        IconURI = "Resources/desktop_icon.png"
    }) { }
}