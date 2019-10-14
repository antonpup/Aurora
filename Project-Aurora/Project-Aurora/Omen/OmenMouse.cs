using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Aurora.Devices.Omen.OmenDevice;

namespace Aurora.Devices.Omen
{
    public class OmenMouse
    {
        private readonly IntPtr hMouse;

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
            MOUSE_LIGHTING_ZONE_LOGO = 1,                           /* Wheel zone */
            MOUSE_LIGHTING_ZONE_WHEEL = 2,                          /* Wireless connection */
        }



        public void SetLights(DeviceKeys key, Color color)
        {
            OmenLighting_Mouse_SetStaticEffect(hMouse, GetMouseLightingZone(key), LightingColor.fromColor(color), IntPtr.Zero);
        }


        internal void Shutdown()
        {
            try
            {
                OmenLighting_Mouse_Close(hMouse);
            }
            catch (Exception exc)
            {
                Global.logger.Error("OMEN Mouse, Exception during Shutdown. Message: " + exc);
            }
        }

        [DllImport("OmenLightingSDK.dll")]
        static extern IntPtr OmenLighting_Mouse_Open();

        [DllImport("OmenLightingSDK.dll")]
        static extern void OmenLighting_Mouse_Close(IntPtr hMouse);

        [DllImport("OmenLightingSDK.dll")]
        static extern bool OmenLighting_Mouse_SetStaticEffect(IntPtr hMouse, MouseLightingZone zone, LightingColor color, IntPtr property);
    }
}
