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
    public class OmenDevices : Device
    {
        bool kbConnected = false;
        bool peripheralConnected = false;

        List<IOmenDevice> devices;

        private bool isInitialized = false;
        private readonly string devicename = "OMEN";

        private readonly System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

        public string GetDeviceName()
        {
            return devicename;
        }

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                string result = devicename + ":";
                foreach (var dev in devices)
                {
                    if (dev.GetDeviceName() != string.Empty)
                    {
                        result += (" " + dev.GetDeviceName() + ";");
                    }
                }

                return result;
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
            Global.kbLayout.KeyboardLayoutUpdated -= DeviceChangedHandler;
            Global.kbLayout.KeyboardLayoutUpdated += DeviceChangedHandler;

            lock (this)
            {
                if (!isInitialized)
                {
                    devices = new List<IOmenDevice>();
                    IOmenDevice dev;
                    if ((dev = OmenKeyboard.GetOmenKeyboard()) != null)
                    {
                        devices.Add(dev);
                        kbConnected = true;
                    }

                    if ((dev = OmenMouse.GetOmenMouse()) != null)
                    {
                        devices.Add(dev);
                        peripheralConnected = true;
                    }

                    if ((dev = OmenMousePad.GetOmenMousePad()) != null)
                    {
                        devices.Add(dev);
                        peripheralConnected = true;
                    }

                    if ((dev = OmenChassis.GetOmenChassis()) != null)
                    {
                        devices.Add(dev);
                        peripheralConnected = true;
                    }

                    if ((dev = OmenSpeaker.GetOmenSpeaker()) != null)
                    {
                        devices.Add(dev);
                        peripheralConnected = true;
                    }

                    isInitialized = (devices.Count != 0);
                }
            }
            return isInitialized;
        }

        public bool IsConnected()
        {
            return IsInitialized() && (devices != null && devices.Count != 0);
        }

        public bool IsInitialized()
        {
            return isInitialized;
        }

        public bool IsKeyboardConnected()
        {
            return kbConnected;
        }

        public bool IsPeripheralConnected()
        {
            return peripheralConnected;
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
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

                        foreach (var dev in devices)
                        {
                            dev.Shutdown();
                        }
                        devices.Clear();
                        devices = null;
                        kbConnected = peripheralConnected = false;
                    }
                }
                catch (Exception e)
                {
                    Global.logger.Error("OMEN device, Exception during Shutdown. Message: " + e);
                }

                isInitialized = false;
            }
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            try
            {
                if (e.Cancel || !isInitialized) return false;

                foreach (var dev in devices)
                {
                    dev.SetLights(keyColors);
                }
            }
            catch (Exception ex)
            {
                Global.logger.Error("OMEN device, Exception during update device. Message: " + ex);
            }

            return true;
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
