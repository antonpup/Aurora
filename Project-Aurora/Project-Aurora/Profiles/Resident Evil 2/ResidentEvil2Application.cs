using Aurora.Profiles.ResidentEvil2.GSI;
using Aurora.Profiles.ResidentEvil2.Layers;

namespace Aurora.Profiles.ResidentEvil2
{
    public class ResidentEvil2 : Application
    {
        public ResidentEvil2()
            : base(new LightEventConfig {
                Name = "Resident Evil 2",
                ID = "residentevil2",
                ProcessNames = new[] { "re2.exe" },
                ProfileType = typeof(ResidentEvil2Profile),
                OverviewControlType = typeof(Control_ResidentEvil2),
                GameStateType = typeof(GameState_ResidentEvil2),
                Event = new GameEvent_ResidentEvil2(),
                IconURI = "Resources/re2_256x256.png"
            })
        {
            Utils.PointerUpdateUtils.MarkAppForUpdate("ResidentEvil2");
            AllowLayer<ResidentEvil2HealthLayerHandler>();
            AllowLayer<ResidentEvil2RankLayerHandler>();
        }
    }
}
