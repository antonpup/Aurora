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
        public virtual string PrettyNameFull => PrettyName + $" (VendorID={device.Attributes.VendorHexId}, ProductID={device.Attributes.ProductHexId})";

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

                    if (IsConnected)
                    {
                        Global.logger.Info($"[UnifiedHID] connected to device {PrettyNameFull}");

                        DeviceColorMap.Clear();

                        foreach (var key in DeviceFuncMap)
                        {
                            // Set black as default color
                            DeviceColorMap.Add(key.Key, Color.Black);
                        }
                    }
                    else
                    { 
                        Global.logger.Error($"[UnifiedHID] error when attempting to open device {PrettyName}");
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.Error($"[UnifiedHID] error when attempting to open device {PrettyName}:\n{exc}");
                }
            }

            return IsConnected;
        }

        public virtual bool Disconnect()
        {
            try
            {
                if (device != null)
                {
                    device.CloseDevice();

                    Global.logger.Info($"[UnifiedHID] disconnected from device {PrettyNameFull})");
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error($"[UnifiedHID] error when attempting to close device {PrettyName}:\n{exc}");
            }

            return !IsConnected;
        }

        public virtual bool SetColor(DeviceKeys key, byte red, byte green, byte blue)
        {
            if (DeviceFuncMap.TryGetValue(key, out Func<byte, byte, byte, bool> func))
                return func.Invoke(red, green, blue);

            return false;
        }
    }
}
