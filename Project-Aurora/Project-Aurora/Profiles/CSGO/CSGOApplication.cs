using Aurora.Profiles.CSGO.Layers;
using Aurora.Settings;

namespace Aurora.Profiles.CSGO;

public class CSGO : Application
{
    public CSGO()
        : base(new LightEventConfig {
            Name = "CS2",
            ID = "csgo",
            AppID = "730",
            ProcessNames = new[] { "csgo.exe", "cs2.exe" },
            SettingsType = typeof(FirstTimeApplicationSettings),
            ProfileType = typeof(CSGOProfile),
            OverviewControlType = typeof(Control_CSGO),
            GameStateType = typeof(GSI.GameState_CSGO),
            IconURI = "Resources/cs2.png"
        })
    {
        AllowLayer<CSGOBackgroundLayerHandler>();
        AllowLayer<CSGOBombLayerHandler>();
        AllowLayer<CSGOKillIndicatorLayerHandler>();
        AllowLayer<CSGOBurningLayerHandler>();
        AllowLayer<CSGOTypingIndicatorLayerHandler>();
        AllowLayer<CSGOWinningTeamLayerHandler>();
        AllowLayer<CSGODeathLayerHandler>();
    }
}