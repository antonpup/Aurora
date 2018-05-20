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
        private string devicename = "SteelSeriesHID";
        private bool isInitialized = false;
        private bool peripheral_updated = false;
        private readonly object action_lock = new object();
        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;
        private bool rival100Found;
        private bool rival300Found;
        private bool rival500Found;
        private bool rival700Found;

        Rival100 _rival100 = new Rival100();
        Rival300 _rival300 = new Rival300();
        Rival500 _rival500 = new Rival500();
        Rival700 _rival700 = new Rival700();

        public bool Initialize()
        {
            lock (action_lock)
            {
                if (!isInitialized)
                {
                    try
                    {

                        if (_rival100.Connect()) { rival100Found = true; isInitialized = true; }
                        if (_rival300.Connect()) { rival300Found = true; isInitialized = true; }
                        if (_rival500.Connect()) { rival500Found = true; isInitialized = true; }


                    }
                    catch (Exception e)
                    {
                        Global.logger.Error("SteelSeriesHID could not be initialized: " + e);
                        isInitialized = false;
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
            return this.isInitialized;
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
                    color = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));

                    if (token.IsCancellationRequested) return false;
                    else if (Global.Configuration.allow_peripheral_devices && !Global.Configuration.devices_disable_mouse)
                    {
                        if (key.Key == DeviceKeys.Peripheral_Logo)
                        {
                            if (rival100Found) { _rival100.SetLedColor1(color.R, color.G, color.B); }
                            if (rival300Found) { _rival300.SetLedColor1(color.R, color.G, color.B); }
                            if (rival500Found) { _rival500.SetLedColor1(color.R, color.G, color.B); }
                        }
                        else if (key.Key == DeviceKeys.Peripheral_ScrollWheel)
                        {
                            if (rival300Found) { _rival300.SetLedColor2(color.R, color.G, color.B); }
                            if (rival500Found) { _rival500.SetLedColor1(color.R, color.G, color.B); }
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

    }


    class Rival100
    {
        private static HidDevice mouse;

        public bool Connect()
        {
            if (HidDevices.Enumerate(0x1038, 0x1702).FirstOrDefault() != null) //SteelSeries Rival 100
            {
                mouse = HidDevices.Enumerate(0x1038, 0x1702).FirstOrDefault();

                try
                {
                    mouse.OpenDevice();
                    return true;
                }
                catch
                {
                    return false;
                }

            }
            else if (HidDevices.Enumerate(0x1038, 0x1729).FirstOrDefault() != null) //SteelSeries Rival 110
            {
                mouse = HidDevices.Enumerate(0x1038, 0x1729).FirstOrDefault();

                try
                {
                    mouse.OpenDevice();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private bool Disconnect()
        {
            try
            {
                mouse.CloseDevice();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void SetLedColor1(byte r, byte g, byte b)
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
    }


    class Rival300
    {
        private static HidDevice mouse;

        public bool Connect()
        {
            if (HidDevices.Enumerate(0x1038, 0x1710).FirstOrDefault() != null) //SteelSeries Rival 300
            {
                mouse = HidDevices.Enumerate(0x1038, 0x1710).FirstOrDefault();

                try
                {
                    mouse.OpenDevice();
                    return true;
                }
                catch
                {
                    return false;
                }

            }
            else if (HidDevices.Enumerate(0x1038, 0x171A).FirstOrDefault() != null) //SteelSeries Rival 300 CS:GO Hyperbeast Edition
            {
                mouse = HidDevices.Enumerate(0x1038, 0x171A).FirstOrDefault();

                try
                {
                    mouse.OpenDevice();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else if (HidDevices.Enumerate(0x1038, 0x1394).FirstOrDefault() != null) //SteelSeries Rival 300 CS:GO Fade Edition
            {
                mouse = HidDevices.Enumerate(0x1038, 0x1394).FirstOrDefault();

                try
                {
                    mouse.OpenDevice();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else if (HidDevices.Enumerate(0x1038, 0x1384).FirstOrDefault() != null) //SteelSeries Rival
            {
                mouse = HidDevices.Enumerate(0x1038, 0x1384).FirstOrDefault();

                try
                {
                    mouse.OpenDevice();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void SetLedColor2(byte r, byte g, byte b)
        {
            HidReport report = mouse.CreateReport();
            report.ReportId = 0x02;
            report.Data[0] = 0x08;
            report.Data[1] = 0x02;
            report.Data[2] = r;
            report.Data[3] = g;
            report.Data[4] = b;
            mouse.WriteReportAsync(report);
        }

        public void SetLedColor1(byte r, byte g, byte b)
        {
            HidReport report = mouse.CreateReport();
            report.ReportId = 0x02;
            report.Data[0] = 0x08;
            report.Data[1] = 0x01;
            report.Data[2] = r;
            report.Data[3] = g;
            report.Data[4] = b;
            mouse.WriteReportAsync(report);
        }
    }


    class Rival500
    {
        private static HidDevice mouse;

        public bool Connect()
        {
            if (HidDevices.Enumerate(0x1038, 0x170e).FirstOrDefault() != null) //SteelSeries Rival 500
            {
                mouse = HidDevices.Enumerate(0x1038, 0x170e).FirstOrDefault();

                try
                {
                    mouse.OpenDevice();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void setLedColor2(byte r, byte g, byte b)
        {
            HidReport report = mouse.CreateReport();
            report.ReportId = 0x03;
            report.Data[0] = 0x05;
            report.Data[1] = 0x00;
            report.Data[2] = 0x01;
            report.Data[3] = r;
            report.Data[4] = g;
            report.Data[5] = b;
            report.Data[6] = 0xFF;
            report.Data[7] = 0x32;
            report.Data[8] = 0xC8;
            report.Data[9] = 0xC8;
            report.Data[10] = 0x00;
            report.Data[11] = 0x01;
            report.Data[12] = 0x01;
            mouse.WriteReportAsync(report);
        }

        public void SetLedColor1(byte r, byte g, byte b)
        {
            HidReport report = mouse.CreateReport();
            report.ReportId = 0x03;
            report.Data[0] = 0x05;
            report.Data[1] = 0x00;
            report.Data[2] = 0x00;
            report.Data[3] = r;
            report.Data[4] = g;
            report.Data[5] = b;
            report.Data[6] = 0xFF;
            report.Data[7] = 0x32;
            report.Data[8] = 0xC8;
            report.Data[9] = 0xC8;
            report.Data[10] = 0x00;
            report.Data[11] = 0x00;
            report.Data[12] = 0x01;

            mouse.WriteReportAsync(report);
        }

    }
    class Rival700
    {
        //Not reverse engineered yet...
    }

}