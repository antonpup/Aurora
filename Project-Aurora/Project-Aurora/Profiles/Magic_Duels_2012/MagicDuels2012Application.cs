using Aurora.Settings;
using Aurora.Settings.Layers;

namespace Aurora.Profiles.Magic_Duels_2012
{
    public class MagicDuels2012 : Application
    {

        public MagicDuels2012()
            : base(new LightEventConfig {
                Name = "Magic: The Gathering - Duels of the Planeswalkers 2012",
                ID = "magic_2012",
                ProcessNames = new[] { "magic_2012.exe" },
                SettingsType = typeof(FirstTimeApplicationSettings),
                ProfileType = typeof(WrapperProfile),
                OverviewControlType = typeof(Control_MagicDuels2012),
                GameStateType = typeof(GameState_Wrapper),
                Event = new GameEvent_Generic(),
                IconURI = "Resources/magic_duels_64x64.png"
            })
        {
            AllowLayer<WrapperLightsLayerHandler>();
        }
    }
}
