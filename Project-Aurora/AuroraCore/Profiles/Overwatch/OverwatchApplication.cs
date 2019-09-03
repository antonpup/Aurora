namespace Aurora.Profiles.Overwatch
{
    public class Overwatch : Application
    {
        public Overwatch()
            : base(new LightEventConfig { Name = "Overwatch", ID = "overwatch", ProcessNames = new[] { "overwatch.exe" }, ProfileType = typeof(RazerChromaProfile), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_Generic(), IconURI = "Resources/overwatch_icon.png" })
        {
            Config.ExtraAvailableLayers.Add("WrapperLights");
        }
    }
}
