namespace Aurora.Profiles.Serious_Sam_3
{
    public class SSam3 : Application
    {
        public SSam3()
            : base(new LightEventConfig { Name = "Serious Sam 3", ID = "ssam3", ProcessNames = new string[] { "sam3.exe", "sam3_unrestricted.exe" }, SettingsType = typeof(FirstTimeApplicationSettings), ProfileType = typeof(SSam3Profile), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_Generic(), IconURI = "Resources/ssam3_48x48.png" })
        {
            Config.ExtraAvailableLayers.Add("WrapperLights");
        }
    }
}
