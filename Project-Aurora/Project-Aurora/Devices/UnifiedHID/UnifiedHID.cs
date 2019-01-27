using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aurora.Settings;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;
using System.ComponentModel;

namespace Aurora.Devices.UnifiedHID
{

    class UnifiedHIDDevice : Device
    {
        private string devicename = "UnifiedHID";
        private bool isInitialized = false;
        private bool peripheral_updated = false;
        private readonly object action_lock = new object();
        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;
        List<ISSDevice> AllDevices = new List<ISSDevice> {
            new Rival100(),
            new Rival110(),
            new Rival300(),
            new Rival500(),
            new AsusPugio()
        };
        List<ISSDevice> FoundDevices = new List<ISSDevice>();

        public bool Initialize()
        {
            lock (action_lock)
            {
                if (!isInitialized)
                {
                    this.FoundDevices.Clear();
                    try
                    {
                        foreach (ISSDevice dev in AllDevices)
                        {
                            if (dev.Connect())
                                FoundDevices.Add(dev);
                        }
                    }
                    catch (Exception e)
                    {
                        Global.logger.Error("UnifiedHID could not be initialized: " + e);
                        isInitialized = false;
                    }
                    if (FoundDevices.Count > 0)
                        isInitialized = true;
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
                        foreach (ISSDevice dev in FoundDevices)
                        {
                            dev.Disconnect();
                        }
                        this.FoundDevices.Clear();
                        this.Reset();

                        isInitialized = false;
                    }
                }
                catch (Exception ex)
                {
                    Global.logger.Error("There was an error shutting down UnifiedHID: " + ex);
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
            Shutdown();
            return Initialize();
        }

        public bool IsConnected()
        {
            return this.isInitialized;
        }

        public bool IsInitialized()
        {
            return this.isInitialized;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel) return false;
            try
            {
                List<Tuple<byte, byte, byte>> colors = new List<Tuple<byte, byte, byte>>();

                foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                {
                    Color color = (Color)key.Value;
                    //Apply and strip Alpha
                    color = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));

                    if (e.Cancel) return false;
                    else if (Global.Configuration.allow_peripheral_devices && !Global.Configuration.devices_disable_mouse)
                    {
                        if (key.Key == DeviceKeys.Peripheral_Logo || key.Key == DeviceKeys.Peripheral_ScrollWheel || key.Key == DeviceKeys.Peripheral_FrontLight)
                        {
                            foreach (ISSDevice device in FoundDevices)
                                device.SetLEDColour(key.Key, color.R, color.G, color.B);
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
                Global.logger.Error("UnifiedHID, error when updating device: " + ex);
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

    interface ISSDevice
    {
        bool IsConnected { get; }
        bool Connect();
        bool Disconnect();
        bool SetLEDColour(DeviceKeys key, byte red, byte green, byte blue);
    }

    abstract class UnifiedBase : ISSDevice
    {
        protected HidDevice device;
        protected Dictionary<DeviceKeys, Func<byte, byte, byte, bool>> deviceKeyMap;
        public bool IsConnected { get; protected set; } = false;

        protected bool Connect(int vendorID, int[] productIDs, short usagePage)
        {
            IEnumerable<HidDevice> devices = HidDevices.Enumerate(vendorID, productIDs);

            if (devices.Count() > 0)
            {
                device = devices.FirstOrDefault(dev => dev.Capabilities.UsagePage == usagePage);
                try
                {
                    device.OpenDevice();
                    return (IsConnected = true);
                }
                catch (Exception exc)
                {
                    Global.logger.LogLine($"Error when attempting to open UnifiedHID device:\n{exc}", Logging_Level.Error);
                }
            }
            return false;
        }

        public abstract bool Connect();

        public virtual bool Disconnect()
        {
            try
            {
                device.CloseDevice();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SetLEDColour(DeviceKeys key, byte red, byte green, byte blue)
        {
            if (this.deviceKeyMap.TryGetValue(key, out Func<byte, byte, byte, bool> func))
                return func.Invoke(red, green, blue);

            return false;
        }
    }


    class Rival100 : UnifiedBase
    {
        public Rival100()
        {
            deviceKeyMap = new Dictionary<DeviceKeys, Func<byte, byte, byte, bool>>
            {
                { DeviceKeys.Peripheral_Logo, SetLogo }
            };
        }

        public override bool Connect()
        {
            return this.Connect(0x1038, new[] { 0x1702 }, unchecked((short)0xFFFFFFC0));
        }

        public bool SetLogo(byte r, byte g, byte b)
        {
            HidReport report = device.CreateReport();
            report.ReportId = 0x02;
            report.Data[0] = 0x05;
            report.Data[1] = 0x00;
            report.Data[2] = r;
            report.Data[3] = g;
            report.Data[4] = b;
            return device.WriteReport(report);
        }
    }


    class Rival110 : UnifiedBase
    {
        public Rival110()
        {
            deviceKeyMap = new Dictionary<DeviceKeys, Func<byte, byte, byte, bool>>
            {
                { DeviceKeys.Peripheral_Logo, SetLogo }
            };
        }

        public override bool Connect()
        {
            return this.Connect(0x1038, new[] { 0x1729 }, unchecked((short)0xFFFFFFC0));
        }

        public bool SetLogo(byte r, byte g, byte b)
        {
            HidReport report = device.CreateReport();
            report.ReportId = 0x02;
            report.Data[0] = 0x05;
            report.Data[1] = 0x00;
            report.Data[2] = r;
            report.Data[3] = g;
            report.Data[4] = b;
            report.Data[5] = 0x00;
            report.Data[6] = 0x00;
            report.Data[7] = 0x00;
            report.Data[8] = 0x00;

            return device.WriteReport(report);
        }
    }


    class Rival300 : UnifiedBase
    {
        public Rival300()
        {
            this.deviceKeyMap = new Dictionary<DeviceKeys, Func<byte, byte, byte, bool>>
            {
                { DeviceKeys.Peripheral_Logo, SetLogo },
                { DeviceKeys.Peripheral_ScrollWheel, SetScrollWheel }
            };
        }

        public override bool Connect()
        {
            return this.Connect(0x1038, new[] { 0x1710, 0x171A, 0x1394, 0x1384, 0x1718, 0x1712 }, unchecked((short)0xFFFFFFC0));
        }

        public bool SetScrollWheel(byte r, byte g, byte b)
        {
            HidReport report = device.CreateReport();
            report.ReportId = 0x02;
            report.Data[0] = 0x08;
            report.Data[1] = 0x02;
            report.Data[2] = r;
            report.Data[3] = g;
            report.Data[4] = b;
            return device.WriteReport(report);
        }

        public bool SetLogo(byte r, byte g, byte b)
        {
            HidReport report = device.CreateReport();
            report.ReportId = 0x02;
            report.Data[0] = 0x08;
            report.Data[1] = 0x01;
            report.Data[2] = r;
            report.Data[3] = g;
            report.Data[4] = b;
            return device.WriteReport(report);
        }
    }


    class Rival500 : UnifiedBase
    {
        public Rival500()
        {
            this.deviceKeyMap = new Dictionary<DeviceKeys, Func<byte, byte, byte, bool>>
            {
                { DeviceKeys.Peripheral_Logo, SetLogo },
                { DeviceKeys.Peripheral_ScrollWheel, SetScrollWheel }
            };
        }

        public override bool Connect()
        {
            return this.Connect(0x1038, new[] { 0x170e }, unchecked((short)0xFFFFFFC0));
        }

        public bool SetScrollWheel(byte r, byte g, byte b)
        {
            HidReport report = device.CreateReport();
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
            return device.WriteReport(report);
        }

        public bool SetLogo(byte r, byte g, byte b)
        {
            HidReport report = device.CreateReport();
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

            return device.WriteReport(report);
        }

    }


    class AsusPugio : UnifiedBase
    {
        public AsusPugio()
        {
            this.deviceKeyMap = new Dictionary<DeviceKeys, Func<byte, byte, byte, bool>>
            {
                { DeviceKeys.Peripheral_Logo, SetLogo },
                { DeviceKeys.Peripheral_ScrollWheel, SetScrollWheel },
                { DeviceKeys.Peripheral_FrontLight, SetBottomLed }
            };
        }

        public override bool Connect()
        {
            return this.Connect(0x0b05, new[] { 0x1846, 0x1847 }, unchecked((short)0xFFFFFF01));
        }

        public bool SetScrollWheel(byte r, byte g, byte b)
        {
            HidReport report = device.CreateReport();
            report.ReportId = 0x00;
            for (int i = 0; i < 64; i++)
            {
                report.Data[i] = 0x00;
            }
            report.Data[0] = 0x51;
            report.Data[1] = 0x28;
            report.Data[2] = 0x01;
            report.Data[4] = 0x00;
            report.Data[5] = 0x04;
            report.Data[6] = r;
            report.Data[7] = g;
            report.Data[8] = b;
            return device.WriteReport(report);
        }

        public bool SetLogo(byte r, byte g, byte b)
        {
            SetBottomLed(r,g,b);
            HidReport report = device.CreateReport();
            report.ReportId = 0x00;
            for (int i = 0; i < 64; i++)
            {
                report.Data[i] = 0x00;
            }
            report.Data[0] = 0x51;
            report.Data[1] = 0x28;
            report.Data[2] = 0x00;
            report.Data[4] = 0x00;
            report.Data[5] = 0x04;
            report.Data[6] = r;
            report.Data[7] = g;
            report.Data[8] = b;
            return device.WriteReport(report);
        }

        public bool SetBottomLed(byte r, byte g, byte b)
        {
            HidReport report = device.CreateReport();
            report.ReportId = 0x00;
            for (int i = 0; i < 64; i++)
            {
                report.Data[i] = 0x00;
            }
            report.Data[0] = 0x51;
            report.Data[1] = 0x28;
            report.Data[2] = 0x02;
            report.Data[4] = 0x00;
            report.Data[5] = 0x04;
            report.Data[6] = r;
            report.Data[7] = g;
            report.Data[8] = b;
            return device.WriteReport(report);
        }
    }

}