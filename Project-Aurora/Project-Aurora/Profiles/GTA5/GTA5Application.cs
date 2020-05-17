using Aurora.Profiles.GTA5.Layers;

namespace Aurora.Profiles.GTA5
{
    public class GTA5 : Application
    {
        public GTA5()
            : base(new LightEventConfig {
                Name = "GTA 5",
                ID = "gta5",
                ProcessNames = new[] { "gta5.exe" },
                ProfileType = typeof(GTA5Profile),
                OverviewControlType = typeof(Control_GTA5),
                GameStateType = typeof(GSI.GameState_GTA5),
                Event = new GameEvent_Generic(),
                IconURI = "Resources/gta5_64x64.png"
            })
        {
            AllowLayer<GTA5BackgroundLayerHandler>();
            AllowLayer<GTA5PoliceSirenLayerHandler>();
        }
    }
}
