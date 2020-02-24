using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Devices.Omen
{
    public class OmenMouse
    {
        private IntPtr hMouse = IntPtr.Zero;

        private OmenMouse(IntPtr hMouse)
        {
            this.hMouse = hMouse;
        }

        public static OmenMouse GetOmenMouse()
        {
            IntPtr ptr = OmenLighting_Mouse_Open();
            if (ptr != IntPtr.Zero)
            {
                return new OmenMouse(ptr);
            }
            return null;
        }

        private static MouseLightingZone GetMouseLightingZone(DeviceKeys key)
        {
            MouseLightingZone zone = MouseLightingZone.MOUSE_LIGHTING_ZONE_ALL;
            switch (key)
            {
                case (DeviceKeys.Peripheral_Logo):
                    return MouseLightingZone.MOUSE_LIGHTING_ZONE_LOGO;
                case (DeviceKeys.Peripheral_FrontLight):
                    return MouseLightingZone.MOUSE_LIGHTING_ZONE_LOGO;
                case (DeviceKeys.Peripheral_ScrollWheel):
                    return MouseLightingZone.MOUSE_LIGHTING_ZONE_WHEEL;
                default:
                    return zone;
            }
        }

        public enum MouseLightingZone
        {
            MOUSE_LIGHTING_ZONE_ALL = 0,                     /* All zone. Only for set lighting effect. */
            MOUSE_LIGHTING_ZONE_LOGO = 1,                           /* Logo zone */
            MOUSE_LIGHTING_ZONE_WHEEL = 2,                          /* Wheel zone */
        }

        public void SetLights(DeviceKeys key, Color color)
        {
            try
            {
                if (hMouse != IntPtr.Zero)
                {
                    int res = OmenLighting_Mouse_SetStatic(hMouse, (int)GetMouseLightingZone(key), LightingColor.FromColor(color), IntPtr.Zero);
                    if (res != 0)
                    {
                        Global.logger.Error("OMEN Mouse, Set static effect fail: " + res);
                    }
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error("OMEN Mouse, exception during set lights: " + exc);
            }
        }

        internal void Shutdown()
        {
            try
            {
                OmenLighting_Mouse_Close(hMouse);
                hMouse = IntPtr.Zero;
            }
            catch (Exception exc)
            {
                Global.logger.Error("OMEN Mouse, exception during shutdown: " + exc);
            }
        }

        [DllImport("OmenLightingSDK.dll")]
        static extern IntPtr OmenLighting_Mouse_Open();

        [DllImport("OmenLightingSDK.dll")]
        static extern IntPtr OmenLighting_Mouse_OpenByName([MarshalAsAttribute(UnmanagedType.LPWStr)] string deviceName);

        [DllImport("OmenLightingSDK.dll")]
        static extern void OmenLighting_Mouse_Close(IntPtr hMouse);

        [DllImport("OmenLightingSDK.dll")]
        static extern int OmenLighting_Mouse_SetStatic(IntPtr hMouse, int zone, LightingColor color, IntPtr property);
    }
}
