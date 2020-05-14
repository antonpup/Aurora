using Aurora.Settings.Layers;

namespace Aurora.Profiles.QuakeChampions
{
    public class QuakeChampions : Application
    {
        public QuakeChampions()
            : base(new LightEventConfig {
                Name = "Quake Champions",
                ID = "QuakeChampions",
                ProcessNames = new[] { "QuakeChampions.exe"},
                ProfileType = typeof(QuakeChampionsProfile),
                OverviewControlType = typeof(Control_QuakeChampions),
                GameStateType = typeof(GameState_Wrapper),
                Event = new GameEvent_Generic(),
                IconURI = "Resources/quakechampions_195x195.png"
            })
        {
            AllowLayer<WrapperLightsLayerHandler>();
        }
    }
}
