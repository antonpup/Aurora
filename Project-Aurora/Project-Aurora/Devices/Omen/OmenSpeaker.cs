using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Aurora.Settings;

namespace Aurora.Devices.Omen
{
    class OmenSpeaker
    {
        private IntPtr hSpeaker = IntPtr.Zero;

        private OmenSpeaker(IntPtr hSpeaker)
        {
            this.hSpeaker = hSpeaker;
        }

        public static OmenSpeaker GetOmenSpeaker()
        {
            IntPtr ptr = OmenLighting_Speaker_Open();

            return (ptr == IntPtr.Zero ? null : new OmenSpeaker(ptr));
        }

        internal void Shutdown()
        {
            try
            {
                Monitor.Enter(this);
                OmenLighting_Speaker_Close(hSpeaker);
                hSpeaker = IntPtr.Zero;
            }
            catch (Exception exc)
            {
                Global.logger.Error("OMEN Speaker, Exception during Shutdown. Message: " + exc);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void SetLights(DeviceKeys keys, Color color)
        {
            if (hSpeaker != IntPtr.Zero)
            {
                Task.Run(() => {
                    if (Monitor.TryEnter(this))
                    {
                        try
                        {
                            int res = OmenLighting_Speaker_SetStatic(hSpeaker, LightingColor.FromColor(color), IntPtr.Zero);
                            if (res != 0)
                            {
                                Global.logger.Error("OMEN Speaker, Set static effect fail: " + res);
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

        [DllImport("OmenLightingSDK.dll")]
        static extern IntPtr OmenLighting_Speaker_Open();

        [DllImport("OmenLightingSDK.dll")]
        static extern void OmenLighting_Speaker_Close(IntPtr hSpeaker);

        [DllImport("OmenLightingSDK.dll")]
        static extern int OmenLighting_Speaker_SetStatic(IntPtr hSpeaker, LightingColor color, IntPtr property);
    }
}
