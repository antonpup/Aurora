using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Settings;

namespace Aurora.Devices.Omen
{
    public class OmenKeyboard : IOmenDevice
    {
        private IntPtr hKB = IntPtr.Zero;

        private OmenKeyboard(IntPtr hKB)
        {
            this.hKB = hKB;
        }

        internal static IOmenDevice GetOmenKeyboard()
        {
            switch (Global.Configuration.keyboard_brand)
            {
                case PreferredKeyboard.OMEN_Sequencer:
                    {
                        IntPtr kboardPointer = IntPtr.Zero;
                        kboardPointer = OmenLighting_Keyboard_OpenByName("Woodstock");
                        return (kboardPointer == IntPtr.Zero ? null : new OmenKeyboard(kboardPointer));
                    }
                case PreferredKeyboard.OMEN_Four_Zone:
                    return new OmenFourZoneLighting();
            }

            return null;
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
                if (Global.Configuration.devices_disable_keyboard)
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
                                Global.logger.Error("OMEN Keyboard, Set static effect fail: " + res);
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
                Global.logger.Error("OMEN Keyboard, Exception during Shutdown. Message: " + exc);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        [DllImport("OmenLightingSDK.dll")]
        static extern void OmenLighting_Keyboard_Close(IntPtr hKeyboard);

        [DllImport("OmenLightingSDK.dll")]
        static extern int OmenLighting_Keyboard_SetStatic(IntPtr hKeyboard, StaticKeyEffect[] staticEffect, int count, IntPtr keyboardLightingEffectProperty);

        [DllImport("OmenLightingSDK.dll")]
        static extern IntPtr OmenLighting_Keyboard_Open();

        [DllImport("OmenLightingSDK.dll")]
        static extern IntPtr OmenLighting_Keyboard_OpenByName([MarshalAsAttribute(UnmanagedType.LPWStr)] string deviceName);
    }

}
