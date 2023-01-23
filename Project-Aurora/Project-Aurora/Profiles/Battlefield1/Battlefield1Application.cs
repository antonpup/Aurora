using Aurora.Settings.Layers;

namespace Aurora.Profiles.Battlefield1;

public class Battlefield1 : Application
{
    public Battlefield1()
        : base(new LightEventConfig {
            Name = "Battlefield 1",
            ID = "Battlefield1",
            ProcessNames = new[] { "bf1.exe" },
            ProfileType = typeof(WrapperProfile),
            OverviewControlType = typeof(Control_Battlefield1),
            GameStateType = typeof(GameState_Wrapper),
            IconURI = "Resources/bf1_128x128.png"
        })
    {
        AllowLayer<WrapperLightsLayerHandler>();
    }
}