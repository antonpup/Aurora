using HidLibrary;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Devices.UnifiedHID
{
    internal class Vyper17SeriesKeyboard : UnifiedBase
    {
        private readonly byte Control = 0x02; // LightOn = 0x02, LightOff = 0x01
        private readonly byte Effect = 0x01; // Color = 0x01, Breathing = 0x02, Wave = 0x03, Flash = 0x12, Mix = 0x13
        private readonly byte Speed = 0x00; // Maximum = 0x00, Medium = 0x05, Minimum = 0x0a
        private readonly byte Light = 0x32; // Maximum = 0x32, Minimum = 0x00
        private readonly byte Direction = 0x03; // LeftRight = 0x01, RightLeft = 0x02, None = 0x03

        private readonly byte ColorIndex = 0x08; // Number of total zone
        private readonly byte Save = 0x00;

        public override string PrettyName => "Vyper 17 Series Keyboard";

        public Vyper17SeriesKeyboard()
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

            return Connect(0x048d, new[] { 0xce00 }, unchecked((short)0xff12));
        }

        public bool SetColor(byte r, byte g, byte b)
        {
            bool ret = true;

            byte[] data = new byte[] { 0x00, 0x14, 0x00, 0x00, r, g, b, 0x00 };

            for (int index = 0; index < ColorIndex; index++)
            {
                data[3] = (byte)(index + 1);
                // Write LED color
                ret = ret && device.WriteFeatureData(data);
            }

            // Write LED effect
            data = new byte[] { 0x00, 0x08, Control, Effect, Speed, Light, ColorIndex, Direction, Save };

            return ret && device.WriteFeatureData(data);
        }
    }
}
