using Aurora.Profiles;
using MemoryAccessProfiles.Profiles.CloneHero.GSI;

namespace MemoryAccessProfiles.Profiles.CloneHero;

public class CloneHero : Application
{
    public CloneHero()
        : base(new LightEventConfig(new Lazy<LightEvent>(() => new GameEvent_CloneHero()))
        {
            Name = "Clone Hero", ID = "clonehero",
            ProcessNames = new[] { "Clone Hero.exe" },
            ProfileType = typeof(CloneHeroProfile),
            OverviewControlType = typeof(Control_CloneHero),
            GameStateType = typeof(GameState_CloneHero),
            IconURI = "Resources/ch_128x128.png"
        })
    {
        Aurora.Utils.PointerUpdateUtils.MarkAppForUpdate("CloneHero");
    }
}