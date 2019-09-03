using Aurora.Devices.Layout.Layouts;
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
            sequence = new KeySequence(new KeyboardKeys[] {
                KeyboardKeys.F1, KeyboardKeys.F2, KeyboardKeys.F3, KeyboardKeys.F4,
                KeyboardKeys.F5, KeyboardKeys.F6, KeyboardKeys.F7, KeyboardKeys.F8,
                KeyboardKeys.F9, KeyboardKeys.F10, KeyboardKeys.F11, KeyboardKeys.F12
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
