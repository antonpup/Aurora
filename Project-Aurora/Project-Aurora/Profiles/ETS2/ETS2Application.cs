using Aurora.Profiles.ETS2.Layers;

namespace Aurora.Profiles.ETS2 {

    public class ETS2 : Application {

        public ETS2() : base(new LightEventConfig {
            Name = "Euro Truck Simulator 2",
            ID = "ets2",
            AppID = "227300",
            ProcessNames = new[] { "eurotrucks2.exe" },
            SettingsType = typeof(Settings.FirstTimeApplicationSettings),
            ProfileType = typeof(ETS2Profile),
            OverviewControlType = typeof(Control_ETS2),
            GameStateType = typeof(GSI.GameState_ETS2),
            Event = new GameEvent_ETS2("eurotrucks2"),
            IconURI = "Resources/ets2_64x64.png"
        }) {

            AllowLayer<ETS2BlinkerLayerHandler>();
            AllowLayer<ETS2BeaconLayerHandler>();
        }
    }
}
