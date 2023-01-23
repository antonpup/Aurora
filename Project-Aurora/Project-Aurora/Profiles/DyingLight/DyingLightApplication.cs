using Aurora.Settings;
using Aurora.Settings.Layers;

namespace Aurora.Profiles.DyingLight;

public class DyingLight : Application
{
    public DyingLight()
        : base(new LightEventConfig {
            Name = "Dying Light",
            ID = "DyingLight",
            ProcessNames = new[] { "DyingLightGame.exe" },
            SettingsType = typeof(FirstTimeApplicationSettings),
            ProfileType = typeof(DyingLightProfile),
            OverviewControlType = typeof(Control_DyingLight),
            GameStateType = typeof(GameState_Wrapper),
            IconURI = "Resources/dl_128x128.png"
        })
    {
        AllowLayer<WrapperLightsLayerHandler>();
    }
}