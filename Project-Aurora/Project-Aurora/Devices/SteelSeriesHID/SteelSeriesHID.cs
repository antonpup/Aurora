using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aurora.Settings;

using System.Threading;
using System.Threading.Tasks;
using HidLibrary;

namespace Aurora.Devices.SteelSeriesHID
{

    class SteelSeriesHIDDevice : Device
    {
        private String devicename = "SteelSeriesHID";
        private bool isInitialized = false;
        private static HidDevice mouse;
        private int mouseType;
        private bool peripheral_updated = false;
        private readonly object action_lock = new object();
        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
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
                List<Tuple<byte, byte, byte>> colors = new List<Tuple<byte, byte, byte>>();

                foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                {
                    Color color = (Color)key.Value;
                    //Apply and strip Alpha
                    color = Color.FromArgb(255,
                        Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));

                    if (token.IsCancellationRequested) return false;
                    else if (Global.Configuration.allow_peripheral_devices && !Global.Configuration.devices_disable_mouse)
                    {
                        if (key.Key == DeviceKeys.Peripheral_Logo)
                        {
                            if (mouseType != 6 && mouseType != 7) { setMouseLogoColor(color.R, color.G, color.B); }
                            if (mouseType == 6 && mouseType == 7) { setMouseColor(color.R, color.G, color.B); }
                        }
                        else if (key.Key == DeviceKeys.Peripheral_ScrollWheel && mouseType != 6 && mouseType != 7)
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

        private void setMouseScrollWheelColor(byte r, byte g, byte b)
        {
            HidReport report = mouse.CreateReport();
            report.Data[0] = 0x08;
            report.Data[1] = 0x02;
            report.Data[2] = r;
            report.Data[3] = g;
            report.Data[4] = b;
            mouse.WriteReportAsync(report);
        }

        private void setMouseLogoColor(byte r, byte g, byte b)
        {
            HidReport report = mouse.CreateReport();
            report.Data[0] = 0x08;
            report.Data[1] = 0x01;
            report.Data[2] = r;
            report.Data[3] = g;
            report.Data[4] = b;
            mouse.WriteReportAsync(report);
        }

        private void setMouseColor(byte r, byte g, byte b)
        {
            HidReport report = mouse.CreateReport();
            report.ReportId = 0x02;
            report.Data[0] = 0x05;
            report.Data[1] = 0x00;
            report.Data[2] = r;
            report.Data[3] = g;
            report.Data[4] = b;
            mouse.WriteReportAsync(report);
        }

        private void init()
        {
            if (HidDevices.Enumerate(0x1038, 0x1710).FirstOrDefault() != null)
            {
                mouseType = 1; //SteelSeries Rival 300
                mouse = HidDevices.Enumerate(0x1038, 0x1710).FirstOrDefault();
                mouse.OpenDevice();
                isInitialized = true;
            }
            else if (HidDevices.Enumerate(0x1038, 0x171A).FirstOrDefault() != null)
            {
                mouseType = 2; //SteelSeries Rival 300 CS:GO Hyperbeast Edition
                mouse = HidDevices.Enumerate(0x1038, 0x171A).FirstOrDefault();
                mouse.OpenDevice();
                isInitialized = true;
            }
            else if (HidDevices.Enumerate(0x1038, 0x1394).FirstOrDefault() != null)
            {
                mouseType = 3; //SteelSeries Rival 300 CS:GO Fade Edition
                mouse = HidDevices.Enumerate(0x1038, 0x1394).FirstOrDefault();
                mouse.OpenDevice();
                isInitialized = true;
            }
            /*else if (HidDevices.Enumerate(0x1038, 0x170E).FirstOrDefault() != null)
            {
                mouseType = 4; //SteelSeries Rival 500 (Experimental)
                mouse = HidDevices.Enumerate(0x1038, 0x170E).FirstOrDefault();
                mouse.OpenDevice();
                isInitialized = true;
            }*/
            else if (HidDevices.Enumerate(0x1038, 0x1384).FirstOrDefault() != null)
            {
                mouseType = 5; //SteelSeries Rival
                mouse = HidDevices.Enumerate(0x1038, 0x1384).FirstOrDefault();
                mouse.OpenDevice();
                isInitialized = true;
            }
            else if (HidDevices.Enumerate(0x1038, 0x1702).FirstOrDefault() != null)
            {

                mouseType = 6; //SteelSeries Rival 100 (only have 1 led)
                mouse = HidDevices.Enumerate(0x1038, 0x1702).FirstOrDefault();
                mouse.OpenDevice();
                isInitialized = true;

            }
            else if (HidDevices.Enumerate(0x1038, 0x1729).FirstOrDefault() != null)
            {
                mouseType = 7; //SteelSeries Rival 110
                mouse = HidDevices.Enumerate(0x1038, 0x1729).FirstOrDefault();
                mouse.OpenDevice();
                isInitialized = true;
            }
            /*else if (HidDevices.Enumerate(0x1038, 0x1720).FirstOrDefault() != null)
            {
                mouseType = 8; //SteelSeries Rival 310 (set_color not reverse engineered yet as I don't own this mouse)
                mouse = HidDevices.Enumerate(0x1038, 0x1720).FirstOrDefault();
            }*/
            else if (HidDevices.Enumerate(0x1038, 0x1384).FirstOrDefault() != null)
            {
                mouseType = 9; //SteelSeries Rival
                mouse = HidDevices.Enumerate(0x1038, 0x1384).FirstOrDefault();
                mouse.OpenDevice();
                isInitialized = true;
            }
            else
            {
                isInitialized = false;
            }

        }
    }

}
