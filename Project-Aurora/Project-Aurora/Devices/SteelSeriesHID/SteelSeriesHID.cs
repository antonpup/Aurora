using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aurora.Settings;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;

namespace Aurora.Devices.SteelSeriesHID
{

    class SteelSeriesHIDDevice : Device
    {
        private String devicename = "SteelSeriesHID";
        int mouseType; //Mouse type, 1 = Rival 300 , 2 = null , 3 = null . Support for other Rival mouses will come
        private bool isInitialized = false;
        private static HidDevice mouse;
        private bool peripheral_updated = false;
        private readonly object action_lock = new object();
        private Stopwatch watch = new Stopwatch();
        private long lastUpdateTime = 0;


        public bool Initialize()
        {
            lock (action_lock)
            {
                if (!isInitialized)
                {
                    try
                    {
                        init();

                        isInitialized = true;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Global.logger.Error("SteelSeriesHID could not be initialized: " + ex);

                        isInitialized = false;
                        return false;
                    }
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
                        mouse.CloseDevice();
                        this.Reset();

                        isInitialized = false;
                    }
                }
                catch (Exception ex)
                {
                    Global.logger.Error("There was an error shutting down SteelSeriesHID: " + ex);
                    isInitialized = false;
                }

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
            if (this.IsInitialized() && (peripheral_updated))
            {

                peripheral_updated = false;
            }
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool IsInitialized()
        {

            return this.isInitialized;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, CancellationToken token, bool forced = false)
        {
            if (token.IsCancellationRequested) return false;

            try
            {
                if (token.IsCancellationRequested) return false;
                List<Tuple<byte, byte, byte>> colors = new List<Tuple<byte, byte, byte>>();

                foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                {
                    if (token.IsCancellationRequested) return false;

                    Color color = (Color)key.Value;
                    //Apply and strip Alpha
                    color = Color.FromArgb(255,
                        Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));

                    if (token.IsCancellationRequested) return false;

                    else if (key.Key == DeviceKeys.Peripheral_Logo ||
                             key.Key == DeviceKeys.Peripheral_FrontLight ||
                             key.Key == DeviceKeys.Peripheral_ScrollWheel)
                    {
                        SendColorToPeripheralZone(key.Key, color);
                    }
                }


                return true;
            }
            catch (Exception ex)
            {
                Global.logger.Error("SteelSeriesHID, error when updating device: " + ex);
                return false;
            }
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, CancellationToken token, bool forced = false)
        {
            watch.Restart();

            bool update_result = UpdateDevice(colorComposition.keyColors, token, forced);

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

        private void SendColorToPeripheralZone(DeviceKeys zone, Color color)
        {
            if (Global.Configuration.allow_peripheral_devices && !Global.Configuration.devices_disable_mouse)
            {
                if (zone == DeviceKeys.Peripheral_Logo)
                {
                    setMouseLogoColor(color.R, color.G, color.B);
                }
                else if (zone == DeviceKeys.Peripheral_ScrollWheel)
                {
                    setMouseScrollWheelColor(color.R, color.G, color.B);
                }
                peripheral_updated = true;
            }
            else
            {
                peripheral_updated = false;
            }
        }

        private void setMouseScrollWheelColor(byte r, byte g, byte b)
        {
            var report = mouse.CreateReport();
            if (mouseType == 1)
            {
                report.ReportId = 0x02;
                report.Data[0] = 0x08;
                report.Data[1] = 0x02;
                report.Data[2] = r;
                report.Data[3] = g;
                report.Data[4] = b;
                mouse.WriteReport(report);
            }
        }

        private void setMouseLogoColor(byte r, byte g, byte b)
        {
            var report = mouse.CreateReport();
            if (mouseType == 1)
            {
                report.ReportId = 0x02;
                report.Data[0] = 0x08;
                report.Data[1] = 0x01;
                report.Data[2] = r;
                report.Data[3] = g;
                report.Data[4] = b;
                mouse.WriteReport(report);
            }
        }
        private void init()
        {
            mouse = HidDevices.Enumerate(0x1038, 0x1710).FirstOrDefault();
            if (mouse != null)
            {
                mouse.OpenDevice();
                isInitialized = true;
                mouseType = 1;
                return;
            }

        }
    }
}
