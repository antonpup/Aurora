using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Aurora.Settings;

namespace Aurora.Devices.Omen
{
    public class OmenDevice : Device
    {
        OmenKeyboard keyboard;
        OmenMouse mouse;
        OmenMousePad mousePad;
        OmenChassis chassis;
        OmenSpeaker speaker;

        private bool isInitialized = false;
        private readonly string devicename = "OMEN";

        private readonly System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;
        private bool keyboard_updated;
        private bool peripheral_updated;

        public string GetDeviceDetails()
        {
            return devicename;
        }

        public string GetDeviceName()
        {
            if (isInitialized)
            {
                return devicename + ": " 
                    + (keyboard != null ? "Keyboard Connected " : " ") 
                    + (mouse != null ? "Mouse Connected " : " ") 
                    + (mousePad != null ? "Mouse Pad Connected " : " ") 
                    + (chassis != null ? "Chassis Connected " : " ")
                    + (speaker != null ? "Speaker Connected " : " ");
            }
            else
            {
                return devicename + ": Not initialized";
            }
        }

        public string GetDeviceUpdatePerformance()
        {
            return (isInitialized ? lastUpdateTime + " ms" : "");
        }

        public VariableRegistry GetRegisteredVariables()
        {
            return new VariableRegistry();
        }

        public bool Initialize()
        {

            lock (this)
            {
                if (!isInitialized)
                {
                    keyboard = OmenKeyboard.GetOmenKeyboard();
                    mouse = OmenMouse.GetOmenMouse();
                    mousePad = OmenMousePad.GetOmenMousePad();
                    chassis = OmenChassis.GetOmenChassis();
                    speaker = OmenSpeaker.GetOmenSpeaker();

                    Global.kbLayout.KeyboardLayoutUpdated -= DeviceChangedHandler;
                    Global.kbLayout.KeyboardLayoutUpdated += DeviceChangedHandler;

                    isInitialized = true;
                }
            }
            return isInitialized;
        }

        public bool IsConnected()
        {
            return IsInitialized() && (keyboard != null || mouse != null);
        }

        public bool IsInitialized()
        {
            return isInitialized;
        }

        public bool IsKeyboardConnected()
        {
            return keyboard != null;
        }

        public bool IsPeripheralConnected()
        {
            return (mouse != null);
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            if (IsInitialized() && (keyboard_updated || peripheral_updated))
            {

                keyboard_updated = false;
                peripheral_updated = false;
            }
        }

        private void DeviceChangedHandler(object sender)
        {
            Global.logger.Info("Devices is changed. Reset Omen Devices");
            Shutdown();
            Initialize();
        }

        public void Shutdown()
        {
            lock (this)
            {
                try
                {
                    if (isInitialized)
                    {
                        Reset();

                        keyboard?.Shutdown();
                        mouse?.Shutdown();
                        mousePad?.Shutdown();
                        chassis?.Shutdown();
                        speaker?.Shutdown();
                    }
                }
                catch (Exception e)
                {
                    Global.logger.Error("OMEN device, Exception during Shutdown. Message: " + e);
                }

                isInitialized = false;
                mouse = null;
                keyboard = null;
                mousePad = null;
                chassis = null;
                speaker = null;
            }
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            try
            {
                if (e.Cancel) return false;

                foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                {
                    if (e.Cancel) return false;
                    if (key.Key == DeviceKeys.Peripheral_Logo || key.Key == DeviceKeys.Peripheral || key.Key == DeviceKeys.Peripheral_FrontLight || key.Key == DeviceKeys.Peripheral_ScrollWheel)
                    {
                        UpdateMouse(key);
                        UpdateChassis(key);
                        if(key.Key == DeviceKeys.Peripheral_Logo)
                        {
                            UpdateSpeaker(key);
                        }
                    }
                    if (key.Key >= DeviceKeys.MOUSEPADLIGHT1 && key.Key <= DeviceKeys.MOUSEPADLIGHT15)
                    {
                        UpdateMousePad(key);
                    }
                }

                UpdateKeyboard(keyColors);
            }
            catch (Exception ex)
            {
                Global.logger.Error("OMEN device, Exception during update device. Message: " + ex);
            }

            return true;
        }

        private void UpdateKeyboard(Dictionary<DeviceKeys, Color> keyColors)
        {
            if (keyboard != null && !Global.Configuration.devices_disable_keyboard)
            {
                keyboard.SetKeys(keyColors);
                keyboard_updated = true;
            }
        }

        private void UpdateMouse(KeyValuePair<DeviceKeys, Color> key)
        {
            if (Global.Configuration.devices_disable_mouse)
                return;

            if (mouse != null && Global.Configuration.allow_peripheral_devices)
            {
                mouse.SetLights(key.Key, key.Value);
                peripheral_updated = true;
            }
            else
            {
                peripheral_updated = false;
            }
        }

        private void UpdateMousePad(KeyValuePair<DeviceKeys, Color> key)
        {
            if (mousePad != null && Global.Configuration.allow_peripheral_devices)
            {
                mousePad.SetLights(key.Key, key.Value);
                peripheral_updated = true;
            }
            else
            {
                peripheral_updated = false;
            }
        }

        private void UpdateChassis(KeyValuePair<DeviceKeys, Color> key)
        {
            if (chassis != null && Global.Configuration.allow_peripheral_devices)
            {
                chassis.SetLights(key.Key, key.Value);
                peripheral_updated = true;
            }
            else
            {
                peripheral_updated = false;
            }
        }

        private void UpdateSpeaker(KeyValuePair<DeviceKeys, Color> key)
        {
            if (speaker != null && Global.Configuration.allow_peripheral_devices)
            {
                speaker.SetLights(key.Key, key.Value);
                peripheral_updated = true;
            }
            else
            {
                peripheral_updated = false;
            }
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            bool update_result = UpdateDevice(colorComposition.keyColors, e, forced);

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            Global.logger.Info($"OMEN device, Update cost {lastUpdateTime} ms ");

            return update_result;
        }
    }
}
