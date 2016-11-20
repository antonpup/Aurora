using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using static CoolerMaster.CoolerMasterSDK;

namespace CoolerMaster
{
    
    class CoolerMasterSDK
    {
        public enum EFF_INDEX
        {
            EFF_FULL_ON = 0,
            EFF_BREATH = 1,
            EFF_BREATH_CYCLE = 2,
            EFF_SINGLE = 3,
            EFF_WAVE = 4,
            EFF_RIPPLE = 5,
            EFF_CROSS = 6,
            EFF_RAIN = 7,
            EFF_STAR = 8,
            EFF_SNAKE = 9,
            EFF_REC = 10,
            EFF_SPECTRUM = 11,
            EFF_RAPID_FIRE = 12,
            EFF_INDICATOR = 13,
            EFF_MULTI_1 = 224,
            EFF_MULTI_2 = 225,
            EFF_MULTI_3 = 226,
            EFF_MULTI_4 = 227,
            EFF_OFF = 254,
        }

        public enum DEVICE_INDEX
        {
            DEV_MKeys_L = 0,
            DEV_MKeys_S = 1,
            DEV_MKeys_L_White = 2,
            DEV_MKeys_M_White = 3,
            DEV_MMouse_L = 4,
        }

        public enum LAYOUT_KEYBOARD
        {
            LAYOUT_UNINIT = 0,
            LAYOUT_US = 1,
            LAYOUT_EU = 2,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEY_COLOR
        {
            public byte r;
            public byte g;
            public byte b;

            public KEY_COLOR(byte r, byte g, byte b)
            {
                this.r = r;
                this.g = g;
                this.b = b;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COLOR_MATRIX
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_LED_ROW * MAX_LED_COLUMN, ArraySubType = UnmanagedType.Struct)]
            public KEY_COLOR[,] KeyColor;
        }

        public const int MAX_LED_ROW = 6;

        public const int MAX_LED_COLUMN = 22;

        public const string sdkDLL = @"SDKDLL.dll";

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void KEY_CALLBACK(int iRow, int iColumn, [MarshalAs(UnmanagedType.I1)] bool bPressed);

        [DllImport(sdkDLL, EntryPoint = "GetNowTime")]
        public static extern IntPtr GetNowTime();

        [DllImport(sdkDLL, EntryPoint = "GetNowCPUUsage", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetNowCPUUsage();

        [DllImport(sdkDLL, EntryPoint = "GetRamUsage")]
        public static extern uint GetRamUsage();

        [DllImport(sdkDLL, EntryPoint = "GetNowVolumePeekValue")]
        public static extern float GetNowVolumePeekValue();

        [DllImport(sdkDLL, EntryPoint = "SetControlDevice")]
        public static extern void SetControlDevice(DEVICE_INDEX devIndex);

        [DllImport(sdkDLL, EntryPoint = "IsDevicePlug")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsDevicePlug();

        [DllImport(sdkDLL, EntryPoint = "GetDeviceLayout")]
        public static extern LAYOUT_KEYBOARD GetDeviceLayout();

        [DllImport(sdkDLL, EntryPoint = "EnableLedControl")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool EnableLedControl([MarshalAs(UnmanagedType.I1)] bool bEnable);

        [DllImport(sdkDLL, EntryPoint = "SwitchLedEffect")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SwitchLedEffect(EFF_INDEX iEffectIndex);

        [DllImport(sdkDLL, EntryPoint = "RefreshLed")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RefreshLed([MarshalAs(UnmanagedType.I1)] bool bAuto);

        [DllImport(sdkDLL, EntryPoint = "SetFullLedColor")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SetFullLedColor(byte r, byte g, byte b);

        [DllImport(sdkDLL, EntryPoint = "SetAllLedColor")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SetAllLedColor(COLOR_MATRIX colorMatrix);

        [DllImport(sdkDLL, EntryPoint = "SetLedColor")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SetLedColor(int iRow, int iColumn, byte r, byte g, byte b);

        [DllImport(sdkDLL, EntryPoint = "EnableKeyInterrupt")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool EnableKeyInterrupt([MarshalAs(UnmanagedType.I1)] bool bEnable);

        [DllImport(sdkDLL, EntryPoint = "SetKeyCallBack")]
        public static extern void SetKeyCallBack(KEY_CALLBACK callback);

    }

}