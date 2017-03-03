using Aurora.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using RobertKoszewski.DeviceDrivers;
using Aurora.Settings;

namespace Aurora.Devices.ArduinoRGB
{
    class ArduinoRGBDevice : Device
    {
        // Generic Variables
        private string devicename = "Arduino Serial RGB";
        private bool isInitialized = false;

        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

        // Settings
        private bool updateLightsOnLogon = true;

        // Clevo Controll Class
        private ArduinoSerialRGBLight arduino = new ArduinoSerialRGBLight();

        // Color Variables
        private Color CurrentColor = Color.Black;
        private Color LastColor = Color.Black;
        private bool ColorUpdated;

        // Session Switch Handler
        private SessionSwitchEventHandler sseh;

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
                    arduino.connect("COM4", 57600);
                    arduino.calibrateColor(255, 200, 200);
                    arduino.turnOn();

                    // Update Lights on Logon (Clevo sometimes resets the lights when you Hibernate, this would fix wrong colors)
                    if (updateLightsOnLogon) {
                        sseh = new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
                        SystemEvents.SessionSwitch += sseh;
                    }

                    // Mark Initialized = TRUE
                    isInitialized = true;
                    return true;
                }
                catch (Exception ex)
                {
                    Global.logger.LogLine("Arduino device, Exception! Message:" + ex, Logging_Level.Error);
                }

                // Mark Initialized = FALSE
                isInitialized = false;
                return false;
            }

            return isInitialized;

        }

        // Handle Logon Event
        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (this.IsInitialized() && e.Reason.Equals(SessionSwitchReason.SessionUnlock)) { // Only Update when Logged In
                this.SendColors(true);
            }
        }

        public void Shutdown()
        {
            if (this.IsInitialized())
            {
                // Release Arduino Connection
                arduino.turnOff();
               
                //arduino.disconnect();

                // Uninstantiate Session Switch
                if (sseh != null) {
                    SystemEvents.SessionSwitch -= sseh;
                    sseh = null;
                }
            }
        }

        public void Reset()
        {
            if (this.IsInitialized())
            {
                arduino.disconnect();
                try
                {
                    arduino.connect("COM4", 57600);
                    isInitialized = true;
                }
                catch (Exception) {
                    isInitialized = false;
                }
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

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced = false) // Is this necessary?
        {
            throw new NotImplementedException();
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, bool forced = false)
        {
            watch.Restart();
            Dictionary<DeviceKeys, Color> keyColors = colorComposition.keyColors;
            try
            {
                foreach (KeyValuePair<DeviceKeys, Color> pair in keyColors)
                {
                    // Peripheral
                    if (pair.Key == DeviceKeys.Peripheral) {
                        CurrentColor = pair.Value;
                        ColorUpdated = true;
                    }
                    
                }

                SendColors(forced);
                watch.Stop();
                lastUpdateTime = watch.ElapsedMilliseconds;
                return true;
            }
            catch (Exception exception)
            {
                Global.logger.LogLine("Arduino device, error when updating device. Error: " + exception, Logging_Level.Error, true);
                Console.WriteLine(exception);
                watch.Stop();
                lastUpdateTime = watch.ElapsedMilliseconds;
                return false;
            }
        }

        private void SendColors(bool forced = false)
        {
            if (forced || ColorUpdated)
            {
                if (forced || !LastColor.Equals(CurrentColor))
                {
                    // MYSTERY: // Why is it B,R,G instead of R,G,B? SetKBLED uses R,G,B but only B,R,G returns the correct colors. Is bitshifting different in C# than in C++?

                    // TODO: Alpha!!
                    //clevo.SetKBLED(ClevoSetKBLED.KBLEDAREA.ColorKBLeft, ColorKBLeft.B, ColorKBLeft.R, ColorKBLeft.G, (double)(ColorKBLeft.A / 0xff));

                    arduino.setColor(CurrentColor.R, CurrentColor.G, CurrentColor.B);
                    LastColor = CurrentColor;
                }
                ColorUpdated = false;
            }
        }

        // Device Status Methods
        public bool IsKeyboardConnected()
        {
            return isInitialized;
        }

        public bool IsPeripheralConnected()
        {
            return isInitialized;
        }

        public string GetDeviceUpdatePerformance()
        {
            return (isInitialized ? lastUpdateTime + " ms" : "");
        }

        public VariableRegistry GetRegisteredVariables()
        {
            return new VariableRegistry();
        }

        private void stopStopWatch() {
            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;
        }
    }
}
