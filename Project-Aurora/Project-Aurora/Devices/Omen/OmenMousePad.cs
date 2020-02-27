using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using Aurora.Settings;

namespace Aurora.Devices.Omen
{
    public class OmenMousePad
    {
        private IntPtr hMousePad = IntPtr.Zero;

        private OmenMousePad(IntPtr hMousePad)
        {
            this.hMousePad = hMousePad;
        }

        public static OmenMousePad GetOmenMousePad()
        {
            IntPtr ptr = IntPtr.Zero;
            switch (Global.Configuration.mouse_preference)
            {
                case PreferredMouse.OMEN_Outpost_Plus_Photon:
                    ptr = OmenLighting_MousePad_OpenByName("Outpost");
                    break;
            }

            if (ptr != IntPtr.Zero)
            {
                return new OmenMousePad(ptr);
            }

            return null;
        }

        internal void Shutdown()
        {
            try
            {
                OmenLighting_MousePad_Close(hMousePad);
                hMousePad = IntPtr.Zero;
            }
            catch (Exception exc)
            {
                Global.logger.Error("OMEN MousePad, Exception during Shutdown. Message: " + exc);
            }
        }

        public void SetLights(DeviceKeys key, Color color)
        {
            int zone;
            zone = (key == DeviceKeys.MOUSEPADLIGHT15 ? (int)MousePadZone.MOUSE_PAD_ZONE_LOGO : (int)MousePadZone.MOUSE_PAD_ZONE_0 + ((int)key - (int)DeviceKeys.MOUSEPADLIGHT1));

            if (hMousePad != IntPtr.Zero)
            {
                int res = OmenLighting_MousePad_SetStatic(hMousePad, zone, LightingColor.FromColor(color), IntPtr.Zero);
                if (res != 0)
                {
                    Global.logger.Error("OMEN MousePad, Set static effect fail: " + res);
                }
            }
        }

        public enum MousePadZone
        {
            MOUSE_PAD_ZONE_ALL = 0,                                 /* All zone */
            MOUSE_PAD_ZONE_LOGO,                                    /* Logo zone */
            MOUSE_PAD_ZONE_PAD,                                     /* Logo zone */
            MOUSE_PAD_ZONE_LEFT,                                    /* Left edge zone */
            MOUSE_PAD_ZONE_BOTTOM,                                  /* Left bottom zone */
            MOUSE_PAD_ZONE_RIGHT,                                   /* Left right zone */
            MOUSE_PAD_ZONE_0,                                       /* Zone 0 */
        }

        [DllImport("OmenLightingSDK.dll")]
        static extern IntPtr OmenLighting_MousePad_Open();

        [DllImport("OmenLightingSDK.dll")]
        static extern IntPtr OmenLighting_MousePad_OpenByName([MarshalAsAttribute(UnmanagedType.LPWStr)] string deviceName);

        [DllImport("OmenLightingSDK.dll")]
        static extern void OmenLighting_MousePad_Close(IntPtr hMouse);

        [DllImport("OmenLightingSDK.dll")]
        static extern int OmenLighting_MousePad_SetStatic(IntPtr hMousePad, int zone, LightingColor color, IntPtr property);
    }
}
