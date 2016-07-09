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
        private const int retryInterval = 5000;
        private const int retryAttemps = 15;
        private int retryAttemptsLeft = retryAttemps;

        public int RetryAttempts
        {
            get
            {
                return retryAttemptsLeft;
            }
        }
        public event EventHandler NewDevicesInitialized;

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

            NewDevicesInitialized?.Invoke(this, new EventArgs());

            if (!retryActivated)
            {
                Thread retryThread = new Thread(RetryInitialize);
                retryThread.Start();
            }
        }

        private void RetryInitialize()
        {
            for (int try_count = 0; try_count < retryAttemps; try_count++)
            {
                Global.logger.LogLine("Retrying Device Initialization", Logging_Level.Info);

                foreach (Device device in devices)
                {
                    if (device.IsInitialized())
                        continue;

                    if (device.Initialize())
                        anyInitialized = true;

                    Global.logger.LogLine("Device, " + device.GetDeviceName() + ", was" + (device.IsInitialized() ? "" : " not") + " initialized", Logging_Level.Info);
                }

                retryAttemptsLeft--;

                NewDevicesInitialized?.Invoke(this, new EventArgs());

                Thread.Sleep(retryInterval);
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
                if (device.IsInitialized())
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
                if (device.IsInitialized())
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

            if(retryAttemptsLeft > 0)
                devices_info += "Retries: " + retryAttemptsLeft + "\r\n";

            return devices_info;
        }
    }
}
