using Aurora.Devices.Layout;
using Aurora.Devices.Layout.Layouts;
using Aurora.Settings;
using DrevoRadi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using LEDINT = System.Int16;

namespace Aurora.Devices.Drevo
{
    class DrevoDevice : Device<DeviceSettings>
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private String devicename = "Drevo";

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
        public override string GetDeviceName()
        {
            return devicename;
        }

        /// Gets specific details about the device instance.
        public override string GetDeviceDetails()
        {
            if (Initialized)
            {
                return devicename + ": Connected";
            }
            else
            {
                return devicename + ": Not initialized";
            }
        }

        /// Attempts to initialize the device instance.
        public override bool Initialize()
        {
            lock (action_lock)
            {
                if (!Initialized)
                {
                    try
                    {
                        if (!DrevoRadiSDK.DrevoRadiInit())
                        {
                            logger.Error("Drevo Radi SDK could not be initialized.");

                            return Initialized = false;
                        }

                        return Initialized = true;
                    }
                    catch (Exception exc)
                    {
                        logger.Error("There was an error initializing Drevo Radi SDK.\r\n" + exc.Message);

                        return Initialized = false;
                    }
                }
                return Initialized;
            }
        }

        /// Shuts down the device instance.
        public override void Shutdown()
        {
            lock (action_lock)
            {
                try
                {
                    if (Initialized)
                    {
                        this.Reset();
                        Initialized = false;
                        DrevoRadiSDK.DrevoRadiShutdown();
                    }
                }
                catch (Exception exc)
                {
                    logger.Error("Drevo device, Exception during Shutdown. Message: " + exc);
                    Initialized = false;
                }
            }
        }

        /// Resets the device instance.
        public override void Reset()
        {
            if (Initialized && (keyboard_updated || peripheral_updated))
            {
                keyboard_updated = false;
                peripheral_updated = false;
            }
        }

        /// Attempts to reconnect the device. [NOT IMPLEMENTED]
        public override bool Reconnect()
        {
            throw new NotImplementedException();
        }

        /// Gets the connection status of this device instance. [NOT IMPLEMENTED]
        public override bool IsConnected()
        {
            throw new NotImplementedException();
        }

        /// Gets the keyboard connection status for this device instance.
        public override bool IsKeyboardConnected()
        {
            return Initialized;
        }

        /// Gets the peripheral connection status for this device instance.
        public override bool IsPeripheralConnected()
        {
            return Initialized;
        }

        /// Updates the device with a specified color arrangement.
        public override bool PerformUpdateDevice(Color GlobalColor, List<DeviceLayout> devices, DoWorkEventArgs e, bool forced = false)
        {
            bool updateResult = true;

            try
            {

                foreach (DeviceLayout layout in devices)
                {
                    switch (layout)
                    {
                        case KeyboardDeviceLayout kb:
                            if (!UpdateDevice(kb, e, forced))
                                updateResult = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Drevo device, error when updating device: " + ex);
                return false;
            }

            return updateResult;
        }

        private bool UpdateDevice(KeyboardDeviceLayout kb, DoWorkEventArgs e, bool forced)
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
                foreach (KeyValuePair<LEDINT, Color> key in kb.DeviceColours.deviceColours)
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
                if (Initialized)
                {
                    DrevoRadiSDK.DrevoRadiSetRGB(bitmap, 392);
                }
                if (e.Cancel) return false;
                return true;
            }
            catch (Exception exc)
            {
                logger.Error("Drevo device, error when updating device. Error: " + exc);
                Console.WriteLine(exc);
                return false;
            }
        }
    }
}
