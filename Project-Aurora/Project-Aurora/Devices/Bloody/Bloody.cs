using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Settings;
using Aurora.Utils;
using Bloody.NET;

namespace Aurora.Devices.Bloody
{
    public class BloodyDevice : DefaultDevice
    {
        public override string DeviceName => "Bloody";

        private BloodyKeyboard keyboard;

        public override bool Initialize()
        {
            keyboard = BloodyKeyboard.Initialize();

            return IsInitialized = (keyboard != null);
        }

        public override void Shutdown()
        {
            keyboard.Disconnect();
            IsInitialized = false;
        }

        public override bool UpdateDevice(Dictionary<int, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!IsInitialized)
                return false;

            foreach (var (key, clr) in keyColors)
            {
                if (BloodyKeyMap.KeyMap.TryGetValue((DeviceKeys)key, out var bloodyKey))
                    keyboard.SetKeyColor(bloodyKey, ColorUtils.CorrectWithAlpha(clr));
            }

            return keyboard.Update();
        }
    }
}