using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Devices.Omen
{
    public class OmenKeyboard
    {
        private IntPtr hKB = IntPtr.Zero;

        private OmenKeyboard(IntPtr hKB)
        {
            this.hKB = hKB;
        }

        public static OmenKeyboard GetOmenKeyboard()
        {
            IntPtr kboardPointer = OmenLighting_Keyboard_Open();
            if (kboardPointer != IntPtr.Zero)
            {
                return new OmenKeyboard(kboardPointer);
            }

            return null;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct StaticKeyEffect
        {
            [MarshalAs(UnmanagedType.Struct)]
            public LightingColor lightingColor;
            public int key;

        }

        public void SetKeys(Dictionary<DeviceKeys, Color> keyColors)
        {
            List<StaticKeyEffect> list = new List<StaticKeyEffect>();
            foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
            {
                StaticKeyEffect staticEffect = CreateStaticEffect(key.Key, key.Value);
                list.Add(staticEffect);
            }
            if (list.Count > 0)
            {
                list.ToArray();
                if (hKB != IntPtr.Zero)
                {
                    int res = OmenLighting_Keyboard_SetStatic(hKB, list.ToArray(), list.Count, IntPtr.Zero);
                    if (res != 0)
                    {
                        Global.logger.Error("OMEN Keyboard, Set static effect fail: " + res);
                    }
                }
            }
        }

        private static StaticKeyEffect CreateStaticEffect(DeviceKeys key, Color color)
        {
            double alpha_amt = (color.A / 255.0);

            LightingColor c = LightingColor.FromColor(color);
            StaticKeyEffect staticEffect = new StaticKeyEffect() { key = OmenKeys.GetKey(key), lightingColor = c };
            return staticEffect;

        }

        internal void Shutdown()
        {
            try
            {
                OmenLighting_Keyboard_Close(hKB);
                hKB = IntPtr.Zero;
            }
            catch (Exception exc)
            {
                Global.logger.Error("OMEN Keyboard, Exception during Shutdown. Message: " + exc);
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
