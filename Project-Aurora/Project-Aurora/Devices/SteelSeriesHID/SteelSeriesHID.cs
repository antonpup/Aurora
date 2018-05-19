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
        private bool HasScrollWheelLed = false;
        private bool HasLogoLed = false;
        private bool HasBacklightLed = false;
        private byte[] LogoCommand;
        private byte[] LogoCommandSuffix;
        private byte[] ScrollCommand;
        private byte[] ScrollCommandSuffix;
        private byte[] BacklightCommand;
        private byte[] BacklightCommandSuffix;
        private byte ReportID;

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

                    //Color to byte array
                    byte[] colorArray = { color.R, color.G, color.B };

                    if (token.IsCancellationRequested) return false;
                    else if (Global.Configuration.allow_peripheral_devices && !Global.Configuration.devices_disable_mouse)
                    {
                        if (key.Key == DeviceKeys.Peripheral_Logo)
                        {
                            if (HasLogoLed) { setMouseLogoColor(colorArray); }
                            if (HasBacklightLed) { setMouseBackLightColor(colorArray); }
                        }
                        else if (key.Key == DeviceKeys.Peripheral_ScrollWheel && HasScrollWheelLed)
                        {
                            setMouseScrollWheelColor(colorArray);
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

        private void setMouseScrollWheelColor(byte[] color)
        {
            HidReport report = mouse.CreateReport();
            report.ReportId = ReportID;
            report.Data = ScrollCommand.Concat(color).Concat(ScrollCommandSuffix).ToArray();
            mouse.WriteReportAsync(report);
        }

        private void setMouseLogoColor(byte[] color)
        {
            HidReport report = mouse.CreateReport();
            report.ReportId = ReportID;
            report.Data = LogoCommand.Concat(color).Concat(LogoCommandSuffix).ToArray();
            mouse.WriteReportAsync(report);
        }

        private void setMouseBackLightColor(byte[] color)
        {
            HidReport report = mouse.CreateReport();
            report.ReportId = ReportID;
            report.Data = BacklightCommand.Concat(color).Concat(BacklightCommandSuffix).ToArray();
            mouse.WriteReportAsync(report);
        }

        private void init()
        {
            if (HidDevices.Enumerate(0x1038, 0x1710).FirstOrDefault() != null) //SteelSeries Rival 300
            {
                devicename = "SteelSeries Rival 300";
                HasScrollWheelLed = true;
                HasLogoLed = true;
                HasBacklightLed = false;
                ReportID = 0x02;
                ScrollCommand = new byte[] { 0x08, 0x02 };
                ScrollCommandSuffix = new byte[] { 0x00 };
                LogoCommand = new byte[] { 0x08, 0x01 };
                LogoCommandSuffix = new byte[] { 0x00 };
                //BacklightCommand = new byte[] { 0x00 };
                //byte[] BacklightCommandSuffix = new byte[] { 0x00 };

                mouse = HidDevices.Enumerate(0x1038, 0x1710).FirstOrDefault();
                mouse.OpenDevice();
                isInitialized = true;

            }

            else if (HidDevices.Enumerate(0x1038, 0x171A).FirstOrDefault() != null) //SteelSeries Rival 300 CS:GO Hyperbeast Edition
            {
                devicename = "SteelSeries Rival 300 CS:GO Hyperbeast Edition";
                HasScrollWheelLed = true;
                HasLogoLed = true;
                HasBacklightLed = false;
                ReportID = 0x02;
                ScrollCommand = new byte[] { 0x08, 0x02 };
                ScrollCommandSuffix = new byte[] { 0x00 };
                LogoCommand = new byte[] { 0x08, 0x01 };
                LogoCommandSuffix = new byte[] { 0x00 };
                //BacklightCommand = new byte[] { 0x00 };
                //BacklightCommandSuffix = new byte[] { 0x00 };

                mouse = HidDevices.Enumerate(0x1038, 0x171A).FirstOrDefault();
                mouse.OpenDevice();
                isInitialized = true;
            }

            else if (HidDevices.Enumerate(0x1038, 0x1394).FirstOrDefault() != null) //SteelSeries Rival 300 CS:GO Fade Edition
            {
                devicename = "SteelSeries Rival 300 CS:GO Fade Edition";
                HasScrollWheelLed = true;
                HasLogoLed = true;
                HasBacklightLed = false;
                ReportID = 0x02;
                byte[] ScrollCommand = new byte[] { 0x08, 0x02 };
                byte[] ScrollCommandSuffix = new byte[] { 0x00 };
                byte[] LogoCommand = new byte[] { 0x08, 0x01 };
                byte[] LogoCommandSuffix = new byte[] { 0x00 };
                //byte[] BacklightCommand = new byte[] { 0x00 };
                //byte[] BacklightCommandSuffix = new byte[] { 0x00 };

                mouse = HidDevices.Enumerate(0x1038, 0x1394).FirstOrDefault();
                mouse.OpenDevice();
                isInitialized = true;
            }

            else if (HidDevices.Enumerate(0x1038, 0x1384).FirstOrDefault() != null) //SteelSeries Rival
            {
                devicename = "SteelSeries Rival";
                HasScrollWheelLed = true;
                HasLogoLed = true;
                HasBacklightLed = false;
                ReportID = 0x02;
                byte[] ScrollCommand = new byte[] { 0x08, 0x02 };
                byte[] ScrollCommandSuffix = new byte[] { 0x00 };
                byte[] LogoCommand = new byte[] { 0x08, 0x01 };
                byte[] LogoCommandSuffix = new byte[] { 0x00 };
                //byte[] BacklightCommand = new byte[] { 0x00 };
                //byte[] BacklightCommandSuffix = new byte[] { 0x00 };

                mouse = HidDevices.Enumerate(0x1038, 0x1384).FirstOrDefault();
                mouse.OpenDevice();
                isInitialized = true;
            }

            else if (HidDevices.Enumerate(0x1038, 0x1702).FirstOrDefault() != null) //SteelSeries Rival 100
            {
                devicename = "SteelSeries Rival 100";
                HasScrollWheelLed = false;
                HasLogoLed = false;
                HasBacklightLed = true;
                ReportID = 0x02;
                //byte[] ScrollCommand = new byte[] { 0x00 };
                //byte[] ScrollCommandSuffix = new byte[] { 0x00 };
                //byte[] LogoCommand = new byte[] { 0x00 };
                //byte[] LogoCommandSuffix = new byte[] { 0x00 };
                byte[] BacklightCommand = new byte[] { 0x05, 0x00 };
                byte[] BacklightCommandSuffix = new byte[] { 0x00 };

                mouse = HidDevices.Enumerate(0x1038, 0x1702).FirstOrDefault();
                mouse.OpenDevice();
                isInitialized = true;

            }

            else if (HidDevices.Enumerate(0x1038, 0x1729).FirstOrDefault() != null) //SteelSeries Rival 110
            {
                devicename = "SteelSeries Rival 110";
                HasScrollWheelLed = false;
                HasLogoLed = false;
                HasBacklightLed = true;
                ReportID = 0x02;
                //byte[] ScrollCommand = new byte[] { 0x00 };
                //byte[] ScrollCommandSuffix = new byte[] { 0x00 };
                //byte[] LogoCommand = new byte[] { 0x00 };
                //byte[] LogoCommandSuffix = new byte[] { 0x00 };
                byte[] BacklightCommand = new byte[] { 0x05, 0x00 };
                byte[] BacklightCommandSuffix = new byte[] { 0x00 };

                mouse = HidDevices.Enumerate(0x1038, 0x1729).FirstOrDefault();
                mouse.OpenDevice();
                isInitialized = true;
            }

            else if (HidDevices.Enumerate(0x1038, 0x170e).FirstOrDefault() != null) //SteelSeries Rival 500
            {
                devicename = "SteelSeries Rival 500";
                HasScrollWheelLed = true;
                HasLogoLed = true;
                HasBacklightLed = false;
                ReportID = 0x03;
                byte[] ScrollCommand = new byte[] { 0x05, 0x00, 0x01 };
                byte[] ScrollCommandSuffix = new byte[] { 0xFF, 0x32, 0xC8, 0xC8, 0x00, 0x01, 0x01 };
                byte[] LogoCommand = new byte[] { 0x05, 0x00, 0x00 };
                byte[] LogoCommandSuffix = new byte[] { 0xFF, 0x32, 0xC8, 0xC8, 0x00, 0x00, 0x01 };
                //byte[] BacklightCommand = new byte[] { 0x00 };
                //byte[] BacklightCommandSuffix = new byte[] { 0x00 };

                mouse = HidDevices.Enumerate(0x1038, 0x170e).FirstOrDefault();
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
