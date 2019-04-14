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

        NZXTSharp.HuePlus.HuePlus hueplus;
        NZXTSharp.KrakenX.KrakenX krakenx;

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
                return devicename + ": " + (hueplus != null ? "HuePlus " : "") + (krakenx != null ? "KrakenX " : "");
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
                        hueplus = new NZXTSharp.HuePlus.HuePlus();
                        Global.logger.Info("NZXT device HuePlus: Initialized");
                    }
                    catch (Exception ex)
                    {
                        Global.logger.Error("NZXT device HuePlus, Exception! Message: " + ex);
                    }

                    try
                    {
                        krakenx = new NZXTSharp.KrakenX.KrakenX();
                        Global.logger.Info("NZXT device KrakenX: Initialized");
                    }
                    catch (Exception ex)
                    {
                        Global.logger.Error("NZXT device KrakenX, Exception! Message: " + ex);
                    }

                    if (hueplus == null && krakenx == null)
                    {
                        Global.logger.Error("NZXT device error: No devices found");
                    }
                    else
                    {
                        isInitialized = true;
                        return true;
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
                        hueplus?.Dispose();
                        krakenx?.Dispose();
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
            var krakenringcolors = new List<byte>(8);

            if (e.Cancel) return false;

            try
            {
                foreach (KeyValuePair<DeviceKeys, Color> key in keycolors)
                {
                    if (e.Cancel) return false;
                    if(key.Key == DeviceKeys.Peripheral_Logo)
                    {
                        krakenx?.ApplyEffect(krakenx.Logo, new NZXTSharp.Fixed(new NZXTSharp.Color(key.Value.R, key.Value.G, key.Value.B)));
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
                    else if (key.Key >= DeviceKeys.NINE && key.Key < DeviceKeys.ZERO       || 
                             key.Key >= DeviceKeys.Q    && key.Key <= DeviceKeys.P         || 
                             key.Key >= DeviceKeys.A    && key.Key <= DeviceKeys.SEMICOLON || 
                             key.Key >= DeviceKeys.Z    && key.Key <= DeviceKeys.PERIOD     )//40 keys
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                }

                hueplus?.ApplyEffect(hueplus.Both, new NZXTSharp.Fixed(huepluscolors.ToArray()));
                krakenx?.ApplyEffect(krakenx.Ring, new NZXTSharp.Fixed(krakenringcolors.ToArray()));

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
