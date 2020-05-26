using Aurora.Profiles.EliteDangerous.Layers;

namespace Aurora.Profiles.EliteDangerous
{
    public class EliteDangerous : Application
    {
        public EliteDangerous()
            : base(new LightEventConfig
            {
                Name = "Elite: Dangerous",
                ID = "EliteDangerous",
                ProcessNames = new[] { "EliteDangerous64.exe" },
                UpdateInterval = 16,
                SettingsType = typeof(EliteDangerousSettings),
                ProfileType = typeof(EliteDangerousProfile),
                OverviewControlType = typeof(Control_EliteDangerous),
                GameStateType = typeof(GSI.GameState_EliteDangerous),
                Event = new GameEvent_EliteDangerous(),
                IconURI = "Resources/elite_dangerous_256x256.png"
            })
        {

            AllowLayer<EliteDangerousBackgroundLayerHandler>();
            AllowLayer<EliteDangerousKeyBindsLayerHandler>();
            AllowLayer<EliteDangerousAnimationLayerHandler>();
        }
    }
}