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
    class DrevoDevice : Device
    {
        private String devicename = "Drevo";
        private bool isInitialized = false;

        private bool keyboard_updated = false;
        private bool peripheral_updated = false;

        private readonly object action_lock = new object();
        private Stopwatch watch = new Stopwatch();
        private Stopwatch keepaliveTimer = new Stopwatch();
        private long lastUpdateTime = 0;

        /// <summary>
        /// Gets registered variables by this device.
        /// </summary>
        /// <returns>Registered Variables</returns>
        public VariableRegistry GetRegisteredVariables()
        {
            return new VariableRegistry();
        }

        /// Gets the device name.
        public string GetDeviceName()
        {
            return devicename;
        }

        /// Gets specific details about the device instance.
        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                return devicename + ": Connected";
            }
            else
            {
                return devicename + ": Not initialized";
            }
        }

        /// Gets the device update performance.
        public string GetDeviceUpdatePerformance()
        {
            return (isInitialized ? lastUpdateTime + " ms" : "");
        }

        /// Attempts to initialize the device instance.
        public bool Initialize()
        {
            lock (action_lock)
            {
                if (!isInitialized)
                {
                    try
                    {
                        if (!DrevoRadiSDK.DrevoRadiInit())
                        {
                            Global.logger.Error("Drevo Radi SDK could not be initialized.");

                            isInitialized = false;
                            return false;
                        }

                        isInitialized = true;
                        return true;
                    }
                    catch (Exception exc)
                    {
                        Global.logger.Error("There was an error initializing Drevo Radi SDK.\r\n" + exc.Message);

                        return false;
                    }
                }
                return isInitialized;
            }
        }

        /// Shuts down the device instance.
        public void Shutdown()
        {
            lock (action_lock)
            {
                try
                {
                    if (IsInitialized())
                    {
                        this.Reset();
                        isInitialized = false;
                        DrevoRadiSDK.DrevoRadiShutdown();
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.Error("Drevo device, Exception during Shutdown. Message: " + exc);
                    isInitialized = false;
                }
            }
        }

        /// Resets the device instance.
        public void Reset()
        {
            if (this.IsInitialized() && (keyboard_updated || peripheral_updated))
            {
                keyboard_updated = false;
                peripheral_updated = false;
            }
        }

        /// Attempts to reconnect the device. [NOT IMPLEMENTED]
        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        /// Gets the initialization status of this device instance.
        public bool IsInitialized()
        {
            return isInitialized;
        }

        /// Gets the connection status of this device instance. [NOT IMPLEMENTED]
        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        /// Gets the keyboard connection status for this device instance.
        public bool IsKeyboardConnected()
        {
            return isInitialized;
        }

        /// Gets the peripheral connection status for this device instance.
        public bool IsPeripheralConnected()
        {
            return isInitialized;
        }

        /// Updates the device with a specified color arrangement.
        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel) return false;
            try
            {
                byte[] bitmap = new byte[392];
                bitmap[0] = 0xF3;
                bitmap[1] = 0x01;
                bitmap[2] = 0x00;
                bitmap[3] = 0x7F;
                int index = 0;
                foreach (KeyValuePair<DeviceKeys, System.Drawing.Color> key in keyColors)
                {
                    if (e.Cancel) return false;

                    index = DrevoRadiSDK.ToDrevoBitmap((int)key.Key);
                    if (index != -1)
                    {
                        index = index * 3 + 4;
                        bitmap[index] = key.Value.R;
                        bitmap[index + 1] = key.Value.G;
                        bitmap[index + 2] = key.Value.B;
                    }
                }
                if (this.IsInitialized())
                {
                    DrevoRadiSDK.DrevoRadiSetRGB(bitmap, 392);
                }
                if (e.Cancel) return false;
                return true;
            }
            catch (Exception exc)
            {
                Global.logger.Error("Drevo device, error when updating device. Error: " + exc);
                Console.WriteLine(exc);
                return false;
            }
            return true;
        }

        /// Updates the device with a specified color composition.
        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            bool update_result = UpdateDevice(colorComposition.keyColors, e, forced);

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return update_result;
        }
    }
}
