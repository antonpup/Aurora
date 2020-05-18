namespace Aurora.Profiles.Desktop {
    public class Desktop : Application {
        public Desktop() : base(new LightEventConfig {
            Name = "Desktop",
            ID = "desktop",
            ProfileType = typeof(DesktopProfile),
            OverviewControlType = typeof(Control_Desktop),
            GameStateType = typeof(EmptyGameState),
            Event = new Event_Desktop(),
            IconURI = "Resources/desktop_icon.png"
        }) { }
    }
}
