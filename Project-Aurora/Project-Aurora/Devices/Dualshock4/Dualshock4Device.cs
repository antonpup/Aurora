using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aurora.Settings;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using DS4Windows;

namespace Aurora.Devices.Dualshock
{

    class DualshockDevice : Device
    {
        private string devicename = "Sony DualShock 4(PS4)";
        private bool isInitialized = false;
        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private System.Diagnostics.Stopwatch cooldown = new System.Diagnostics.Stopwatch();
        private System.Diagnostics.Stopwatch effectwatch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;
        private VariableRegistry default_registry = null;
        DS4HapticState state;
        DS4Color initColor;
        DS4Device device;

        Color newColor;
        Color setRestoreColor;
        DeviceKeys newkey;

        public bool Initialize()
        {
            newkey = Global.Configuration.VarRegistry.GetVariable<DeviceKeys>($"{devicename}_devicekey");
            setRestoreColor = Color.Transparent;
            DS4Devices.findControllers();
            IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
            if (!devices.Any())
                return false;

            device = devices.ElementAt(0);
            initColor = device.LightBarColor;

            if (!isInitialized)
            {
                try
                {
                    device.Report += SendColor;
                    device.StartUpdate();
                    Global.logger.Info("Initialized Dualshock");
                }
                catch (Exception e)
                {
                    Global.logger.Error("Could not initialize Dualshock" + e);
                    isInitialized = false;
                }
                if (device != null)
                {
                    isInitialized = true;
                    if (Global.Configuration.dualshock_first_time)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            DualshockInstallInstructions instructions = new DualshockInstallInstructions();
                            instructions.ShowDialog();
                        });
                        Global.Configuration.dualshock_first_time = false;
                        Settings.ConfigManager.Save(Global.Configuration);
                    }
                }
            }

            return isInitialized;
        }

        public void Shutdown()
        {
            try
            {
                if (isInitialized)
                {
                    if (Global.Configuration.VarRegistry.GetVariable<bool>($"{devicename}_disconnect_when_stop"))
                    {
                        device.DisconnectBT();
                        device.DisconnectDongle();
                    }
                    RestoreColor();
                    device.StopUpdate();
                    DS4Devices.stopControllers();
                    isInitialized = false;
                }
            }
            catch (Exception e)
            {
                Global.logger.Error("There was an error shutting down DualShock: " + e);
                isInitialized = true;
            }
        }

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                string DS4ConnectionType;
                switch (device.getConnectionType())
                {
                    case ConnectionType.BT:
                        DS4ConnectionType = " over Bluetooth";
                        break;
                    case ConnectionType.USB:
                        DS4ConnectionType = " over USB";
                        break;
                    case ConnectionType.SONYWA:
                        DS4ConnectionType = " over DS4 Wireless adapter";
                        break;
                    default:
                        DS4ConnectionType = "";
                        break;
                }

                string charging;
                if (device.isCharging())
                    charging = " ⚡";
                else
                    charging = " ";

                return devicename + ": Connected" + DS4ConnectionType + charging + "🔋" + device.getBattery() + "%" + " Delay: " + device.Latency.ToString("0.00") + " ms";

            }
            else
            {
                return devicename + ": Not connected";
            }
        }

        public string GetDeviceName()
        {
            return devicename;
        }

        public void Reset()
        {
            if (this.IsInitialized())
            {
                Shutdown();
                Initialize();
            }
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            return this.isInitialized;
        }

        public bool IsInitialized()
        {
            if (effectwatch.IsRunning && !isInitialized)
            {
                effectwatch.Reset();
                //Global.logger.Info("Stop Ewatch");
            }

            bool isdisabled = Global.Configuration.devices_disabled.Contains(typeof(DualshockDevice));
            int auto_connect_cooldown = 3000;
            bool auto_connect_enabled = Global.Configuration.VarRegistry.GetVariable<bool>($"{devicename}_auto_connect");

            //Global.logger.Info("cooldown is running: " + cooldown.IsRunning);
            //Global.logger.Info("isDisabled: " + isdisabled);

            if ((isdisabled || isInitialized) && cooldown.IsRunning)
            {
                cooldown.Stop();
                //Global.logger.Info("Cooldown Stop");
            }

            if (!isdisabled && auto_connect_enabled && !isInitialized && !cooldown.IsRunning)
            {
                cooldown.Start();
                //Global.logger.Info("Cooldown Start");
            }

            if (!isInitialized && auto_connect_enabled && (cooldown.ElapsedMilliseconds > auto_connect_cooldown))
            {
                //Global.logger.Info("Initialize");
                Initialize();
                cooldown.Restart();
            }
            if (isInitialized && device.isDisconnectingStatus())
            {
                Shutdown();
            }
            return this.isInitialized;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel) return false;
            try
            {
                foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                {

                    if (e.Cancel) return false;

                    if (key.Key == newkey)
                    {
                        //Apply and strip Alpha
                        newColor = (Color)key.Value;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Global.logger.Error("Dualshock, error when updating device: " + ex);
                return false;
            }
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            bool update_result = UpdateDevice(colorComposition.keyColors, e, forced);

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return update_result;
        }

        public bool IsPeripheralConnected()
        {
            return isInitialized;
        }

        public bool IsKeyboardConnected()
        {
            return false;
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
                default_registry.Register($"{devicename}_restore_dualshock", new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(255, 0, 0, 255)), "Color", new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(255, 255, 255, 255)), new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(0, 0, 0, 0)), "Set restore color for your DS4 Controller");
                default_registry.Register($"{devicename}_disconnect_when_stop", false, "Disconnect when Stopping");
                default_registry.Register($"{devicename}_auto_connect", true, "Auto connect");
                default_registry.Register($"{devicename}_LowBattery_threshold", 20, "Low battery threshold", 100, 0, "In percent. To deactivate set to 0");
                default_registry.Register($"{devicename}_LowBattery_color", new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(255, 255, 0, 0)), "Low battery color", new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(255, 255, 255, 255)), new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(0, 0, 0, 0)));
                default_registry.Register($"{devicename}_devicekey", DeviceKeys.Peripheral, "Key to Use", DeviceKeys.MOUSEPADLIGHT15, DeviceKeys.Peripheral);
            }
            return default_registry;
        }

        public void RestoreColor()
        {
            System.Drawing.Color restore_fallback = Global.Configuration.VarRegistry.GetVariable<Aurora.Utils.RealColor>($"{devicename}_restore_dualshock").GetDrawingColor();
            setRestoreColor = restore_fallback;
        }

        public void SendColor(object sender, EventArgs e)
        {
            DS4Color ds4color;
            //calculate new Color out of KeyColor and Effect Color
            Color newDS4Color = CalculateAlpha(Utils.ColorUtils.AddColors(newColor, LowBatteryEffect()));
            if (setRestoreColor.Equals(Color.Transparent))
            {
                ds4color.green = newDS4Color.G;
                ds4color.blue = newDS4Color.B;
                ds4color.red = newDS4Color.R;
            }
            else
            {
                ds4color.green = setRestoreColor.G;
                ds4color.blue = setRestoreColor.B;
                ds4color.red = setRestoreColor.R;
            }

            state.LightBarColor = ds4color;
            if (ds4color.Equals(System.Drawing.Color.Black))
            {
                state.LightBarExplicitlyOff = false;
            }
            else
            {
                state.LightBarExplicitlyOff = true;
            }
            device.pushHapticState(state);
        }

        private Color CalculateAlpha(Color input)
            {
            //Apply and strip Alpha
            Color result = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(input, input.A / 255.0D));
            return result;
            }

        private Color LowBatteryEffect()
        {
            int flashlength = 100; 
            int pause = 50;
            int longpause = 4000;
            int LowBattery_threshold = Global.Configuration.VarRegistry.GetVariable<int>($"{devicename}_LowBattery_threshold");
            System.Drawing.Color LowBattery_color = Global.Configuration.VarRegistry.GetVariable<Aurora.Utils.RealColor>($"{devicename}_LowBattery_color").GetDrawingColor();

            Color LowBatteryColor = Color.Transparent;
            if (!effectwatch.IsRunning && (device.getBattery() <= LowBattery_threshold) && !device.isCharging())
            {
                effectwatch.Start();
                //Global.logger.Info("Start Ewatch");
            }

            if (effectwatch.ElapsedMilliseconds > 0 && effectwatch.ElapsedMilliseconds <= flashlength)
            {
                //Global.logger.Info("On");
                LowBatteryColor = LowBattery_color;
            }
            if (effectwatch.ElapsedMilliseconds > flashlength && effectwatch.ElapsedMilliseconds <= (flashlength + pause))
            {
                //Global.logger.Info("short Off");
                LowBatteryColor = Color.Transparent;
            }
            if (effectwatch.ElapsedMilliseconds > (flashlength + pause) && effectwatch.ElapsedMilliseconds <= (flashlength + pause + flashlength))
            {
                //Global.logger.Info("long On");
                LowBatteryColor = LowBattery_color;
            }
            if (effectwatch.ElapsedMilliseconds > (flashlength + pause + flashlength) && effectwatch.ElapsedMilliseconds <= (flashlength + pause + flashlength + longpause))
            {
                //Global.logger.Info("long Pause");
                LowBatteryColor = Color.Transparent;
            }
            if (effectwatch.IsRunning && (effectwatch.ElapsedMilliseconds > (flashlength + pause + flashlength + longpause)))
            {
                effectwatch.Reset();
                //Global.logger.Info("Reset Ewatch");
            }

            return LowBatteryColor;
        }


    }
}
