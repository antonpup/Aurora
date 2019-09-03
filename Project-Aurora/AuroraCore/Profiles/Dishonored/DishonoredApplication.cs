using Aurora.Profiles.Dishonored.GSI;

namespace Aurora.Profiles.Dishonored
{
    public class Dishonored : Application
    {
        public Dishonored()
            : base(new LightEventConfig { Name = "Dishonored", ID = "Dishonored", ProcessNames = new[] { "Dishonored.exe" }, ProfileType = typeof(DishonoredProfile), GameStateType = typeof(GameState_Dishonored), Event = new GameEvent_Dishonored(), IconURI = "Resources/dh_128x128.png" })
        {
            Utils.PointerUpdateUtils.MarkAppForUpdate("Dishonored");
        }
    }
}
