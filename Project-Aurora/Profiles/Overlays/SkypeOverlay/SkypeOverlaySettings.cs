using Aurora.Settings;
using System.Drawing;

namespace Aurora.Profiles.Overlays.SkypeOverlay
{
    public class SkypeOverlaySettings
    {
        //Generic
        public bool enabled;
        public bool start_integration_client_on_launch;

        //Missed Messages Indicator
        public bool mm_enabled;
        public KeySequence mm_sequence;
        public Color mm_color_primary;
        public Color mm_color_secondary;
        public bool mm_blink;

        //Incoming Call Indicator
        public bool call_enabled;
        public KeySequence call_sequence;
        public Color call_color_primary;
        public Color call_color_secondary;
        public bool call_blink;


        public SkypeOverlaySettings()
        {
            //Generic
            enabled = true;
            start_integration_client_on_launch = true;

            //Missed Messages Indicator
            mm_enabled = true;
            mm_sequence = new KeySequence(new Devices.DeviceKeys[] {
                Devices.DeviceKeys.PRINT_SCREEN, Devices.DeviceKeys.SCROLL_LOCK, Devices.DeviceKeys.PAUSE_BREAK
            });
            mm_color_primary = Color.Orange;
            mm_color_secondary = Color.Black;
            mm_blink = true;

            //Incoming Call Indicator
            call_enabled = true;
            call_sequence = new KeySequence(new Devices.DeviceKeys[] {
                Devices.DeviceKeys.INSERT, Devices.DeviceKeys.HOME, Devices.DeviceKeys.PAGE_UP,
                Devices.DeviceKeys.DELETE, Devices.DeviceKeys.END, Devices.DeviceKeys.PAGE_DOWN
            });
            call_color_primary = Color.Green;
            call_color_secondary = Color.Red;
            call_blink = true;
        }
    }
}
