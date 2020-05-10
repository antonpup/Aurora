using Aurora.Profiles.CSGO.Layers;
using Aurora.Settings;

namespace Aurora.Profiles.CSGO
{
    public class CSGO : Application
    {
        public CSGO()
            : base(new LightEventConfig {
                Name = "CS:GO",
                ID = "csgo",
                AppID = "730",
                ProcessNames = new[] { "csgo.exe" },
                SettingsType = typeof(FirstTimeApplicationSettings),
                ProfileType = typeof(CSGOProfile),
                OverviewControlType = typeof(Control_CSGO),
                GameStateType = typeof(GSI.GameState_CSGO),
                Event = new GameEvent_Generic(),
                IconURI = "Resources/csgo_64x64.png"
            })
        {
            AllowLayer<CSGOBackgroundLayerHandler>();
            AllowLayer<CSGOBombLayerHandler>();
            AllowLayer<CSGOKillIndicatorLayerHandler>();
            AllowLayer<CSGOBurningLayerHandler>();
            AllowLayer<CSGOFlashbangLayerHandler>();
            AllowLayer<CSGOTypingIndicatorLayerHandler>();
            AllowLayer<CSGOWinningTeamLayerHandler>();
            AllowLayer<CSGODeathLayerHandler>();
        }
    }
}
