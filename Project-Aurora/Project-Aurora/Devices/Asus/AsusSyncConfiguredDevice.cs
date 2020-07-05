using System.Collections.Generic;
using System.Drawing;
using AuraServiceLib;
using Aurora.Devices.Asus.Config;

namespace Aurora.Devices.Asus
{
    public class AsusSyncConfiguredDevice : AuraSyncDevice
    {
        private AsusConfig.AsusConfigDevice config;
        private readonly object configLock = new object();
        private static readonly List<AsusSyncConfiguredDevice> allConfigDevices = new List<AsusSyncConfiguredDevice>();
        
        /// <inheritdoc />
        public AsusSyncConfiguredDevice(AsusHandler asusHandler, IAuraSyncDevice device, AsusConfig.AsusConfigDevice config,  int frameRate = 30) : base(asusHandler, device, frameRate)
        {
            this.config = config;
            allConfigDevices.Add(this);
        }

        /// <inheritdoc />
        protected override void ApplyColors(Dictionary<DeviceKeys, Color> colors)
        {
            lock (configLock)
            {
                foreach (var keyPair in config.KeyMapper)
                {
                    if (colors.TryGetValue(keyPair.Value, out var color))
                        SetRgbLight(keyPair.Key, color);
                }
            }
        }

        public void UpdateConfig(AsusConfig.AsusConfigDevice config)
        {
            lock (configLock)
            {
                this.config = config;
            }
        }
        
        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            allConfigDevices.Remove(this);
        }

        public static void UpdateConfig(AsusConfig config)
        {
            foreach (var configDevice in config.Devices)
            {
                foreach (var device in allConfigDevices)
                {
                    if (configDevice.Equals(device.config))
                    {
                        device.UpdateConfig(configDevice);
                    }
                }
            }
        }
    }
}