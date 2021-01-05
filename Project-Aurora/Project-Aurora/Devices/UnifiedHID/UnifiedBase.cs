using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aurora.Settings;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;
using System.ComponentModel;
using System.Reflection;
using Aurora.Utils;

namespace Aurora.Devices.UnifiedHID
{
    internal abstract class UnifiedBase
    {
        protected HidDevice device = null;

        public Dictionary<DeviceKeys, Func<byte, byte, byte, bool>> DeviceFuncMap { get; protected set; } = new Dictionary<DeviceKeys, Func<byte, byte, byte, bool>>();
        public Dictionary<DeviceKeys, Color> DeviceColorMap { get; protected set; } = new Dictionary<DeviceKeys, Color>();

        public virtual bool IsConnected => device?.IsOpen ?? false;
        public virtual string PrettyName => "DeviceBase";

        public abstract bool Connect();

        protected bool Connect(int vendorID, int[] productIDs, short usagePage)
        {
            IEnumerable<HidDevice> devices = HidDevices.Enumerate(vendorID, productIDs);

            if (devices.Count() > 0)
            {
                try
                {
                    device = devices.First(dev => dev.Capabilities.UsagePage == usagePage);
                    device.OpenDevice();

                    DeviceColorMap.Clear();

                    foreach (var key in DeviceFuncMap)
                    {
                        // Set black as default color
                        DeviceColorMap.Add(key.Key, Color.Black);
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.LogLine($"Error when attempting to open UnifiedHID device:\n{exc}", Logging_Level.Error);
                }
            }

            return IsConnected;
        }

        public virtual bool Disconnect()
        {
            try
            {
                device.CloseDevice();
            }
            catch (Exception exc)
            {
                Global.logger.LogLine($"Error when attempting to close UnifiedHID device:\n{exc}", Logging_Level.Error);
            }

            return !IsConnected;
        }

        public virtual bool SetColor(DeviceKeys key, byte red, byte green, byte blue)
        {
            if (IsConnected && DeviceFuncMap.TryGetValue(key, out Func<byte, byte, byte, bool> func))
                return func.Invoke(red, green, blue);

            return false;
        }
    }
}
