using Aurora.Profiles;
using MemoryAccessProfiles.Profiles.ResidentEvil2.GSI;
using MemoryAccessProfiles.Profiles.ResidentEvil2.Layers;

namespace MemoryAccessProfiles.Profiles.ResidentEvil2;

public class ResidentEvil2 : Application
{
    public ResidentEvil2()
        : base(new LightEventConfig(new Lazy<LightEvent>(() => new GameEvent_ResidentEvil2())) {
            Name = "Resident Evil 2",
            ID = "residentevil2",
            ProcessNames = new[] { "re2.exe" },
            ProfileType = typeof(ResidentEvil2Profile),
            OverviewControlType = typeof(Control_ResidentEvil2),
            GameStateType = typeof(GameState_ResidentEvil2),
            IconURI = "Resources/re2_256x256.png"
        })
    {
        Aurora.Utils.PointerUpdateUtils.MarkAppForUpdate("ResidentEvil2");
        AllowLayer<ResidentEvil2HealthLayerHandler>();
        AllowLayer<ResidentEvil2RankLayerHandler>();
    }
}