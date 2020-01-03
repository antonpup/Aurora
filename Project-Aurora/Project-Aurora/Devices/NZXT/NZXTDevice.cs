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
using System.Diagnostics;

namespace Aurora.Devices.NZXT
{
    public class NZXTDeviceData
    {
        public byte[] Colors { get; private set; }
        public bool Changed { get; set; }

        public NZXTDeviceData(int leds)
        {
            Colors = new byte[leds * 3];
            Changed = false;
        }

        public void SetColor(int index, Color clr)
        {
            if (index % 3 != 0 || index < 0 || index > Colors.Length - 2)
                throw new ArgumentOutOfRangeException();

            UpdateValueAt(index, clr.G);
            UpdateValueAt(index + 1,clr.R);
            UpdateValueAt(index + 2,clr.B);
        }

        private void UpdateValueAt(int index, byte value)
        {
            if(Colors[index] != value)
            {
                Changed = true;
                Colors[index] = value;
            }
        }
    }

    class NZXTDevice : Device
    {
        private String devicename = "NZXT (beta)";
        private bool isInitialized = false;

        private DeviceLoader DeviceLoader;
        private NZXTDeviceData kraken = new NZXTDeviceData(8);
        private NZXTDeviceData hueplus = new NZXTDeviceData(40);
        private NZXTDeviceData logo = new NZXTDeviceData(1);

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
                        if(Process.GetProcessesByName("NZXT CAM").Length > 0)
                        {
                            Global.logger.Error("NZXT CAM is running. Ensure that it is not open and try again.");
                            return false;
                        }
                        DeviceLoader = new DeviceLoader(false, DeviceLoadFilter.LightingControllers);
                        DeviceLoader.ThrowExceptions = false;
                        DeviceLoader.Initialize();

                        if (DeviceLoader.NumDevices == 0)
                        {
                            Global.logger.Error("NZXT device error: No devices found");
                        }
                        else
                        {
                            isInitialized = true;
                            Global.logger.Info("Starting NZXT debug information: Windows Build version: " + System.Runtime.InteropServices.RuntimeInformation.OSDescription);

                            foreach (var device in DeviceLoader.Devices)
                            {
                                if (device is NZXTSharp.KrakenX.KrakenX)
                                    Global.logger.Info("Found KrakenX, firmware version: " + (device as NZXTSharp.KrakenX.KrakenX).FirmwareVersion ?? "");
                                if (device is NZXTSharp.HuePlus.HuePlus)
                                    Global.logger.Info("Found HuePlus, firmware version: " + (device as NZXTSharp.HuePlus.HuePlus).FirmwareVersion ?? "");
                            }

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
            if (e.Cancel) return false;

            try
            {
                foreach (KeyValuePair<DeviceKeys, Color> key in keycolors)
                {
                    int index;
                    if (e.Cancel) return false;

                    if (key.Key == DeviceKeys.Peripheral_Logo)
                    {
                        logo.SetColor(0, key.Value);
                    }

                    if((index = GetLedIndex(NZXTLayoutMap.HuePlus, key.Key)) != -1)
                    {
                        hueplus.SetColor(index, key.Value);
                    }

                    if ((index = GetLedIndex(NZXTLayoutMap.KrakenX, key.Key)) != -1)
                    {
                        kraken.SetColor(index, key.Value);
                    }
                }

                if (hueplus.Changed || forced)
                {
                    DeviceLoader.HuePlus?.ApplyEffect(DeviceLoader.HuePlus.Both, new NZXTSharp.Fixed(hueplus.Colors));
                    hueplus.Changed = false;
                }

                if (kraken.Changed || forced)
                {
                    DeviceLoader.KrakenX?.ApplyEffect(DeviceLoader.KrakenX.Ring, new NZXTSharp.Fixed(kraken.Colors));
                    kraken.Changed = false;
                }

                if (logo.Changed || forced)
                {
                    DeviceLoader.KrakenX?.ApplyEffect(DeviceLoader.KrakenX.Logo, new NZXTSharp.Fixed(logo.Colors));
                    logo.Changed = false;
                }

                Thread.Sleep(16);//limiting the update speed this way for now, might fry the devices otherwise

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

        private int GetLedIndex(Dictionary<DeviceKeys, int> layout, DeviceKeys key)
        {
            if (layout.ContainsKey(key))
                return layout[key];

            return -1;
        }
    }
}
