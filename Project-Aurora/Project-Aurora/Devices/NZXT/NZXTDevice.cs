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

        DeviceLoader DeviceLoader;

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
            if (!isInitialized)
            {
                try
                {
                    DeviceLoader = new DeviceLoader(DeviceLoadFilter.LightingControllers);
                    DeviceLoader.Initialize();

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
            var huepluscolors = new List<byte>(120);
            var krakenringcolors = new List<byte>(24);

            if (e.Cancel) return false;

            try
            {
                foreach (KeyValuePair<DeviceKeys, Color> key in keycolors)
                {
                    if (e.Cancel) return false;
                    if (key.Key == DeviceKeys.Peripheral_Logo)
                    {
                        DeviceLoader.KrakenX?.ApplyEffect(DeviceLoader.KrakenX.Logo, new NZXTSharp.Fixed(new NZXTSharp.Color(key.Value.R, key.Value.G, key.Value.B)));
                    }
                    else if (key.Key >= DeviceKeys.ONE && key.Key <= DeviceKeys.EIGHT)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                        krakenringcolors.Add(key.Value.G);
                        krakenringcolors.Add(key.Value.R);
                        krakenringcolors.Add(key.Value.B);
                    }
                    else if (key.Key >= DeviceKeys.NINE && key.Key < DeviceKeys.ZERO ||
                             key.Key >= DeviceKeys.Q && key.Key <= DeviceKeys.P ||
                             key.Key >= DeviceKeys.A && key.Key <= DeviceKeys.SEMICOLON ||
                             key.Key >= DeviceKeys.Z && key.Key <= DeviceKeys.PERIOD)//40 keys
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                }

                DeviceLoader.HuePlus?.ApplyEffect(DeviceLoader.HuePlus.Both, new NZXTSharp.Fixed(huepluscolors.ToArray()));
                DeviceLoader.KrakenX?.ApplyEffect(DeviceLoader.KrakenX.Ring, new NZXTSharp.Fixed(krakenringcolors.ToArray()));

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
    }
}
