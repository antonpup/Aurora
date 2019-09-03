namespace Aurora.Profiles.Factorio
{
    public class Factorio : Application
    {
        public Factorio()
            : base(new LightEventConfig { Name = "Factorio", ID = "Factorio", ProcessNames = new[] { "factorio.exe" }, ProfileType = typeof(WrapperProfile), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_Generic(), IconURI = "Resources/factorio_64x64.png" })
        {
            Config.ExtraAvailableLayers.Add("WrapperLights");
        }
    }
}
