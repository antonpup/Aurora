using Corale.Colore.Core;
using Corale.Colore.Razer.Keyboard;
using System;
using System.Collections.Generic;

namespace Aurora.Devices.Clevo
{
    class ClevoDevice : Device
    {
        private String devicename = "Clevo Keyboard";
        private bool isInitialized = false;

        private ClevoSetKBLED clevo = new ClevoSetKBLED();

        private Color globalKBColor = new Color(0, 0, 0);
        private bool globalKBColorUpdated = false;

        private System.Drawing.Color previous_peripheral_Color = System.Drawing.Color.Black;

        public string GetDeviceName()
        {
            return devicename;
        }

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                return devicename + ": Initialized";
            }
            else
            {
                return devicename + ": Not initialized";
            }
        }

        public bool Initialize()
        {
            if (!isInitialized)
            {
                try
                {
                    // Initialize Clevo WMI Interface Connection
                    if (!clevo.Initialize())
                    {
                        Global.logger.LogLine("Could not connect to Clevo WMI Interface", Logging_Level.Info);
                        throw new Exception("Could not connect to Clevo WMI Interface");
                    }

                    // Mark Initialized = TRUE
                    isInitialized = true;
                    return true;
                }
                catch (Exception ex)
                {
                    Global.logger.LogLine("Clevo device, Exception! Message:" + ex, Logging_Level.Error);
                }

                // Mark Initialized = FALSE
                isInitialized = false;
                return false;
            }

            return isInitialized;

        }

        public void Shutdown()
        {
        }

        public void Reset()
        {
            if (this.IsInitialized())
            {
                clevo.Release();
                isInitialized = clevo.Initialize();
            }
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public bool IsInitialized()
        {
            return isInitialized;
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, System.Drawing.Color> keyColors, bool forced = false)
        {
            try
            {
                foreach (KeyValuePair<DeviceKeys, System.Drawing.Color> key in keyColors)
                {
                    if (key.Key == DeviceKeys.Peripheral)
                    {
                        SetGlobalKBColor(key.Value, forced);
                    }

                    // TODO: Implement custom regions on keyboard? Clevo only supports 3 regions on keyboard + touchpad
                }

                SendColorsToKeyboard(forced);
                return true;
            }
            catch (Exception e)
            {
                Global.logger.LogLine("Corsair device, error when updating device. Error: " + e, Logging_Level.Error);
                Console.WriteLine(e);
                return false;
            }
        }

       

        private void SetGlobalKBColor(System.Drawing.Color color, bool forced = false)
        {
            globalKBColor = color;
            globalKBColorUpdated = true;
        }

        private void SendColorsToKeyboard(bool forced = false)
        {
            // Update all regions and Keys
            if (globalKBColorUpdated) { 
                clevo.SetKBLED(ClevoSetKBLED.KBLEDAREA.ColorKBLeft, globalKBColor.R, globalKBColor.G, globalKBColor.B, globalKBColor.A/255);
                clevo.SetKBLED(ClevoSetKBLED.KBLEDAREA.ColorKBCenter, globalKBColor.R, globalKBColor.G, globalKBColor.B, globalKBColor.A / 255);
                clevo.SetKBLED(ClevoSetKBLED.KBLEDAREA.ColorKBRight, globalKBColor.R, globalKBColor.G, globalKBColor.B, globalKBColor.A / 255);
                clevo.SetKBLED(ClevoSetKBLED.KBLEDAREA.ColorTouchpad, globalKBColor.R, globalKBColor.G, globalKBColor.B, globalKBColor.A / 255);
                globalKBColorUpdated = false;
            }
        }


        public bool IsKeyboardConnected()
        {
            return true; // TODO: Implement Keyboard functionality
        }

        public bool IsPeripheralConnected()
        {
            return isInitialized;
        }
    }
}
