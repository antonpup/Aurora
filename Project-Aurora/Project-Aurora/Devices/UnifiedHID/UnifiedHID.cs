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
        private VariableRegistry default_registry = null;

        List<ISSDevice> AllDevices = new List<ISSDevice> {
            new Rival100(),
            new Rival110(),
            new Rival300(),
            new Rival500(),
            new AsusPugio(),
            new RoccatVulcan()
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

                foreach (ISSDevice device in FoundDevices)
                {
                    if (e.Cancel) return false;

                    if (!device.IsKeyboard)
                    {
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
                                    device.SetLEDColour(key.Key, color.R, color.G, color.B);
                                }
                                peripheral_updated = true;
                            }
                            else
                            {
                                peripheral_updated = false;
                            }
                        }
                    }
                    else
                    {
                        if (!Global.Configuration.devices_disable_keyboard)
                        {
                            device.SetMultipleLEDColour(keyColors);
                            peripheral_updated = true;
                        }
                        else
                        {
                            peripheral_updated = false;
                        }
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
            return isInitialized;
            //return false;
        }

        public string GetDeviceUpdatePerformance()
        {
            return (isInitialized ? lastUpdateTime + " ms" : "");
        }

        public VariableRegistry GetRegisteredVariables()
        {
            if (default_registry == null)
            {
                default_registry = new VariableRegistry();
                foreach (ISSDevice device in AllDevices)
                {
                    default_registry.Register($"UnifiedHID_{device.GetType().Name}_enable", false, $"Enable {device.GetType().Name} in {devicename}");
                }
            }
            return default_registry;
        }

    }

    interface ISSDevice
    {
        bool IsConnected { get; }
        bool IsKeyboard { get; }
        bool Connect();
        bool Disconnect();
        bool SetLEDColour(DeviceKeys key, byte red, byte green, byte blue);
        bool SetMultipleLEDColour(Dictionary<DeviceKeys, Color> keyColors);
    }

    abstract class UnifiedBase : ISSDevice
    {
        protected HidDevice device;
        protected Dictionary<DeviceKeys, Func<byte, byte, byte, bool>> deviceKeyMap;
        public bool IsConnected { get; protected set; } = false;
        public bool IsKeyboard { get; protected set; } = false;

        protected bool Connect(int vendorID, int[] productIDs, short usagePage)
        {
            IEnumerable<HidDevice> devices = HidDevices.Enumerate(vendorID, productIDs);

            if (devices.Count() > 0)
            {
                try
                {
                    device = devices.First(dev => dev.Capabilities.UsagePage == usagePage);
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

        public virtual bool SetMultipleLEDColour(Dictionary<DeviceKeys, Color> keyColors)
        {
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
            if (!Global.Configuration.VarRegistry.GetVariable<bool>($"UnifiedHID_{this.GetType().Name}_enable"))
            {
                return false;
            }

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
            if (!Global.Configuration.VarRegistry.GetVariable<bool>($"UnifiedHID_{this.GetType().Name}_enable"))
            {
                return false;
            }

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
            if (!Global.Configuration.VarRegistry.GetVariable<bool>($"UnifiedHID_{this.GetType().Name}_enable"))
            {
                return false;
            }

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
            if (!Global.Configuration.VarRegistry.GetVariable<bool>($"UnifiedHID_{this.GetType().Name}_enable"))
            {
                return false;
            }

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
            if (!Global.Configuration.VarRegistry.GetVariable<bool>($"UnifiedHID_{this.GetType().Name}_enable"))
            {
                return false;
            }

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
            SetBottomLed(r, g, b);
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


    class RoccatVulcan : UnifiedBase
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static internal extern bool WriteFile(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, [System.Runtime.InteropServices.In] ref System.Threading.NativeOverlapped lpOverlapped);

        private static HidDevice ctrl_device_leds;
        private static HidDevice ctrl_device;
        private static readonly int RV_NUM_KEYS = 144;

        public static Dictionary<DeviceKeys, int> KeyMap = new Dictionary<DeviceKeys, int> {
            { DeviceKeys.ESC, 0 },
            { DeviceKeys.TILDE, 1 },
            { DeviceKeys.TAB, 2 },
            { DeviceKeys.LEFT_SHIFT, 4 },
            { DeviceKeys.CAPS_LOCK, 3 },
            { DeviceKeys.LEFT_CONTROL, 5 },
            { DeviceKeys.LEFT_WINDOWS, 10 },

            { DeviceKeys.ONE, 6 },
            { DeviceKeys.TWO, 12 },
            { DeviceKeys.THREE, 18 },
            { DeviceKeys.FOUR, 24 },
            { DeviceKeys.FIVE, 29 },
            { DeviceKeys.SIX, 33 },
            { DeviceKeys.SEVEN, 49 },
            { DeviceKeys.EIGHT, 54 },
            { DeviceKeys.NINE, 60 },
            { DeviceKeys.ZERO, 66 },

            { DeviceKeys.F1, 11 },
            { DeviceKeys.F2, 17 },
            { DeviceKeys.F3, 23 },
            { DeviceKeys.F4, 28 },
            { DeviceKeys.F5, 48 },
            { DeviceKeys.F6, 53 },
            { DeviceKeys.F7, 59 },
            { DeviceKeys.F8, 65 },
            { DeviceKeys.F9, 78 },
            { DeviceKeys.F10, 84 },
            { DeviceKeys.F11, 85 },
            { DeviceKeys.F12, 86 },


            { DeviceKeys.Q, 7 },
            { DeviceKeys.A, 8 },
            { DeviceKeys.W, 13 },
            { DeviceKeys.S, 14 },
            { DeviceKeys.Z, 15 },

            { DeviceKeys.LEFT_ALT, 16 },

            { DeviceKeys.E, 19 },
            { DeviceKeys.D, 20 },
            { DeviceKeys.X, 21 },
            { DeviceKeys.R, 25 },
            { DeviceKeys.C, 27 },
            { DeviceKeys.T, 30 },
            { DeviceKeys.G, 31 },
            { DeviceKeys.V, 32 },
            { DeviceKeys.Y, 34 },
            { DeviceKeys.H, 35 },
            { DeviceKeys.B, 36 },

            { DeviceKeys.SPACE, 37 },

            { DeviceKeys.U, 50 },
            { DeviceKeys.J, 51 },
            { DeviceKeys.N, 52 },
            { DeviceKeys.I, 55 },
            { DeviceKeys.K, 56 },
            { DeviceKeys.M, 57 },
            { DeviceKeys.O, 61 },
            { DeviceKeys.L, 62 },
            { DeviceKeys.F, 26 },
            { DeviceKeys.P, 67 },

            { DeviceKeys.COMMA, 63 },
            { DeviceKeys.SEMICOLON, 68 },
            { DeviceKeys.PERIOD, 69 },
            { DeviceKeys.RIGHT_ALT, 70 },
            { DeviceKeys.MINUS, 72 },
            { DeviceKeys.OPEN_BRACKET, 73 },
            { DeviceKeys.APOSTROPHE, 74 },
            { DeviceKeys.FORWARD_SLASH, 75 },
            { DeviceKeys.FN_Key, 76 },
            { DeviceKeys.EQUALS, 79 },
            { DeviceKeys.CLOSE_BRACKET, 80 },
            { DeviceKeys.BACKSLASH, 81 },
            { DeviceKeys.RIGHT_SHIFT, 82 },
            { DeviceKeys.APPLICATION_SELECT, 83 },
            { DeviceKeys.BACKSPACE, 87 },
            { DeviceKeys.ENTER, 88 },
            { DeviceKeys.RIGHT_CONTROL, 89 },

            { DeviceKeys.PRINT_SCREEN, 99 },
            { DeviceKeys.INSERT, 100 },
            { DeviceKeys.DELETE, 101 },
            { DeviceKeys.ARROW_LEFT, 102 },
            { DeviceKeys.SCROLL_LOCK, 103 },
            { DeviceKeys.HOME, 104 },
            { DeviceKeys.END, 105 },
            { DeviceKeys.ARROW_UP, 106 },
            { DeviceKeys.ARROW_DOWN, 107 },
            { DeviceKeys.PAUSE_BREAK, 108 },
            { DeviceKeys.PAGE_UP, 109 },
            { DeviceKeys.PAGE_DOWN, 110 },
            { DeviceKeys.ARROW_RIGHT, 111 },

            { DeviceKeys.NUM_LOCK, 113 },
            { DeviceKeys.NUM_SEVEN, 114 },
            { DeviceKeys.NUM_FOUR, 115 },
            { DeviceKeys.NUM_ONE, 116 },
            { DeviceKeys.NUM_ZERO, 117 },
            { DeviceKeys.NUM_SLASH, 119 },
            { DeviceKeys.NUM_EIGHT, 120 },
            { DeviceKeys.NUM_FIVE, 121 },
            { DeviceKeys.NUM_TWO, 122 },
            { DeviceKeys.NUM_ASTERISK, 124 },
            { DeviceKeys.NUM_NINE, 125 },
            { DeviceKeys.NUM_SIX, 126 },
            { DeviceKeys.NUM_THREE, 127 },
            { DeviceKeys.NUM_PERIOD, 128 },
            { DeviceKeys.NUM_MINUS, 129 },
            { DeviceKeys.NUM_PLUS, 130 },
            { DeviceKeys.NUM_ENTER, 131 }
        };

        // Most of these functions were ported from https://github.com/duncanthrax/roccat-vulcan
        static bool rv_wait_for_ctrl_device()
        {
            for (int i = 1; i < 100; i++) // If still fails after 100 tries then timeout
            {
                // 150ms is the magic number here, should suffice on first try.
                Thread.Sleep(150);
                if (ctrl_device.ReadFeatureData(out byte[] buffer, 0x04) && buffer.Length > 2)
                {
                    if (buffer[1] == 0x01)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        private bool rv_get_ctrl_report(byte report_id)
        {
            return (ctrl_device.ReadFeatureData(out byte[] buffer, report_id) && buffer.Length >= 8);
        }

        #region Buffers for ctrl_reports
        static readonly byte[] ctrl_report_buffer_0x15 = new byte[] { 0x15, 0x00, 0x01 };
        static readonly byte[] ctrl_report_buffer_0x05 = new byte[] { 0x05, 0x04, 0x00, 0x04 };
        static readonly byte[] ctrl_report_buffer_0x07 = new byte[] { 0x07 ,0x5f ,0x00 ,0x3a ,0x00 ,0x00 ,0x3b ,0x00 ,0x00 ,0x3c ,0x00 ,0x00 ,0x3d ,0x00 ,0x00 ,0x3e,
                        0x00 ,0x00 ,0x3f ,0x00 ,0x00 ,0x40 ,0x00 ,0x00 ,0x41 ,0x00 ,0x00 ,0x42 ,0x00 ,0x00 ,0x43 ,0x00,
                        0x00 ,0x44 ,0x00 ,0x00 ,0x45 ,0x00 ,0x00 ,0x46 ,0x00 ,0x00 ,0x47 ,0x00 ,0x00 ,0x48 ,0x00 ,0x00,
                        0xb3 ,0x00 ,0x00 ,0xb4 ,0x00 ,0x00 ,0xb5 ,0x00 ,0x00 ,0xb6 ,0x00 ,0x00 ,0xc2 ,0x00 ,0x00 ,0xc3,
                        0x00 ,0x00 ,0xc0 ,0x00 ,0x00 ,0xc1 ,0x00 ,0x00 ,0xce ,0x00 ,0x00 ,0xcf ,0x00 ,0x00 ,0xcc ,0x00,
                        0x00 ,0xcd ,0x00 ,0x00 ,0x46 ,0x00 ,0x00 ,0xfc ,0x00 ,0x00 ,0x48 ,0x00 ,0x00 ,0xcd ,0x0e };
        static readonly byte[] ctrl_report_buffer_0x0a = new byte[] { 0x0a, 0x08, 0x00, 0xff, 0xf1, 0x00, 0x02, 0x02 };
        static readonly byte[] ctrl_report_buffer_0x0b = new byte[] { 0x0b ,0x41 ,0x00 ,0x1e ,0x00 ,0x00 ,0x1f ,0x00 ,0x00 ,0x20 ,0x00 ,0x00 ,0x21 ,0x00 ,0x00 ,0x22,
                        0x00 ,0x00 ,0x14 ,0x00 ,0x00 ,0x1a ,0x00 ,0x00 ,0x08 ,0x00 ,0x00 ,0x15 ,0x00 ,0x00 ,0x17 ,0x00,
                        0x00 ,0x04 ,0x00 ,0x00 ,0x16 ,0x00 ,0x00 ,0x07 ,0x00 ,0x00 ,0x09 ,0x00 ,0x00 ,0x0a ,0x00 ,0x00,
                        0x1d ,0x00 ,0x00 ,0x1b ,0x00 ,0x00 ,0x06 ,0x00 ,0x00 ,0x19 ,0x00 ,0x00 ,0x05 ,0x00 ,0x00 ,0xde ,0x01};
        static readonly byte[] ctrl_report_buffer_0x06 = new byte[] { 0x06 ,0x85 ,0x00 ,0x3a ,0x29 ,0x35 ,0x1e ,0x2b ,0x39 ,0xe1 ,0xe0 ,0x3b ,0x1f ,0x14 ,0x1a ,0x04,
                        0x64 ,0x00 ,0x00 ,0x3d ,0x3c ,0x20 ,0x21 ,0x08 ,0x16 ,0x1d ,0xe2 ,0x3e ,0x23 ,0x22 ,0x15 ,0x07,
                        0x1b ,0x06 ,0x8b ,0x3f ,0x24 ,0x00 ,0x17 ,0x0a ,0x09 ,0x19 ,0x91 ,0x40 ,0x41 ,0x00 ,0x1c ,0x18,
                        0x0b ,0x05 ,0x2c ,0x42 ,0x26 ,0x25 ,0x0c ,0x0d ,0x0e ,0x10 ,0x11 ,0x43 ,0x2a ,0x27 ,0x2d ,0x12,
                        0x0f ,0x36 ,0x8a ,0x44 ,0x45 ,0x89 ,0x2e ,0x13 ,0x33 ,0x37 ,0x90 ,0x46 ,0x49 ,0x4c ,0x2f ,0x30,
                        0x34 ,0x38 ,0x88 ,0x47 ,0x4a ,0x4d ,0x31 ,0x32 ,0x00 ,0x87 ,0xe6 ,0x48 ,0x4b ,0x4e ,0x28 ,0x52,
                        0x50 ,0xe5 ,0xe7 ,0xd2 ,0x53 ,0x5f ,0x5c ,0x59 ,0x51 ,0x00 ,0xf1 ,0xd1 ,0x54 ,0x60 ,0x5d ,0x5a,
                        0x4f ,0x8e ,0x65 ,0xd0 ,0x55 ,0x61 ,0x5e ,0x5b ,0x62 ,0xa4 ,0xe4 ,0xfc ,0x56 ,0x57 ,0x85 ,0x58,
                        0x63 ,0x00 ,0x00 ,0xc2 ,0x24};
        static readonly byte[] ctrl_report_buffer_0x09 = new byte[] { 0x09 ,0x2b ,0x00 ,0x49 ,0x00 ,0x00 ,0x4a ,0x00 ,0x00 ,0x4b ,0x00 ,0x00 ,0x4c ,0x00 ,0x00 ,0x4d,
                        0x00 ,0x00 ,0x4e ,0x00 ,0x00 ,0xa4 ,0x00 ,0x00 ,0x8e ,0x00 ,0x00 ,0xd0 ,0x00 ,0x00 ,0xd1 ,0x00,
                        0x00 ,0x00 ,0x00 ,0x00 ,0x01 ,0x00 ,0x00 ,0x00 ,0x00 ,0xcd ,0x04};
        static readonly byte[] ctrl_report_buffer_0x0d = new byte[] { 0x0d ,0xbb ,0x01 ,0x00 ,0x06 ,0x0b ,0x05 ,0x45 ,0x83 ,0xca ,0xca ,0xca ,0xca ,0xca ,0xca ,0xce,
                            0xce ,0xd2 ,0xce ,0xce ,0xd2 ,0x19 ,0x19 ,0x19 ,0x19 ,0x19 ,0x19 ,0x23 ,0x23 ,0x2d ,0x23 ,0x23,
                            0x2d ,0xe0 ,0xe0 ,0xe0 ,0xe0 ,0xe0 ,0xe0 ,0xe3 ,0xe3 ,0xe6 ,0xe3 ,0xe3 ,0xe6 ,0xd2 ,0xd2 ,0xd5,
                            0xd2 ,0xd2 ,0xd5 ,0xd5 ,0xd5 ,0xd9 ,0xd5 ,0x00 ,0xd9 ,0x2d ,0x2d ,0x36 ,0x2d ,0x2d ,0x36 ,0x36,
                            0x36 ,0x40 ,0x36 ,0x00 ,0x40 ,0xe6 ,0xe6 ,0xe9 ,0xe6 ,0xe6 ,0xe9 ,0xe9 ,0xe9 ,0xec ,0xe9 ,0x00,
                            0xec ,0xd9 ,0xd9 ,0xdd ,0xd9 ,0xdd ,0xdd ,0xe0 ,0xe0 ,0xdd ,0xe0 ,0xe4 ,0xe4 ,0x40 ,0x40 ,0x4a,
                            0x40 ,0x4a ,0x4a ,0x53 ,0x53 ,0x4a ,0x53 ,0x5d ,0x5d ,0xec ,0xec ,0xef ,0xec ,0xef ,0xef ,0xf2,
                            0xf2 ,0xef ,0xf2 ,0xf5 ,0xf5 ,0xe4 ,0xe4 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00,
                            0x00 ,0x5d ,0x5d ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0xf5 ,0xf5 ,0x00,
                            0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0xe4 ,0xe4 ,0xe8 ,0xe8 ,0xe8 ,0xe8 ,0xe8,
                            0xeb ,0xeb ,0xeb ,0x00 ,0xeb ,0x5d ,0x5d ,0x67 ,0x67 ,0x67 ,0x67 ,0x67 ,0x70 ,0x70 ,0x70 ,0x00,
                            0x70 ,0xf5 ,0xf5 ,0xf8 ,0xf8 ,0xf8 ,0xf8 ,0xf8 ,0xfb ,0xfb ,0xfb ,0x00 ,0xfb ,0xeb ,0xef ,0xef,
                            0xef ,0x00 ,0xef ,0xf0 ,0xf0 ,0xed ,0xf0 ,0xf0 ,0x00 ,0x70 ,0x7a ,0x7a ,0x7a ,0x00 ,0x7a ,0x7a,
                            0x7a ,0x6f ,0x7a ,0x7a ,0x00 ,0xfb ,0xfd ,0xfd ,0xfd ,0x00 ,0xfd ,0xf8 ,0xf8 ,0xea ,0xf8 ,0xf8,
                            0x00 ,0xed ,0xed ,0xea ,0xed ,0xed ,0x00 ,0xed ,0xea ,0xea ,0xf6 ,0xe7 ,0xea ,0x6f ,0x6f ,0x65,
                            0x6f ,0x6f ,0x00 ,0x6f ,0x65 ,0x65 ,0x66 ,0x5a ,0x65 ,0xea ,0xea ,0xdc ,0xea ,0xea ,0x00 ,0xea,
                            0xdc ,0xdc ,0x00 ,0xce ,0xdc ,0xea ,0xe7 ,0xe5 ,0xe7 ,0xe5 ,0xe5 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00,
                            0x00 ,0x65 ,0x5a ,0x50 ,0x5a ,0x50 ,0x50 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0xdc ,0xce ,0xc0,
                            0xce ,0xc0 ,0xc0 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0xe7 ,0x00 ,0x00 ,0xe2 ,0xe2 ,0xe2 ,0xe2,
                            0xdf ,0xdf ,0xdf ,0xdf ,0xdf ,0x5a ,0x00 ,0x00 ,0x45 ,0x45 ,0x45 ,0x45 ,0x3b ,0x3b ,0x3b ,0x3b,
                            0x3b ,0xce ,0x00 ,0x00 ,0xb2 ,0xb2 ,0xb2 ,0xb2 ,0xa4 ,0xa4 ,0xa4 ,0xa4 ,0xa4 ,0xdc ,0xdc ,0xdc,
                            0xdc ,0x00 ,0xda ,0xda ,0xda ,0xda ,0xda ,0x00 ,0xd7 ,0x30 ,0x30 ,0x30 ,0x30 ,0x00 ,0x26 ,0x26,
                            0x26 ,0x26 ,0x26 ,0x00 ,0x1c ,0x96 ,0x96 ,0x96 ,0x96 ,0x00 ,0x88 ,0x88 ,0x88 ,0x88 ,0x88 ,0x00,
                            0x7a ,0xd7 ,0xd7 ,0xd7 ,0x00 ,0xd4 ,0xd4 ,0xd4 ,0xd4 ,0xd4 ,0xd1 ,0xd1 ,0xd1 ,0x1c ,0x1c ,0x1c,
                            0x00 ,0x11 ,0x11 ,0x11 ,0x11 ,0x11 ,0x06 ,0x06 ,0x06 ,0x7a ,0x7a ,0x7a ,0x00 ,0x6c ,0x6c ,0x6c,
                            0x6c ,0x6c ,0x5e ,0x5e ,0x5e ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00,
                            0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00,
                            0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x24 ,0xcf};
        static readonly byte[] ctrl_report_buffer_0x13 = new byte[] { 0x13, 0x08, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00 };
        #endregion

        private bool rv_set_ctrl_report(byte report_id)
        {
            bool success;
            switch (report_id)
            {
                case 0x15:
                    success = ctrl_device.WriteFeatureData(ctrl_report_buffer_0x15);
                    break;
                case 0x05:
                    success = ctrl_device.WriteFeatureData(ctrl_report_buffer_0x05);
                    break;
                case 0x07:
                    success = ctrl_device.WriteFeatureData(ctrl_report_buffer_0x07);
                    break;
                case 0x0a:
                    success = ctrl_device.WriteFeatureData(ctrl_report_buffer_0x0a);
                    break;
                case 0x0b:
                    success = ctrl_device.WriteFeatureData(ctrl_report_buffer_0x0b);
                    break;
                case 0x06:
                    success = ctrl_device.WriteFeatureData(ctrl_report_buffer_0x06);
                    break;
                case 0x09:
                    success = ctrl_device.WriteFeatureData(ctrl_report_buffer_0x09);
                    break;
                case 0x0d:
                    success = ctrl_device.WriteFeatureData(ctrl_report_buffer_0x0d);
                    break;
                case 0x13:
                    success = ctrl_device.WriteFeatureData(ctrl_report_buffer_0x13);
                    break;
                default:
                    success = false;
                    break;
            }

            return success;
        }

        public RoccatVulcan()
        {
            IsKeyboard = true;
        }

        public override bool Connect()
        {
            if (!Global.Configuration.VarRegistry.GetVariable<bool>($"UnifiedHID_{this.GetType().Name}_enable"))
            {
                return false;
            }

            IEnumerable<HidDevice> devices = HidDevices.Enumerate(0x1E7D, new int[] { 0x307A, 0x3098 });

            try
            {
                if (devices.Count() > 0)
                {
                    ctrl_device_leds = devices.First(dev => dev.Capabilities.UsagePage == 0x0001 && dev.Capabilities.Usage == 0x0000);
                    ctrl_device = devices.First(dev => dev.Capabilities.FeatureReportByteLength > 50);

                    ctrl_device.OpenDevice();
                    ctrl_device_leds.OpenDevice();

                    bool success =
                        rv_get_ctrl_report(0x0f) &&
                        rv_set_ctrl_report(0x15) &&
                        rv_wait_for_ctrl_device() &&
                        rv_set_ctrl_report(0x05) &&
                        rv_wait_for_ctrl_device() &&
                        rv_set_ctrl_report(0x07) &&
                        rv_wait_for_ctrl_device() &&
                        rv_set_ctrl_report(0x0a) &&
                        rv_wait_for_ctrl_device() &&
                        rv_set_ctrl_report(0x0b) &&
                        rv_wait_for_ctrl_device() &&
                        rv_set_ctrl_report(0x06) &&
                        rv_wait_for_ctrl_device() &&
                        rv_set_ctrl_report(0x09) &&
                        rv_wait_for_ctrl_device() &&
                        rv_set_ctrl_report(0x0d) &&
                        rv_wait_for_ctrl_device() &&
                        rv_set_ctrl_report(0x13) &&
                        rv_wait_for_ctrl_device();

                    if (!success)
                    {
                        ctrl_device.CloseDevice();
                        ctrl_device_leds.CloseDevice();
                    }

                    return (IsConnected = success);
                }
            }
            catch (Exception exc)
            {
                Global.logger.LogLine($"Error when attempting to open UnifiedHID device:\n{exc}", Logging_Level.Error);
            }

            return false;
        }

        // We need to override Disconnect() too cause we have two HID devices open for this keyboard.
        public override bool Disconnect()
        {
            try
            {
                ctrl_device.CloseDevice();
                ctrl_device_leds.CloseDevice();
                return true;
            }
            catch (Exception exc)
            {
                Global.logger.LogLine($"Error when attempting to close UnifiedHID device:\n{exc}", Logging_Level.Error);
            }
            return false;
        }

        public override bool SetMultipleLEDColour(Dictionary<DeviceKeys, Color> keyColors)
        {
            try
            {
                if (!this.IsConnected)
                    return false;

                // Send seven chunks with 64 bytes each
                byte[] hwmap = new byte[444];

                foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                {
                    Color clr = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(key.Value, key.Value.A / 255.0D));

                    if (!KeyMap.TryGetValue(key.Key, out int index))
                    {
                        continue;
                    }

                    if (index > RV_NUM_KEYS)
                    {
                        continue;
                    }

                    int offset = ((index / 12) * 36) + (index % 12);
                    hwmap[offset + 0] = clr.R;
                    hwmap[offset + 12] = clr.G;
                    hwmap[offset + 24] = clr.B;
                }


                // Plus one byte report ID for the lib
                byte[] workbuf = new byte[65];

                // First chunk comes with header
                workbuf[0] = 0x00;
                workbuf[1] = 0xa1;
                workbuf[2] = 0x01;
                workbuf[3] = 0x01;
                workbuf[4] = 0xb4;

                Array.Copy(hwmap, 0, workbuf, 5, 60);

                NativeOverlapped overlapped = new NativeOverlapped();
                unsafe
                {
                    fixed (byte* workbufPointer = workbuf)
                    {
                        if (WriteFile(ctrl_device_leds.Handle, (IntPtr)workbufPointer, (uint)workbuf.Length, out _, ref overlapped) != true)
                        {
                            return false;
                        }
                    }
                }

                // Six more chunks
                for (int i = 1; i < 7; i++)
                {
                    workbuf[0] = 0x00;
                    Array.Copy(hwmap, (i * 64) - 4, workbuf, 1, 64);

                    unsafe
                    {
                        fixed (byte* workbufPointer = workbuf)
                        {
                            if (WriteFile(ctrl_device_leds.Handle, (IntPtr)workbufPointer, (uint)workbuf.Length, out _, ref overlapped) != true)
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}