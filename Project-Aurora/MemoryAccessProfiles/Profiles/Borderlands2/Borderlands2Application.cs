using Aurora.Profiles;
using MemoryAccessProfiles.Profiles.Borderlands2.GSI;

namespace MemoryAccessProfiles.Profiles.Borderlands2;

public class Borderlands2 : Application
{
    public Borderlands2()
        : base(new LightEventConfig(new Lazy<LightEvent>(() => new GameEvent_Borderlands2()))
        {
            Name = "Borderlands 2",
            ID = "borderlands2",
            ProcessNames = new[] { "borderlands2.exe" },
            ProfileType = typeof(Borderlands2Profile),
            OverviewControlType = typeof(Control_Borderlands2),
            GameStateType = typeof(GameState_Borderlands2),
            IconURI = "Resources/Borderlands2_256x256.png"
        })
    {
        Aurora.Utils.PointerUpdateUtils.MarkAppForUpdate("Borderlands2");
    }
}