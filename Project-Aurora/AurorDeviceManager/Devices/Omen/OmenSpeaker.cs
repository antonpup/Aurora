using System.Drawing;
using System.Runtime.InteropServices;
using Common.Devices;

namespace AurorDeviceManager.Devices.Omen
{
    class OmenSpeaker : IOmenDevice
    {
        private const string OmenLightingSdkDll = "x64\\OmenLightingSDK.dll";
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

        public void Shutdown()
        {
            try
            {
                Monitor.Enter(this);
                OmenLighting_Speaker_Close(hSpeaker);
                hSpeaker = IntPtr.Zero;
            }
            catch (Exception exc)
            {
                Global.Logger.Error("OMEN Speaker, Exception during Shutdown. Message: " + exc);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void SetLights(Dictionary<DeviceKeys, Color> keyColors)
        {
            if (hSpeaker != IntPtr.Zero)
            {
                if (keyColors.ContainsKey(DeviceKeys.Peripheral_Logo))
                {
                    SetLight(DeviceKeys.Peripheral_Logo, keyColors[DeviceKeys.Peripheral_Logo]);
                    return;
                }
            }
        }

        private void SetLight(DeviceKeys keys, Color color)
        {
            if (hSpeaker != IntPtr.Zero)
            {
                Task.Run(() =>
                {
                    if (Monitor.TryEnter(this))
                    {
                        try
                        {
                            int res = OmenLighting_Speaker_SetStatic(hSpeaker, LightingColor.FromColor(color), IntPtr.Zero);
                            if (res != 0)
                            {
                                Global.Logger.Error("OMEN Speaker, Set static effect fail: " + res);
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
            return (hSpeaker != IntPtr.Zero ? "Speaker Connected" : string.Empty);
        }

        [DllImport(OmenLightingSdkDll)]
        static extern IntPtr OmenLighting_Speaker_Open();

        [DllImport(OmenLightingSdkDll)]
        static extern void OmenLighting_Speaker_Close(IntPtr hSpeaker);

        [DllImport(OmenLightingSdkDll)]
        static extern int OmenLighting_Speaker_SetStatic(IntPtr hSpeaker, LightingColor color, IntPtr property);
    }
}
