using HidLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Devices.UnifiedHID
{
    internal class RoccatTyon : UnifiedBase
    {
        private HidDevice deviceLeds;

        public override bool IsConnected => (device?.IsOpen ?? false) || (deviceLeds?.IsOpen ?? false);
        public override string PrettyName => "Roccat Tyon";

        public RoccatTyon()
        {
            DeviceFuncMap = new Dictionary<DeviceKeys, Func<byte, byte, byte, bool>>
            {
                { DeviceKeys.Peripheral_ScrollWheel, SetColour },
                { DeviceKeys.Peripheral_FrontLight, SetColour }
            };
        }

        private bool InitMouseColor()
        {
            return device.WriteFeatureData(initPacket);
        }

        private bool WaitCtrlDevice()
        {
            // 3 Tries because the first one always fails.
            for (int i = 1; i < 3; i++)
            {
                if (device.ReadFeatureData(out byte[] buffer, 0x04) && buffer.Length > 2)
                {
                    if (buffer[1] == 0x01)
                        return true;
                }
                else
                    return false;
            }

            return false;
        }

        public override bool Connect()
        {
            if (!Global.Configuration.VarRegistry.GetVariable<bool>($"UnifiedHID_{this.GetType().Name}_enable"))
            {
                return false;
            }

            IEnumerable<HidDevice> devices = HidDevices.Enumerate(0x1E7D, new int[] { 0x2E4A });

            try
            {
                if (devices.Count() > 0)
                {
                    device = devices.First(dev => dev.Capabilities.FeatureReportByteLength > 50);
                    deviceLeds = devices.First(dev => dev.Capabilities.UsagePage == 0x0001 && dev.Capabilities.Usage == 0x0002);

                    device.OpenDevice();
                    deviceLeds.OpenDevice();

                    bool success = InitMouseColor() && WaitCtrlDevice();

                    if (!success)
                    {
                        Global.logger.LogLine($"Roccat Tyon Could not connect\n", Logging_Level.Error);
                        device.CloseDevice();
                        deviceLeds.CloseDevice();
                    }
                    else
                    {
                        Global.logger.LogLine($"Roccat Tyon connected\n", Logging_Level.Info);
                    }
                }
            }
            catch (Exception exc)
            {
                Global.logger.LogLine($"Error when attempting to open UnifiedHID device:\n{exc}", Logging_Level.Error);
            }

            return false;
        }

        // We need to override Disconnect() because we have two HID devices open for this mouse.
        public override bool Disconnect()
        {
            base.Disconnect();

            try
            {
                deviceLeds.CloseDevice();
            }
            catch (Exception exc)
            {
                Global.logger.LogLine($"Error when attempting to close UnifiedHID device:\n{exc}", Logging_Level.Error);
            }

            return !IsConnected;
        }

        // TODO: Set diffent colour for wheel and bottom led ?
        public bool SetColour(byte r, byte g, byte b)
        {
            try
            {
                if (!IsConnected)
                    return false;

                byte[] hwmap =
                {
                    r, g, b,
                    0x00, 0x00,
                    r, g, b,
                    0x00, 0x80, 0x80
                };

                byte[] workbuf = new byte[30];

                Array.Copy(controlPacket, 0, workbuf, 0, controlPacket.Length);
                Array.Copy(hwmap, 0, workbuf, controlPacket.Length, hwmap.Length);

                return device.WriteFeatureData(workbuf);
            }
            catch (Exception exc)
            {
                Global.logger.LogLine($"Error when attempting to close UnifiedHID device:\n{exc}", Logging_Level.Error);
                return false;
            }
        }

        // Packet with values set to white for mouse initialization.
        static readonly byte[] initPacket = new byte[] {
            0x06,0x1e,0x00,0x00,
            0x06,0x06,0x06,0x10,0x20,0x40,0x80,0xa4,0x02,0x03,0x33,0x00,0x01,0x01,0x03,
            0xff,0xff,0xff,0x00,0x00,0xff,0xff,0xff,0x00,0x01,0x08
        };

        // Packet with fixed values for affixing to mouse colors.
        static readonly byte[] controlPacket = new byte[] {
            0x06,0x1e,0x00,0x00,
            0x06,0x06,0x06,0x10,0x20,0x40,0x80,0xa4,0x02,0x03,0x33,0x00,0x01,0x01,0x03
        };
    }
}
