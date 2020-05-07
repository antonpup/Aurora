using Aurora.Profiles.ETS2;
using Aurora.Profiles.ETS2.GSI;
using Aurora.Profiles.ETS2.Layers;
using Aurora.Settings;

namespace Aurora.Profiles.ATS {
    public class ATS : Application {

        // American Truck Simulator uses the same engine and telemetry DLL as Euro Truck Simulator 2, so
        // we will just use ETS2's classes and layers etc.

        public ATS() : base(new LightEventConfig() {
            Name = "American Truck Simulator",
            ID = "ats",
            AppID = "270880",
            ProcessNames = new[] { "amtrucks.exe" },
            SettingsType = typeof(FirstTimeApplicationSettings),
            ProfileType = typeof(ETS2Profile),
            OverviewControlType = typeof(Control_ATS),
            GameStateType = typeof(GameState_ETS2),
            Event = new GameEvent_ETS2("amtrucks"),
            IconURI = "Resources/ats_64x64.png"
        }) {

            AllowLayer<ETS2BlinkerLayerHandler>();
            AllowLayer<ETS2BeaconLayerHandler>();
        }
    }
}
