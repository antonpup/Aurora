using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Devices.UnifiedHID
{
    internal class AsusPugio : UnifiedBase
    {
        public AsusPugio()
        {
            PrettyName = "Asus Pugio";
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

}
