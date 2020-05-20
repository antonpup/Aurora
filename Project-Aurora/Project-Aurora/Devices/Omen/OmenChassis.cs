using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Devices.Omen
{
    public class OmenChassis : IOmenDevice
    {
        private IntPtr hChassis = IntPtr.Zero;

        private OmenChassis(IntPtr hChassis)
        {
            this.hChassis = hChassis;
        }

        public static OmenChassis GetOmenChassis()
        {
            IntPtr ptr = OmenLighting_Chassis_Open();

            return (ptr == IntPtr.Zero ? null : new OmenChassis(ptr));
        }

        public void Shutdown()
        {
            try
            {
                Monitor.Enter(this);
                OmenLighting_Chassis_Close(hChassis);
                hChassis = IntPtr.Zero;
            }
            catch (Exception exc)
            {
                Global.logger.Error("OMEN Chassis, Exception during Shutdown. Message: " + exc);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        private void SetLight(DeviceKeys key, Color color)
        {
            if (hChassis != IntPtr.Zero)
            {
                Task.Run(() => {
                    if (Monitor.TryEnter(this))
                    {
                        try
                        {
                            int res = OmenLighting_Chassis_SetStatic(hChassis, (int)GetZone(key), LightingColor.FromColor(color), IntPtr.Zero);
                            if (res != 0)
                            {
                                Global.logger.Error("OMEN Chassis, Set static effect fail: " + res);
                            }
                        }
                        finally
                        {
                            Monitor.Exit(this);
                        }
                    }
                });
            }
        }

        public string GetDeviceName()
        {
            return (hChassis != IntPtr.Zero ? "Chassis Connected" : string.Empty);
        }

        public void SetLights(Dictionary<DeviceKeys, Color> keyColors)
        {
            if (hChassis != IntPtr.Zero)
            {
                if (keyColors.ContainsKey(DeviceKeys.Peripheral_Logo))
                {
                    SetLight(DeviceKeys.Peripheral_Logo, keyColors[DeviceKeys.Peripheral_Logo]);
                    return;
                }
            }
        }

        /* Zone ground of chassis */
        public enum ChassisZone
        {
            CHASSIS_ZONE_ALL = 0,                                   /* All zone */
            CHASSIS_ZONE_0,                                         /* Zone 0 */
            CHASSIS_ZONE_1,                                         /* Zone 1 */
            CHASSIS_ZONE_2,                                         /* Zone 2 */
            CHASSIS_ZONE_3,                                         /* Zone 3 */
            CHASSIS_ZONE_4,                                         /* Zone 4 */
            CHASSIS_ZONE_5,                                         /* Zone 5 */
            CHASSIS_ZONE_6,                                         /* Zone 6 */
            CHASSIS_ZONE_7,                                         /* Zone 7 */
            CHASSIS_ZONE_8,                                         /* Zone 8 */
            CHASSIS_ZONE_9,                                         /* Zone 9 */
        };

        private static ChassisZone GetZone(DeviceKeys key)
        {
            return ChassisZone.CHASSIS_ZONE_ALL;
        }

        [DllImport("OmenLightingSDK.dll")]
        static extern IntPtr OmenLighting_Chassis_Open();

        [DllImport("OmenLightingSDK.dll")]
        static extern void OmenLighting_Chassis_Close(IntPtr hChassis);

        [DllImport("OmenLightingSDK.dll")]
        static extern int OmenLighting_Chassis_SetStatic(IntPtr hChassis, int zone, LightingColor color, IntPtr property);
    }
}
