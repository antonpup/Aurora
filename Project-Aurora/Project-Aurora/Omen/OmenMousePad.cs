using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using static Aurora.Devices.Omen.OmenDevice;

namespace Aurora.Devices.Omen
{
    public class OmenMousePad
    {
        private readonly IntPtr hMousePad;

        private OmenMousePad(IntPtr hMousePad)
        {
            this.hMousePad = hMousePad;
        }

        public static OmenMousePad GetOmenMousePad()
        {
            // IntPtr kboardPointer = default(IntPtr);
            IntPtr ptr = OmenLighting_MousePad_Open();
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
            }
            catch (Exception exc)
            {
                Global.logger.Error("OMEN Mouse Pad, Exception during Shutdown. Message: " + exc);
            }
        }

        public void SetLights(DeviceKeys key, Color color)
        {
            int zone;
            zone = (key == DeviceKeys.MOUSEPADLIGHT15 ? (int)MousePadZone.MOUSE_PAD_ZONE_LOGO : (int)MousePadZone.MOUSE_PAD_ZONE_0 + ((int)key - (int)DeviceKeys.MOUSEPADLIGHT1));

            OmenLighting_MousePad_SetStaticEffect(hMousePad, zone, LightingColor.fromColor(color), IntPtr.Zero);
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
        static extern void OmenLighting_MousePad_Close(IntPtr hMouse);

        [DllImport("OmenLightingSDK.dll")]
        static extern bool OmenLighting_MousePad_SetStaticEffect(IntPtr hMousePad, int zone, LightingColor color, IntPtr property);
    }
}
