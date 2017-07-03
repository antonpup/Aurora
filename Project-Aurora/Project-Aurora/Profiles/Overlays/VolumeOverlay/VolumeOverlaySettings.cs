using Aurora.Settings;
using System.Drawing;

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
        public bool dim_background;
        public Color dim_color;

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

            dim_background = false;
            dim_color = Color.FromArgb(169, 0, 0, 0);
        }
    }
}
