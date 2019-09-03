namespace Aurora.Profiles.XCOM
{
    public class XCOM : Application
    {
        public XCOM()
            : base(new LightEventConfig { Name = "XCOM: Enemy Unknown", ID = "XCOM", ProcessNames = new[] { "xcomgame.exe" }, SettingsType = typeof(FirstTimeApplicationSettings), ProfileType = typeof(XCOMProfile), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_Generic(), IconURI = "Resources/xcom_64x64.png" })
        {
            Config.ExtraAvailableLayers.Add("WrapperLights");
        }
    }
}
