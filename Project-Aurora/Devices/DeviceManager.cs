using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace Aurora.Devices
{
    public class DeviceManager
    {
        private List<Device> devices = new List<Device>();

        private bool anyInitialized = false;
        private bool retryActivated = false;

        public DeviceManager()
        {
            devices.Add(new Devices.Logitech.LogitechDevice());
            devices.Add(new Devices.Corsair.CorsairDevice());
            devices.Add(new Devices.Razer.RazerDevice());
        }

        public void Initialize()
        {
            foreach (Device device in devices)
            {
                if (device.Initialize())
                    anyInitialized = true;

                Global.logger.LogLine("Device, " + device.GetDeviceName() + ", was" + (device.IsInitialized() ? "" : " not") + " initialized", Logging_Level.Info);
            }

            if(!retryActivated)
            {
                Thread retryThread = new Thread(RetryInitialize);
                retryThread.Start();
            }
        }

        private void RetryInitialize()
        {
            for(int try_count = 0; try_count < 5; try_count++)
            {
                Global.logger.LogLine("Retrying Device Initialization", Logging_Level.Info);

                foreach (Device device in devices)
                {
                    if (device.Initialize())
                        anyInitialized = true;

                    Global.logger.LogLine("Device, " + device.GetDeviceName() + ", was" + (device.IsInitialized() ? "" : " not") + " initialized", Logging_Level.Info);
                }

                Thread.Sleep(5000);
            }
        }

        public bool AnyInitialized()
        {
            return anyInitialized;
        }

        public Device[] GetInitializedDevices()
        {
            List<Device> ret = new List<Device>();

            foreach (Device device in devices)
            {
                if(device.IsInitialized())
                {
                    ret.Add(device);
                }
            }

            return ret.ToArray();
        }

        public void Shutdown()
        {
            foreach (Device device in devices)
            {
                if(device.IsInitialized())
                {
                    device.Shutdown();
                    Global.logger.LogLine("Device, " + device.GetDeviceName() + ", was shutdown", Logging_Level.Info);
                }
            }
        }

        public void ResetDevices()
        {
            foreach (Device device in devices)
            {
                if (device.IsInitialized())
                {
                    device.Reset();
                }
            }
        }

        public bool UpdateDevices(Dictionary<DeviceKeys, Color> keyColors, bool forced = false)
        {
            bool anyUpdated = false;
            Dictionary<DeviceKeys, Color> _keyColors = new Dictionary<DeviceKeys, Color>(keyColors);

            foreach (Device device in devices)
            {
                if (device.IsInitialized())
                {
                    if (device.UpdateDevice(_keyColors, forced))
                        anyUpdated = true;
                }
            }

            return anyUpdated;
        }

        public string GetDevices()
        {
            string devices_info = "";

            foreach (Device device in devices)
                devices_info += device.GetDeviceDetails() + "\r\n";

            return devices_info;
        }
    }
}
