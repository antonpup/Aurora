using Aurora.Settings;

namespace Aurora.Devices.Integration
{
    public class DeviceIntegrationSettings : SettingsProfile
    {
        protected bool enabled;
        public bool Enabled
        {
            get => enabled;
            set => UpdateVar(ref enabled, value);
        }
        
        public override void Default()
        {
            Enabled = true;
        }
    }
}