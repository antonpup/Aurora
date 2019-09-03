namespace Aurora.Profiles.Desktop
{
    public class Desktop : Application
    {
        public Desktop()
            : base(new LightEventConfig { Name = "Desktop", ID = "desktop", ProfileType = typeof(DesktopProfile), GameStateType = typeof(GameState), Event = new Event_Desktop(), IconURI = "Resources/desktop_icon.png" })
        {
        }
    }
}
