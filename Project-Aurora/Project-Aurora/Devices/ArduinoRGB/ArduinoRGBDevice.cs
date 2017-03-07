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
        private string devicename_key = "ArduinoSerialRGB";
        private int baud_rate = 57600;
        private bool isInitialized = false;

        private VariableRegistry default_registry = null;

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
                    if (!arduino.connect(Global.Configuration.VarRegistry.GetVariable<string>($"{devicename_key}_port_name"), baud_rate)) {
                        return false;
                    }

                    // Configure Receiver
                    arduino.enableColorTransitions(Global.Configuration.VarRegistry.GetVariable<bool>($"{devicename_key}_use_smoothing"));
                    arduino.enableGamaCorrection(Global.Configuration.VarRegistry.GetVariable<bool>($"{devicename_key}_use_gamma"));
                    try
                    {
                        arduino.calibrateColor(
                            Byte.Parse(Global.Configuration.VarRegistry.GetVariable<int>($"{devicename_key}_max_white_r") + ""),
                            Byte.Parse(Global.Configuration.VarRegistry.GetVariable<int>($"{devicename_key}_max_white_g") + ""),
                            Byte.Parse(Global.Configuration.VarRegistry.GetVariable<int>($"{devicename_key}_max_white_b") + ""));

                    }
                    catch (Exception e) {
                        Global.logger.LogLine("Arduino device, Exception! WRONG MAX Message:" + e, Logging_Level.Error);
                    }
                    
                    arduino.turnOn();

                    // Update Lights on Logon
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
                arduino.disconnect();
                isInitialized = false;

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
                this.Shutdown();
                this.Initialize();
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
            if (default_registry == null)
            {
                default_registry = new VariableRegistry();
                default_registry.Register($"{devicename_key}_port_name", "COM4", "Serial Port");
                default_registry.Register($"{devicename_key}_brightness", /*(byte)*/255, "Brightness");
                default_registry.Register($"{devicename_key}_use_smoothing", true, "Use Smoothing");
                default_registry.Register($"{devicename_key}_use_gamma", true, "Enable Gamma Correction");
                default_registry.Register($"{devicename_key}_max_white_r", /*(byte)*/255, "Maximum Color Strength (Red)");
                default_registry.Register($"{devicename_key}_max_white_g", /*(byte)*/200, "Maximum Color Strength (Green)");
                default_registry.Register($"{devicename_key}_max_white_b", /*(byte)*/200, "Maximum Color Strength (Blue)");
            }

            return default_registry;
        }

        private void stopStopWatch() {
            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;
        }
    }
}
