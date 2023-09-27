using System.ComponentModel;
using System.Drawing;
using Common.Devices;

namespace AurorDeviceManager.Devices.Drevo
{
    public class DrevoDevice : DefaultDevice
    {
        public override string DeviceName => "Drevo";

        protected override Task<bool> DoInitialize()
        {
            if (IsInitialized)
                return Task.FromResult(true);

            try
            {
                if (!DrevoRadiSdk.DrevoRadiInit())
                {
                    LogError("Drevo Radi SDK could not be initialized.");
                    IsInitialized = false; 
                    return Task.FromResult(false);
                }

                IsInitialized = true;
                return Task.FromResult(true);
            }
            catch (Exception exc)
            {
                LogError($"There was an error initializing Drevo Radi SDK", exc);
                IsInitialized = false;
                return Task.FromResult(false);
            }
        }

        protected override Task Shutdown()
        {
            if (!IsInitialized)
                return Task.CompletedTask;

            try
            {
                DrevoRadiSdk.DrevoRadiShutdown();
                IsInitialized = false;
            }
            catch (Exception exc)
            {
                LogError("Exception during Shutdown", exc);
                IsInitialized = false;
            }

            return Task.CompletedTask;
        }

        protected override Task<bool> UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!IsInitialized)
                return Task.FromResult(false);

            try
            {
                byte[] bitmap = new byte[392];
                bitmap[0] = 0xF3;
                bitmap[1] = 0x01;
                bitmap[2] = 0x00;
                bitmap[3] = 0x7F;
                int index = 0;

                foreach (var key in keyColors)
                {
                    index = DrevoRadiSdk.ToDrevoBitmap((int)key.Key);
                    if (index != -1)
                    {
                        index = index * 3 + 4;
                        bitmap[index] = key.Value.R;
                        bitmap[index + 1] = key.Value.G;
                        bitmap[index + 2] = key.Value.B;
                    }
                }

                return Task.FromResult(DrevoRadiSdk.DrevoRadiSetRGB(bitmap, 392));
            }
            catch (Exception exc)
            {
                LogError($"Error when updating device", exc);
                return Task.FromResult(false);
            }
        }
    }
}
