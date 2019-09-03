namespace Aurora.Profiles.Blade_and_Soul
{
    public class BnS : Application
    {
        public BnS()
            : base(new LightEventConfig { Name = "Blade and Soul", ID = "BnS", ProcessNames = new[] { "client.exe" }, ProfileType = typeof(WrapperProfile), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_Generic(), IconURI = "Resources/bns_48x48.png" })
        {
            Config.ExtraAvailableLayers.Add("WrapperLights");
        }
    }
}
