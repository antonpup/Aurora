using Aurora.Settings.Layers;

namespace Aurora.Profiles.Cardaclysm
{
    public class Cardaclysm : Application
    {
        public Cardaclysm() : base(new LightEventConfig {
            Name = "Cardaclysm",
            ID = "Cardaclysm",
            ProcessNames = new[] { "cardaclysm.exe" },
            ProfileType = typeof(WrapperProfile),
            OverviewControlType = typeof(Control_Cardaclysm),
            GameStateType = typeof(GameState_Wrapper),
            Event = new GameEvent_Generic(),
            IconURI = "Resources/cardaclysm_64x64.png"
        })
        {
            AllowLayer<WrapperLightsLayerHandler>();
        }
    }
}
