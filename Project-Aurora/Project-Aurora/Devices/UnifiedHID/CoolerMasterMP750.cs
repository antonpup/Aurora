using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Devices.UnifiedHID
{
    internal class CoolerMasterMP750 : UnifiedBase
    {
        public override string PrettyName => "Cooler Master MP750";

        private int packageSize = 65; // Number of bytes to write

        public CoolerMasterMP750()
        {
            DeviceFuncMap = new Dictionary<DeviceKeys, Func<byte, byte, byte, bool>>
            {
                { DeviceKeys.Peripheral_Logo, SetColor }
            };
        }

        public override bool Connect()
        {
            if (!Global.Configuration.VarRegistry.GetVariable<bool>($"UnifiedHID_{this.GetType().Name}_enable"))
            {
                return false;
            }

            return Connect(0x2516, new[] { 0x0109 }, unchecked((short)0xff00));
        }


        public bool SetColor(byte r, byte g, byte b)
        {
            byte[] data = new byte[packageSize];

            for (var i = 0; i < packageSize; i++)
            {
                data[i] = 0x00;
            }

            data[1] = 0x01;
            data[2] = 0x04;
            data[3] = r;
            data[4] = g;
            data[5] = b;

            return device.Write(data);
        }
    }
}
