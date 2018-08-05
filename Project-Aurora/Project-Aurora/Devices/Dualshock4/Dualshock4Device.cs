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
        private string devicename = "Dualshock";
        private bool isInitialized = false;
        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;
        private VariableRegistry default_registry = null;
        DS4HapticState state;
        DS4Color initColor;
        DS4Device device;

        Color newColor;
    
        public bool Initialize()
        {
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
                catch(Exception e)
                {
                    Global.logger.Error("Could not initialize Dualshock" + e);
                    isInitialized = false;
                }
                if (device != null)
                    isInitialized = true;
            }

            return isInitialized;
        }

        public void Shutdown()
        {
            try
            {
                if (isInitialized)
                {
                    RestoreColor();
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
                return devicename + ": Connected";
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
            return this.isInitialized;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel) return false;
            try
            {
                foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                {
                    Color color = (Color)key.Value;
                    //Apply and strip Alpha
                    color = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));

                    if (e.Cancel) return false;

                    if (key.Key == DeviceKeys.Peripheral_Logo)
                    {
                        newColor = color;                     
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
            if(default_registry==null)
            {
                default_registry = new VariableRegistry();
                default_registry.Register($"{devicename}_restore_dualshock", new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(255, 0, 0, 255)), "Color", new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(255, 255, 255, 255)), new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(0, 0, 0, 0)), "Set restore color for your generic roccat devices");
            }
            return default_registry;
        }

        public void RestoreColor()
        {
            System.Drawing.Color restore_fallback = Global.Configuration.VarRegistry.GetVariable<Aurora.Utils.RealColor>($"{devicename}_restore_dualshock").GetDrawingColor();
            newColor = restore_fallback;
        }

        public void SendColor(object sender, EventArgs e)
        {
            DS4Color ds4color;
            ds4color.green = newColor.G;
            ds4color.blue = newColor.B;
            ds4color.red = newColor.R;
            state.LightBarColor = ds4color;
            device.pushHapticState(state);
        }
    }
}