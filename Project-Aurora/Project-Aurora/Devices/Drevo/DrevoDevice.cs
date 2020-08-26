using System;
using DrevoRadi;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Settings;

namespace Aurora.Devices.Drevo
{
    public class DrevoDevice : DefaultDevice
    {
        public override string DeviceName => "Drevo";

        public override bool Initialize()
        {
            if (IsInitialized)
                return IsInitialized;

            try
            {
                if (!DrevoRadiSDK.DrevoRadiInit())
                {
                    LogError("Drevo Radi SDK could not be initialized.");
                    return IsInitialized = false;
                }

                return IsInitialized = true;
            }
            catch (Exception exc)
            {
                LogError($"There was an error initializing Drevo Radi SDK: {exc.Message}");
                return IsInitialized = false;
            }
        }

        public override void Shutdown()
        {
            if (!IsInitialized)
                return;

            try
            {
                DrevoRadiSDK.DrevoRadiShutdown();
                IsInitialized = false;
            }
            catch (Exception exc)
            {
                LogError("Exception during Shutdown: " + exc);
                IsInitialized = false;
            }
        }

        public override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!IsInitialized)
                return false;

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
                    index = DrevoRadiSDK.ToDrevoBitmap((int)key.Key);
                    if (index != -1)
                    {
                        index = index * 3 + 4;
                        bitmap[index] = key.Value.R;
                        bitmap[index + 1] = key.Value.G;
                        bitmap[index + 2] = key.Value.B;
                    }
                }

                return DrevoRadiSDK.DrevoRadiSetRGB(bitmap, 392);
            }
            catch (Exception exc)
            {
                LogError($"Error when updating device: {exc}");
                return false;
            }
        }
    }
}
