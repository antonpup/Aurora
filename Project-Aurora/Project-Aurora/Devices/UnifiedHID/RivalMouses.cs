using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Devices.UnifiedHID
{

    internal class Rival100 : UnifiedBase
    {
        public Rival100()
        {
            PrettyName = "Rival 100";
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

    internal class Rival110 : UnifiedBase
    {
        public Rival110()
        {
            PrettyName = "Rival 110";
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

    internal class Rival300 : UnifiedBase
    {
        public Rival300()
        {
            PrettyName = "Rival 300";
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

    internal class Rival500 : UnifiedBase
    {
        public Rival500()
        {
            PrettyName = "Rival 500";
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

}
