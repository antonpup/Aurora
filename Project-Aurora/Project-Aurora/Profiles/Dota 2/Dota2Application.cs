using Aurora.Profiles.Dota_2.Layers;
using Aurora.Settings;

namespace Aurora.Profiles.Dota_2
{
    public class Dota2 : Application
    {
        public Dota2()
            : base(new LightEventConfig {
                Name = "Dota 2",
                ID = "dota2",
                AppID = "570",
                ProcessNames = new[] { "dota2.exe" },
                SettingsType = typeof(FirstTimeApplicationSettings),
                ProfileType = typeof(Dota2Profile),
                OverviewControlType = typeof(Control_Dota2),
                GameStateType = typeof(GSI.GameState_Dota2),
                Event = new GameEvent_Generic(),
                IconURI = "Resources/dota2_64x64.png"
            })
        {
            AllowLayer<Dota2BackgroundLayerHandler>();
            AllowLayer<Dota2RespawnLayerHandler>();
            AllowLayer<Dota2AbilityLayerHandler>();
            AllowLayer<Dota2ItemLayerHandler>();
            AllowLayer<Dota2HeroAbilityEffectsLayerHandler>();
            AllowLayer<Dota2KillstreakLayerHandler>();
        }
    }
}
