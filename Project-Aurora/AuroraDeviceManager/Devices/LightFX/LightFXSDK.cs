using System.Runtime.InteropServices;

namespace AuroraDeviceManager.Devices.LightFX
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

        private const string LightfxSdkDll = "x64\\LightFX_SDK.dll";

        private const string LightfxDll = "x64\\LightFX.dll";

        public static void ResetColors()
        {
            color1.Reset();
            color2.Reset();
            color3.Reset();
            color4.Reset();
            color5.Reset();
        }

        [DllImport(LightfxDll)]
        public static extern uint LFX_Initialize();

        [DllImport(LightfxDll)]
        public static extern uint LFX_Release();

        [DllImport(LightfxDll)]
        public static extern uint LFX_Reset();

        [DllImport(LightfxDll)]
        public static extern uint LFX_Update();

        [DllImport(LightfxDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_GetNumDevices(ref uint numDevices);

        [DllImport(LightfxDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_GetDeviceDescription(uint devIndex, ref char description, uint descSize, ref byte devtype);

        [DllImport(LightfxDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_SetTiming(int timing);

        [DllImport(LightfxDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_GetNumLights(uint devIndex, ref uint numLights);

        [DllImport(LightfxDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_GetLightColor(uint devIndex, uint lightIndex, ref LFX_COLOR color);

        [DllImport(LightfxDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_SetLightColor(uint devIndex, uint lightIndex, ref LFX_COLOR color);

        [DllImport(LightfxSdkDll, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool HIDInitialize(int vid, int pid);

        [DllImport(LightfxSdkDll, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern int LightFXInitialize(int vid);

        [DllImport(LightfxSdkDll)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool HIDClose();

        [DllImport(LightfxSdkDll)]
        public static extern int GetError();

        [DllImport(LightfxSdkDll)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool getReadStatus();

        [DllImport(LightfxSdkDll, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool HIDWrite(byte[] Buffer, int len);

        [DllImport(LightfxSdkDll, CallingConvention = CallingConvention.Cdecl)]
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
