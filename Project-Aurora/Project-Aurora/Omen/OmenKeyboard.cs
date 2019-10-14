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
    public class OmenKeyboard
    {
        private readonly IntPtr kboardPointer;

        private OmenKeyboard(IntPtr kboardPointer)
        {
            this.kboardPointer = kboardPointer;
        }

        public static OmenKeyboard GetOmenKeyboard()
        {
            // IntPtr kboardPointer = default(IntPtr);
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

            [MarshalAs(UnmanagedType.Struct)]
            public KeyLocation keyLocation;

        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct KeyLocation
        {
            public int row;
            public int col;
        }




        public void SetKeys(Dictionary<DeviceKeys, Color> keyColors)
        {
            List<StaticKeyEffect> list = new List<StaticKeyEffect>();
            foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
            {
                StaticKeyEffect staticEffect = CreateStaticEffect(key.Key, key.Value);
                if (staticEffect.keyLocation.row >= 0 && staticEffect.keyLocation.col >= 0)
                {
                    list.Add(staticEffect);
                }
            }
            if (list.Count > 0)
            {
                list.ToArray();
                OmenLighting_Keyboard_SetStaticEffect(kboardPointer, list.ToArray(), list.Count, IntPtr.Zero);
            }
        }


        public void SetSingleKey(DeviceKeys key, Color color)
        {
            //StaticKeyEffect staticEffect = CreateStaticEffect(key, color);
            //OmenLighting_SetSingleKeyStaticEffect(kboardPointer, staticEffect);
            SetKeys(new Dictionary<DeviceKeys, Color> { { key, color } });
        }

        private static StaticKeyEffect CreateStaticEffect(DeviceKeys key, Color color)
        {
            double alpha_amt = (color.A / 255.0);
            KeyLocation keyLoc = GetKeyLocation(key);


            LightingColor c = LightingColor.fromColor(color);
            StaticKeyEffect staticEffect = new StaticKeyEffect() { keyLocation = keyLoc, lightingColor = c };
            return staticEffect;

        }

        private static KeyLocation GetKeyLocation(DeviceKeys key)
        {
            KeyLocation defaultLoc = new KeyLocation() { row = -1, col = -1 };
            switch (key)
            {
                case (DeviceKeys.LOGO):
                    return defaultLoc;
                case (DeviceKeys.BRIGHTNESS_SWITCH):
                    return defaultLoc;
                case (DeviceKeys.LOCK_SWITCH):
                    return defaultLoc;
                case (DeviceKeys.VOLUME_MUTE):
                    return defaultLoc;
                case (DeviceKeys.VOLUME_UP):
                    return defaultLoc;
                case (DeviceKeys.VOLUME_DOWN):
                    return defaultLoc;
                case (DeviceKeys.MEDIA_STOP):
                    return new KeyLocation() { row = 0, col = 17 };
                case (DeviceKeys.MEDIA_PLAY_PAUSE):
                    return new KeyLocation() { row = 0, col = 16 };
                case (DeviceKeys.MEDIA_PREVIOUS):
                    return new KeyLocation() { row = 0, col = 18 };
                case (DeviceKeys.MEDIA_NEXT):
                    return new KeyLocation() { row = 0, col = 19 };
                case (DeviceKeys.ESC):
                    return new KeyLocation() { row = 0, col = 0 };
                case (DeviceKeys.F1):
                    return new KeyLocation() { row = 0, col = 1 };
                case (DeviceKeys.F2):
                    return new KeyLocation() { row = 0, col = 2 };
                case (DeviceKeys.F3):
                    return new KeyLocation() { row = 0, col = 3 };
                case (DeviceKeys.F4):
                    return new KeyLocation() { row = 0, col = 4 };
                case (DeviceKeys.F5):
                    return new KeyLocation() { row = 0, col = 5 };
                case (DeviceKeys.F6):
                    return new KeyLocation() { row = 0, col = 6 };
                case (DeviceKeys.F7):
                    return new KeyLocation() { row = 0, col = 7 };
                case (DeviceKeys.F8):
                    return new KeyLocation() { row = 0, col = 8 };
                case (DeviceKeys.F9):
                    return new KeyLocation() { row = 0, col = 9 };
                case (DeviceKeys.F10):
                    return new KeyLocation() { row = 0, col = 10 };
                case (DeviceKeys.F11):
                    return new KeyLocation() { row = 0, col = 11 };
                case (DeviceKeys.F12):
                    return new KeyLocation() { row = 0, col = 12 };
                case (DeviceKeys.PRINT_SCREEN):
                    return new KeyLocation() { row = 0, col = 13 };
                case (DeviceKeys.SCROLL_LOCK):
                    return new KeyLocation() { row = 0, col = 14 };
                case (DeviceKeys.PAUSE_BREAK):
                    return new KeyLocation() { row = 0, col = 15 };
                case (DeviceKeys.TILDE):
                    return new KeyLocation() { row = 1, col = 1 };
                case (DeviceKeys.ONE):
                    return new KeyLocation() { row = 1, col = 2 };
                case (DeviceKeys.TWO):
                    return new KeyLocation() { row = 1, col = 3 };
                case (DeviceKeys.THREE):
                    return new KeyLocation() { row = 1, col = 4 };
                case (DeviceKeys.FOUR):
                    return new KeyLocation() { row = 1, col = 5 };
                case (DeviceKeys.FIVE):
                    return new KeyLocation() { row = 1, col = 6 };
                case (DeviceKeys.SIX):
                    return new KeyLocation() { row = 1, col = 7 };
                case (DeviceKeys.SEVEN):
                    return new KeyLocation() { row = 1, col = 8 };
                case (DeviceKeys.EIGHT):
                    return new KeyLocation() { row = 1, col = 9 };
                case (DeviceKeys.NINE):
                    return new KeyLocation() { row = 1, col = 10 };
                case (DeviceKeys.ZERO):
                    return new KeyLocation() { row = 1, col = 11 };
                case (DeviceKeys.MINUS):
                    return new KeyLocation() { row = 1, col = 12 };
                case (DeviceKeys.EQUALS):
                    return new KeyLocation() { row = 1, col = 13 };
                case (DeviceKeys.BACKSPACE):
                    return new KeyLocation() { row = 1, col = 14 };
                case (DeviceKeys.INSERT):
                    return new KeyLocation() { row = 1, col = 15 };
                case (DeviceKeys.HOME):
                    return new KeyLocation() { row = 1, col = 16 };
                case (DeviceKeys.PAGE_UP):
                    return new KeyLocation() { row = 1, col = 17 };
                case (DeviceKeys.NUM_LOCK):
                    return new KeyLocation() { row = 1, col = 18 };
                case (DeviceKeys.NUM_SLASH):
                    return new KeyLocation() { row = 1, col = 19 };
                case (DeviceKeys.NUM_ASTERISK):
                    return new KeyLocation() { row = 1, col = 20 };
                case (DeviceKeys.NUM_MINUS):
                    return new KeyLocation() { row = 1, col = 21 };
                case (DeviceKeys.TAB):
                    return new KeyLocation() { row = 2, col = 1 };
                case (DeviceKeys.Q):
                    return new KeyLocation() { row = 2, col = 2 };
                case (DeviceKeys.W):
                    return new KeyLocation() { row = 2, col = 3 };
                case (DeviceKeys.E):
                    return new KeyLocation() { row = 2, col = 4 };
                case (DeviceKeys.R):
                    return new KeyLocation() { row = 2, col = 5 };
                case (DeviceKeys.T):
                    return new KeyLocation() { row = 2, col = 6 };
                case (DeviceKeys.Y):
                    return new KeyLocation() { row = 2, col = 7 };
                case (DeviceKeys.U):
                    return new KeyLocation() { row = 2, col = 8 };
                case (DeviceKeys.I):
                    return new KeyLocation() { row = 2, col = 9 };
                case (DeviceKeys.O):
                    return new KeyLocation() { row = 2, col = 10 };
                case (DeviceKeys.P):
                    return new KeyLocation() { row = 2, col = 11 };
                case (DeviceKeys.OPEN_BRACKET):
                    return new KeyLocation() { row = 2, col = 12 };
                case (DeviceKeys.CLOSE_BRACKET):
                    return new KeyLocation() { row = 2, col = 13 };
                case (DeviceKeys.BACKSLASH):
                    return new KeyLocation() { row = 2, col = 14 };
                case (DeviceKeys.DELETE):
                    return new KeyLocation() { row = 2, col = 15 };
                case (DeviceKeys.END):
                    return new KeyLocation() { row = 2, col = 16 };
                case (DeviceKeys.PAGE_DOWN):
                    return new KeyLocation() { row = 2, col = 17 };
                case (DeviceKeys.NUM_SEVEN):
                    return new KeyLocation() { row = 2, col = 18 };
                case (DeviceKeys.NUM_EIGHT):
                    return new KeyLocation() { row = 2, col = 19 };
                case (DeviceKeys.NUM_NINE):
                    return new KeyLocation() { row = 2, col = 20 };
                case (DeviceKeys.NUM_PLUS):
                    return new KeyLocation() { row = 2, col = 21 };
                case (DeviceKeys.CAPS_LOCK):
                    return new KeyLocation() { row = 3, col = 1 };
                case (DeviceKeys.A):
                    return new KeyLocation() { row = 3, col = 2 };
                case (DeviceKeys.S):
                    return new KeyLocation() { row = 3, col = 3 };
                case (DeviceKeys.D):
                    return new KeyLocation() { row = 3, col = 4 };
                case (DeviceKeys.F):
                    return new KeyLocation() { row = 3, col = 5 };
                case (DeviceKeys.G):
                    return new KeyLocation() { row = 3, col = 6 };
                case (DeviceKeys.H):
                    return new KeyLocation() { row = 3, col = 7 };
                case (DeviceKeys.J):
                    return new KeyLocation() { row = 3, col = 8 };
                case (DeviceKeys.K):
                    return new KeyLocation() { row = 3, col = 9 };
                case (DeviceKeys.L):
                    return new KeyLocation() { row = 3, col = 10 };
                case (DeviceKeys.SEMICOLON):
                    return new KeyLocation() { row = 3, col = 11 };
                case (DeviceKeys.APOSTROPHE):
                    return new KeyLocation() { row = 3, col = 12 };
                case (DeviceKeys.HASHTAG):
                    return defaultLoc;
                case (DeviceKeys.ENTER):
                    return new KeyLocation() { row = 3, col = 13 };
                case (DeviceKeys.NUM_FOUR):
                    return new KeyLocation() { row = 3, col = 14 };
                case (DeviceKeys.NUM_FIVE):
                    return new KeyLocation() { row = 3, col = 15 };
                case (DeviceKeys.NUM_SIX):
                    return new KeyLocation() { row = 3, col = 16 };
                case (DeviceKeys.LEFT_SHIFT):
                    return new KeyLocation() { row = 4, col = 1 };
                case (DeviceKeys.BACKSLASH_UK):
                    return defaultLoc;
                case (DeviceKeys.Z):
                    return new KeyLocation() { row = 4, col = 2 };
                case (DeviceKeys.X):
                    return new KeyLocation() { row = 4, col = 3 };
                case (DeviceKeys.C):
                    return new KeyLocation() { row = 4, col = 4 };
                case (DeviceKeys.V):
                    return new KeyLocation() { row = 4, col = 5 };
                case (DeviceKeys.B):
                    return new KeyLocation() { row = 4, col = 6 };
                case (DeviceKeys.N):
                    return new KeyLocation() { row = 4, col = 7 };
                case (DeviceKeys.M):
                    return new KeyLocation() { row = 4, col = 8 };
                case (DeviceKeys.COMMA):
                    return new KeyLocation() { row = 4, col = 9 };
                case (DeviceKeys.PERIOD):
                    return new KeyLocation() { row = 4, col = 10 };
                case (DeviceKeys.FORWARD_SLASH):
                    return new KeyLocation() { row = 4, col = 11 };
                case (DeviceKeys.OEM8):
                    return defaultLoc;
                case (DeviceKeys.OEM102):
                    return defaultLoc;
                case (DeviceKeys.RIGHT_SHIFT):
                    return new KeyLocation() { row = 4, col = 12 };
                case (DeviceKeys.ARROW_UP):
                    return new KeyLocation() { row = 5, col = 9 };
                case (DeviceKeys.NUM_ONE):
                    return new KeyLocation() { row = 4, col = 13 };
                case (DeviceKeys.NUM_TWO):
                    return new KeyLocation() { row = 4, col = 14 };
                case (DeviceKeys.NUM_THREE):
                    return new KeyLocation() { row = 4, col = 15 };
                case (DeviceKeys.NUM_ENTER):
                    return new KeyLocation() { row = 5, col = 12 };
                case (DeviceKeys.LEFT_CONTROL):
                    return new KeyLocation() { row = 5, col = 1 };
                case (DeviceKeys.LEFT_WINDOWS):
                    return new KeyLocation() { row = 5, col = 2 };
                case (DeviceKeys.LEFT_ALT):
                    return new KeyLocation() { row = 5, col = 3 };
                case (DeviceKeys.SPACE):
                    return new KeyLocation() { row = 6, col = 1 };
                case (DeviceKeys.RIGHT_ALT):
                    return new KeyLocation() { row = 5, col = 5 };
                case (DeviceKeys.RIGHT_WINDOWS):
                    return new KeyLocation() { row = 5, col = 7 };
                case (DeviceKeys.APPLICATION_SELECT):
                    return new KeyLocation() { row = 5, col = 6 };
                case (DeviceKeys.RIGHT_CONTROL):
                    return new KeyLocation() { row = 5, col = 8 };
                case (DeviceKeys.ARROW_LEFT):
                    return new KeyLocation() { row = 6, col = 0 };
                case (DeviceKeys.ARROW_DOWN):
                    return new KeyLocation() { row = 6, col = 1 };
                case (DeviceKeys.ARROW_RIGHT):
                    return new KeyLocation() { row = 6, col = 2 };
                case (DeviceKeys.NUM_ZERO):
                    return new KeyLocation() { row = 5, col = 10 };
                case (DeviceKeys.NUM_PERIOD):
                    return new KeyLocation() { row = 5, col = 11 };
                case (DeviceKeys.FN_Key):
                    return new KeyLocation() { row = 5, col = 6 };
                case (DeviceKeys.G1):
                    return new KeyLocation() { row = 1, col = 0 };
                case (DeviceKeys.G2):
                    return new KeyLocation() { row = 2, col = 0 };
                case (DeviceKeys.G3):
                    return new KeyLocation() { row = 3, col = 0 };
                case (DeviceKeys.G4):
                    return new KeyLocation() { row = 4, col = 0 };
                case (DeviceKeys.G5):
                    return new KeyLocation() { row = 5, col = 0 };
                default:
                    return defaultLoc;
            }
        }

        internal void Shutdown()
        {
            try
            {
                OmenLighting_Keyboard_Close(kboardPointer);
            }
            catch (Exception exc)
            {
                Global.logger.Error("OMEN Keyboard, Exception during Shutdown. Message: " + exc);
            }
        }

        [DllImport("OmenLightingSDK.dll")]
        static extern void OmenLighting_Keyboard_Close(IntPtr hKeyboard);

        [DllImport("OmenLightingSDK.dll")]
        static extern int OmenLighting_Keyboard_SetStaticEffect(IntPtr hKeyboard, StaticKeyEffect[] staticEffect, int count, IntPtr keyboardLightingEffectProperty);

        //[DllImport("OmenLightingSDK.dll")]
        //static extern bool OmenLighting_SetSingleKeyStaticEffect(IntPtr hKeyboard, StaticEffect staticEffect);

        [DllImport("OmenLightingSDK.dll")]
        static extern IntPtr OmenLighting_Keyboard_Open();
    }

}
