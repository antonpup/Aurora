using Aurora.Profiles.CloneHero.GSI;

namespace Aurora.Profiles.CloneHero
{
    public class CloneHero : Application
    {
        public CloneHero()
            : base(new LightEventConfig { Name = "Clone Hero", ID = "clonehero", ProcessNames = new[] { "Clone Hero.exe" }, ProfileType = typeof(CloneHeroProfile), GameStateType = typeof(GameState_CloneHero), Event = new GameEvent_CloneHero(), IconURI = "Resources/ch_128x128.png" })
        {
            Utils.PointerUpdateUtils.MarkAppForUpdate("CloneHero");
        }
    }
}
