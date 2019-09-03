namespace Aurora.Profiles.Metro_Last_Light
{
    public class MetroLL : Application
    {
        public MetroLL()
            : base(new LightEventConfig { Name = "Metro: Last Light", ID = "MetroLL", ProcessNames = new[] { "metroll.exe" }, SettingsType = typeof(FirstTimeApplicationSettings), ProfileType = typeof(MetroLLProfile), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_Generic(), IconURI = "Resources/metro_ll_48x48.png" })
        {

        }
    }
}
