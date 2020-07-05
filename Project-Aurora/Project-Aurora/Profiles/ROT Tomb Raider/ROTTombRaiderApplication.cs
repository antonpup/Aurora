using Aurora.Settings.Layers;

namespace Aurora.Profiles.ROTTombRaider
{
    public class ROTTombRaider : Application
    {
        public ROTTombRaider()
            : base(new LightEventConfig {
                Name = "Rise of the Tomb Raider",
                ID = "rot_tombraider",
                ProcessNames = new[] { "ROTTR.exe" },
                ProfileType = typeof(WrapperProfile),
                OverviewControlType = typeof(Control_ROTTombRaider),
                GameStateType = typeof(GameState_Wrapper),
                Event = new GameEvent_Generic(),
                IconURI = "Resources/rot_tombraider.png"
            })
        {
            AllowLayer<WrapperLightsLayerHandler>();
        }
    }
}
