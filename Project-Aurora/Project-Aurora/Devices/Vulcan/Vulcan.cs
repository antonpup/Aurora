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
using Vulcan.NET;

namespace Aurora.Devices.Vulcan
{
    public class VulcanDevice : DefaultDevice
    {
        public override string DeviceName => "Vulcan";
        public override bool IsInitialized => VulcanKeyboard.IsConnected;

        public override bool Initialize() => VulcanKeyboard.Initialize();

        public override void Shutdown() => VulcanKeyboard.Disconnect();

        public override bool UpdateDevice(Dictionary<int, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!IsInitialized)
                return false;

            foreach (var (key, clr) in keyColors)
            {
                if (VulcanKeyMap.KeyMap.TryGetValue((DeviceKeys)key, out var vulcanKey))
                    VulcanKeyboard.SetKeyColor(vulcanKey, ColorUtils.CorrectWithAlpha(clr));
            }

            return VulcanKeyboard.Update();
        }
    }
}
