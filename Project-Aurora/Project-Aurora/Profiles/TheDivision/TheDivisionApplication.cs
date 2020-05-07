using Aurora.Settings.Layers;

namespace Aurora.Profiles.TheDivision
{
    public class TheDivision : Application
    {
        public TheDivision()
            : base(new LightEventConfig {
                Name = "The Division",
                ID = "the_division",
                ProcessNames = new[] { "thedivision.exe" },
                ProfileType = typeof(WrapperProfile),
                OverviewControlType = typeof(Control_TheDivision),
                GameStateType = typeof(GameState_Wrapper),
                Event = new GameEvent_Generic(),
                IconURI = "Resources/division_64x64.png"
            })
        {
            AllowLayer<WrapperLightsLayerHandler>();
        }
    }
}
