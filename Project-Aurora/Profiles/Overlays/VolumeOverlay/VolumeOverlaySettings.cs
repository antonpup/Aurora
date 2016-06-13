using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Overlays
{
    public class VolumeOverlaySettings
    {
        //Generic
        public bool enabled;
        public KeySequence sequence;

        public Color low_color;
        public Color med_color;
        public Color high_color;
        public int delay;

        public VolumeOverlaySettings()
        {
            enabled = true;
            sequence = new KeySequence(new Devices.DeviceKeys[] {
                Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4,
                Devices.DeviceKeys.F5, Devices.DeviceKeys.F6, Devices.DeviceKeys.F7, Devices.DeviceKeys.F8,
                Devices.DeviceKeys.F9, Devices.DeviceKeys.F10, Devices.DeviceKeys.F11, Devices.DeviceKeys.F12
            });

            low_color = Color.FromArgb(255, 0, 255, 0);
            med_color = Color.OrangeRed;
            high_color = Color.Red;

            delay = 3;
        }
    }
}
