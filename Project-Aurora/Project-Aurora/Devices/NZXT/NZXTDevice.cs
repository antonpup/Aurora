using NZXTSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Settings;
using Microsoft.Win32.TaskScheduler;
using System.ComponentModel;
using Color = System.Drawing.Color;

namespace Aurora.Devices.NZXT
{
    class NZXTDevice : Device
    {
        private String devicename = "NZXT";
        private bool isInitialized = false;

        private DeviceLoader DeviceLoader;
        private byte[] hueDevice = new byte[120];
        private byte[] krakenringDevice = new byte[24];
        private Color krakenlogoDevice = Color.Black;

        private readonly object action_lock = new object();

        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

        public string GetDeviceName()
        {
            return devicename;
        }

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                return devicename + ": " + (DeviceLoader.HuePlus != null ? "HuePlus " : "") + (DeviceLoader.KrakenX != null ? "KrakenX " : "");
            }
            else
            {
                return devicename + ": Not initialized";
            }
        }

        public bool Initialize()
        {
            lock (action_lock)
            {
                if (!isInitialized)
                {
                    try
                    {
                        DeviceLoader = new DeviceLoader(DeviceLoadFilter.LightingControllers);

                        if (DeviceLoader.NumDevices == 0)
                        {
                            Global.logger.Error("NZXT device error: No devices found");
                        }
                        else
                        {
                            isInitialized = true;
                            return true;
                        }
                    }
                    catch (Exception exc)
                    {
                        Global.logger.Error("NZXT device, Exception during Initialize. Message: " + exc);
                    }

                    isInitialized = false;
                    return false;
                }
                return isInitialized;
            }
        }

        public void Shutdown()
        {
            lock (action_lock)
            {
                try
                {
                    if (isInitialized)
                    {
                        DeviceLoader.Dispose();
                        isInitialized = false;
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.Error("NZXT device, Exception during Shutdown. Message: " + exc);
                    isInitialized = false;
                }
            }
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

        public bool IsInitialized()
        {
            return isInitialized;
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keycolors, DoWorkEventArgs e, bool forced = false)
        {
            int hueIndex = 0, krakenIndex = 0;
            bool hueChanged = false, krakenChanged = false, krakenLogoChanged = false;

            if (e.Cancel) return false;

            try
            {
                foreach (KeyValuePair<DeviceKeys, Color> key in keycolors)
                {
                    if (e.Cancel) return false;
                    if (key.Key == DeviceKeys.Peripheral_Logo)
                    {
                        if (krakenlogoDevice != key.Value)
                        {
                            krakenlogoDevice = key.Value;
                            krakenLogoChanged = true;
                        }
                    }
                    else if (key.Key >= DeviceKeys.ONE && key.Key <= DeviceKeys.EIGHT)
                    {
                        hueChanged |= UpdateValueAt(hueDevice, hueIndex++, key.Value.G);
                        hueChanged |= UpdateValueAt(hueDevice, hueIndex++, key.Value.R);
                        hueChanged |= UpdateValueAt(hueDevice, hueIndex++, key.Value.B);

                        krakenChanged |= UpdateValueAt(krakenringDevice, krakenIndex++, key.Value.G);
                        krakenChanged |= UpdateValueAt(krakenringDevice, krakenIndex++, key.Value.R);
                        krakenChanged |= UpdateValueAt(krakenringDevice, krakenIndex++, key.Value.B);
                    }
                    else if (key.Key >= DeviceKeys.NINE && key.Key <= DeviceKeys.ZERO      ||
                             key.Key >= DeviceKeys.Q    && key.Key <= DeviceKeys.P         ||
                             key.Key >= DeviceKeys.A    && key.Key <= DeviceKeys.SEMICOLON ||
                             key.Key >= DeviceKeys.Z    && key.Key <= DeviceKeys.PERIOD    )//40 keys
                    {
                        hueChanged |= UpdateValueAt(hueDevice, hueIndex++, key.Value.G);
                        hueChanged |= UpdateValueAt(hueDevice, hueIndex++, key.Value.R);
                        hueChanged |= UpdateValueAt(hueDevice, hueIndex++, key.Value.B);
                    }
                }

                if (hueChanged || forced)
                    DeviceLoader.HuePlus?.ApplyEffect(DeviceLoader.HuePlus.Both, new NZXTSharp.Fixed(hueDevice));

                if (krakenChanged || forced)
                    DeviceLoader.KrakenX?.ApplyEffect(DeviceLoader.KrakenX.Ring, new NZXTSharp.Fixed(krakenringDevice));

                if (krakenLogoChanged || forced)
                    DeviceLoader.KrakenX?.ApplyEffect(DeviceLoader.KrakenX.Logo, new NZXTSharp.Fixed(new NZXTSharp.Color(krakenlogoDevice.R, krakenlogoDevice.G, krakenlogoDevice.B)));


                return true;
            }
            catch (Exception ex)
            {
                Global.logger.Error("NZXT, error when updating device: " + ex);
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
            return new VariableRegistry();
        }

        private bool UpdateValueAt(byte[] array, int index, byte value)
        {
            var changed = array[index] != value;
            if (changed) array[index] = value;
            return changed;
        }
    }
}
