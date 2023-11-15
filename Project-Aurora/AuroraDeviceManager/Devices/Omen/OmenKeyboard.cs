using System.Drawing;
using System.Runtime.InteropServices;
using Common.Devices;

namespace AuroraDeviceManager.Devices.Omen
{
    public class OmenKeyboard : IOmenDevice
    {
        private const string OmenLightingSdkDll = "x64\\OmenLightingSDK.dll";
        private IntPtr hKB = IntPtr.Zero;

        private OmenKeyboard(IntPtr hKB)
        {
            this.hKB = hKB;
        }

        internal static IEnumerable<IOmenDevice> GetOmenKeyboards()
        {
            var kboardPointer = OmenLighting_Keyboard_OpenByName("Woodstock");
            if (kboardPointer != IntPtr.Zero)
                yield return new OmenKeyboard(kboardPointer);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct StaticKeyEffect
        {
            [MarshalAs(UnmanagedType.Struct)]
            public LightingColor lightingColor;
            public int key;

            public StaticKeyEffect(KeyValuePair<DeviceKeys, Color> key)
            {
                lightingColor = LightingColor.FromColor(key.Value);
                this.key = OmenKeys.GetKey(key.Key);
            }
        }

        public string GetDeviceName()
        {
            return (hKB != IntPtr.Zero ? "Keyboard Connected" : string.Empty);
        }

        public void SetLights(Dictionary<DeviceKeys, Color> keyColors)
        {
            if (hKB != IntPtr.Zero && keyColors.Count > 0)
            {
                if (Global.DeviceConfig.DevicesDisableKeyboard)
                {
                    return;
                }

                Task.Run(() =>
                {
                    if (Monitor.TryEnter(this))
                    {
                        try
                        {
                            List<StaticKeyEffect> list = new List<StaticKeyEffect>();
                            foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                            {
                                list.Add(new StaticKeyEffect(key));
                            }

                            int res = OmenLighting_Keyboard_SetStatic(hKB, list.ToArray(), list.Count, IntPtr.Zero);
                            if (res != 0)
                            {
                                Global.Logger.Error("OMEN Keyboard, Set static effect fail: " + res);
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

        public void Shutdown()
        {
            try
            {
                Monitor.Enter(this);
                OmenLighting_Keyboard_Close(hKB);
                hKB = IntPtr.Zero;
            }
            catch (Exception exc)
            {
                Global.Logger.Error("OMEN Keyboard, Exception during Shutdown. Message: " + exc);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        [DllImport(OmenLightingSdkDll)]
        static extern void OmenLighting_Keyboard_Close(IntPtr hKeyboard);

        [DllImport(OmenLightingSdkDll)]
        static extern int OmenLighting_Keyboard_SetStatic(IntPtr hKeyboard, StaticKeyEffect[] staticEffect, int count, IntPtr keyboardLightingEffectProperty);

        [DllImport(OmenLightingSdkDll)]
        static extern IntPtr OmenLighting_Keyboard_Open();

        [DllImport(OmenLightingSdkDll)]
        static extern IntPtr OmenLighting_Keyboard_OpenByName([MarshalAs(UnmanagedType.LPWStr)] string deviceName);
    }

}
