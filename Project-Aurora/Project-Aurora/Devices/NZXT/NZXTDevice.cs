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
                        this.Reset();
                        hueplus.Dispose();
                        krakenx.Dispose();
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

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyhuepluscolors, DoWorkEventArgs e, bool forced = false)
        {
            var huepluscolors = new List<byte>();
            var krakenringcolors = new List<byte>();

            if (e.Cancel) return false;

            try
            {
                foreach (KeyValuePair<DeviceKeys, Color> key in keyhuepluscolors)
                {
                    if (e.Cancel) return false;
                    if(key.Key == DeviceKeys.Peripheral_Logo)
                    {
                        SendColorToKraken(key.Value);
                    }
                    else if(key.Key == DeviceKeys.ONE)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                        krakenringcolors.Add(key.Value.G);
                        krakenringcolors.Add(key.Value.R);
                        krakenringcolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.TWO)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                        krakenringcolors.Add(key.Value.G);
                        krakenringcolors.Add(key.Value.R);
                        krakenringcolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.THREE)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                        krakenringcolors.Add(key.Value.G);
                        krakenringcolors.Add(key.Value.R);
                        krakenringcolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.FOUR)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                        krakenringcolors.Add(key.Value.G);
                        krakenringcolors.Add(key.Value.R);
                        krakenringcolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.FIVE)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                        krakenringcolors.Add(key.Value.G);
                        krakenringcolors.Add(key.Value.R);
                        krakenringcolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.SIX)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                        krakenringcolors.Add(key.Value.G);
                        krakenringcolors.Add(key.Value.R);
                        krakenringcolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.SEVEN)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                        krakenringcolors.Add(key.Value.G);
                        krakenringcolors.Add(key.Value.R);
                        krakenringcolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.EIGHT)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                        krakenringcolors.Add(key.Value.G);
                        krakenringcolors.Add(key.Value.R);
                        krakenringcolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.NINE)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.ZERO)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.MINUS)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.W)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.E)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.R)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.T)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.Y)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.U)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.I)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.O)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.P)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.A)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.S)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.D)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.F)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.G)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.H)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.J)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.K)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.L)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.SEMICOLON)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.BACKSLASH)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.Z)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.X)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.C)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.V)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.B)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.N)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.M)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.COMMA)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                    else if (key.Key == DeviceKeys.PERIOD)
                    {
                        huepluscolors.Add(key.Value.G);
                        huepluscolors.Add(key.Value.R);
                        huepluscolors.Add(key.Value.B);
                    }
                }

                var huecolorarray = huepluscolors.ToArray();
                SendColorToHuePlus(huecolorarray, huecolorarray);
                SendColorToKraken(krakenringcolors.ToArray());


                return true;
            }
            catch (Exception ex)
            {
                Global.logger.Error("NZXT, error when updating device: " + ex);
                return false;
            }
        }

        private void SendColorToHuePlus(byte[] ch1, byte[] ch2)
        {
            if (hueplus != null)
            {
                hueplus.ApplyEffect(hueplus.Channel1, new NZXTSharp.Fixed(ch1));
                hueplus.ApplyEffect(hueplus.Channel2, new NZXTSharp.Fixed(ch2));
            }
        }

        private void SendColorToKraken(byte[] ring)
        {
            if (krakenx != null)
            {
                krakenx.ApplyEffect(krakenx.Ring, new NZXTSharp.Fixed(ring));
            }
        }

        private void SendColorToKraken(Color logo)
        {
            if (krakenx != null)
            {
                krakenx.ApplyEffect(krakenx.Logo, new NZXTSharp.Fixed(new NZXTSharp.Color(logo.R, logo.G, logo.B)));
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
