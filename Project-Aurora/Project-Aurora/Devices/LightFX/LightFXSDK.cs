using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LightFXAPI
{
    public enum BITMASK
    {
        leftZone = 0x8,
        leftMiddleZone = 0x4,
        rightZone = 0x1,
        rightMiddleZone = 0x2,
        AlienFrontLogo = 0x40,
        AlienBackLogo = 0x20,
        LeftPanelTop = 0x1000,
        LeftPanelBottom = 0x400,
        RightPanelTop = 0x2000,
        RightPanelBottom = 0x800,
        TouchPad = 0x80,
    }
    class LightFXSDK
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct LFX_COLOR
        {
            public byte red;
            public byte green;
            public byte blue;

            public byte brightness;

            public void Reset()
            {
                this.SetRGB(0, 0, 0);
            }

            public void SetRGB(byte r, byte g, byte b, byte brightness = 255)
            {
                this.brightness = 255;
                this.red = r;
                this.green = g;
                this.blue = b;
            }
        };

        public static LFX_COLOR color, color1, color2, color3, color4, color5 = new LFX_COLOR();

        public static void ResetColors()
        {
            color1.Reset();
            color2.Reset();
            color3.Reset();
            color4.Reset();
            color5.Reset();
        }

        [DllImport("LightFX.dll")]
        public static extern uint LFX_Initialize();

        [DllImport("LightFX.dll")]
        public static extern uint LFX_Release();

        [DllImport("LightFX.dll")]
        public static extern uint LFX_Reset();

        [DllImport("LightFX.dll")]
        public static extern uint LFX_Update();

        [DllImport("LightFX.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_GetNumDevices(ref uint numDevices);

        [DllImport("LightFX.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_GetDeviceDescription(uint devIndex, ref char description, uint descSize, ref byte devtype);

        [DllImport("LightFX.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_SetTiming(int timing);

        [DllImport("LightFX.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_GetNumLights(uint devIndex, ref uint numLights);

        [DllImport("LightFX.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_GetLightColor(uint devIndex, uint lightIndex, ref LFX_COLOR color);

        [DllImport("LightFX.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_SetLightColor(uint devIndex, uint lightIndex, ref LFX_COLOR color);

        [DllImport("LightFX_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool HIDInitialize(int vid, int pid);

        [DllImport("LightFX_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern int LightFXInitialize(int vid);

        [DllImport("LightFX_SDK.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool HIDClose();

        [DllImport("LightFX_SDK.dll")]
        public static extern int GetError();

        [DllImport("LightFX_SDK.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool getReadStatus();

        [DllImport("LightFX_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool HIDWrite(byte[] Buffer, int len);

        [DllImport("LightFX_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int HIDRead(byte[] Buffer, int len);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        public const int LFX_SUCCESS = 0;
        // Success

        public const int LFX_FAILURE = 1;
        // Generic failure

        public const int LFX_ERROR_NOINIT = 2;
        // System not initialized yet

        public const int LFX_ERROR_NODEVS = 3;
        // No devices available

        public const int LFX_ERROR_NOLIGHTS = 4;
        // No lights available

        public const int LFX_ERROR_BUFFSIZE = 5;
        // Buffer size too small

        public const int LFX_FIRSTEVENT = 0;
        // First event

        public const int LFX_NEXTEVENT = 1;
        // Next event

        public const int LFX_GAME = 0;
        // The application is a game

        public const int LFX_GENERALUSEAPP = 1;
        // It is a general use application

    }
}
