using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
            [Description("None")]
            None = -1,
            [Description("MasterKeys Pro L")]
            DEV_MKeys_L = 0,
            [Description("MasterKeys Pro S")]
            DEV_MKeys_S = 1,
            [Description("MasterKeys Pro L White")]
            DEV_MKeys_L_White = 2,
            [Description("MasterKeys Pro M White")]
            DEV_MKeys_M_White = 3,
            [Description("MasterMouse Pro L")]
            DEV_MMouse_L = 4,
            [Description("MasterMouse Pro S")]
            DEV_MMouse_S = 5,
            [Description("MasterKeys Pro M")]
            DEV_MKeys_M = 6,
            [Description("MasterKeys Pro S White")]
            DEV_MKeys_S_White = 7,
            [Description("MasterMouse MM520")]
            DEV_MM520 = 8,
            [Description("MasterMouse MM530")]
            DEV_MM530 = 9,
            [Description("MasterKeys MK750")]
            DEV_MK750 = 10,
            [Description("CK372")]
            DEV_CK372 = 11,
            [Description("CK552")]
            DEV_CK550_552 = 12,
            [Description("CK551")]
            DEV_CK551 = 13,
            [Description("Default")]
            DEV_DEFAULT = 0xFFFF
        }

        public static List<DEVICE_INDEX> Mice = new List<DEVICE_INDEX>
        {
            DEVICE_INDEX.DEV_MMouse_L,
            DEVICE_INDEX.DEV_MMouse_S,
            DEVICE_INDEX.DEV_MM520,
            DEVICE_INDEX.DEV_MM530
        };

        public static List<DEVICE_INDEX> Keyboards = new List<DEVICE_INDEX>
        {
            DEVICE_INDEX.DEV_MKeys_L,
            DEVICE_INDEX.DEV_MKeys_L_White,
            DEVICE_INDEX.DEV_MKeys_M,
            DEVICE_INDEX.DEV_MKeys_M_White,
            DEVICE_INDEX.DEV_MKeys_S,
            DEVICE_INDEX.DEV_MKeys_S_White,
            DEVICE_INDEX.DEV_MK750,
            DEVICE_INDEX.DEV_CK372,
            DEVICE_INDEX.DEV_CK550_552,
            DEVICE_INDEX.DEV_CK551
        };

        public enum LAYOUT_KEYBOARD
        {
            LAYOUT_UNINIT = 0,
            LAYOUT_US = 1,
            LAYOUT_EU = 2,
            LAYOUT_JP = 3
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

        public const int MAX_LED_ROW = 7;

        public const int MAX_LED_COLUMN = 24;

        public const string sdkDLL = @"SDKDLL.dll";

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void KEY_CALLBACK(int iRow, int iColumn, [MarshalAs(UnmanagedType.I1)] bool bPressed);

        /// <summary>
        /// Get SDK Dll's Version
        /// </summary>
        /// <returns>DLL's Version</returns>
        [DllImport(sdkDLL, EntryPoint = "GetCM_SDK_DllVer")]
        public static extern int GetCM_SDK_DllVer();

        /// <summary>
        /// Obtain current system time
        /// </summary>
        /// <returns>TCHAR *: string index format is %Y %m/%d %H:%M %S</returns>
        [DllImport(sdkDLL, EntryPoint = "GetNowTime")]
        public static extern IntPtr GetNowTime();

        /// <summary>
        /// obtain current CPU usuage ratio
        /// </summary>
        /// <returns>0 ~ 100 integer</returns>
        [DllImport(sdkDLL, EntryPoint = "GetNowCPUUsage", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetNowCPUUsage();

        /// <summary>
        /// Obtain current RAM usuage ratio
        /// </summary>
        /// <returns>0 ~ 100 integer</returns>
        [DllImport(sdkDLL, EntryPoint = "GetRamUsage")]
        public static extern uint GetRamUsage();

        /// <summary>
        /// Obtain current volume
        /// </summary>
        /// <returns>0 ~ 1 float number</returns>
        [DllImport(sdkDLL, EntryPoint = "GetNowVolumePeekValue")]
        public static extern float GetNowVolumePeekValue();

        /// <summary>
        /// set operating device
        /// </summary>
        /// <param name="devIndex"></param>
        [DllImport(sdkDLL, EntryPoint = "SetControlDevice")]
        public static extern void SetControlDevice(DEVICE_INDEX devIndex);

        /// <summary>
        /// verify if the deviced is plugged in
        /// </summary>
        /// <param name="devIndex"></param>
        /// <returns>true plugged in，false not plugged in</returns>
        [DllImport(sdkDLL, EntryPoint = "IsDevicePlug")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsDevicePlug(DEVICE_INDEX devIndex = DEVICE_INDEX.DEV_DEFAULT);

        /// <summary>
        /// Obtain current device layout
        /// </summary>
        /// <param name="devIndex"></param>
        /// <returns></returns>
        [DllImport(sdkDLL, EntryPoint = "GetDeviceLayout")]
        public static extern LAYOUT_KEYBOARD GetDeviceLayout(DEVICE_INDEX devIndex = DEVICE_INDEX.DEV_DEFAULT);

        /// <summary>
        /// set control over device’s LED
        /// </summary>
        /// <param name="bEnable">true Controlled by SW，false Controlled by FW</param>
        /// <param name="devIndex"></param>
        /// <returns>true Success，false Fail</returns>
        [DllImport(sdkDLL, EntryPoint = "EnableLedControl")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool EnableLedControl([MarshalAs(UnmanagedType.I1)] bool bEnable, DEVICE_INDEX devIndex = DEVICE_INDEX.DEV_DEFAULT);

        /// <summary>
        /// switch device current effect
        /// </summary>
        /// <param name="iEffectIndex">index value of the effect</param>
        /// <param name="devIndex"></param>
        /// <returns>true Success，false Fail</returns>
        [DllImport(sdkDLL, EntryPoint = "SwitchLedEffect")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SwitchLedEffect(EFF_INDEX iEffectIndex, DEVICE_INDEX devIndex = DEVICE_INDEX.DEV_DEFAULT);

        /// <summary>
        ///  Print out the lights setting from Buffer to LED
        /// </summary>
        /// <param name="bAuto">false means manual, call this function once, then print out once; true means auto, any light update will print out directly</param>
        /// <param name="devIndex"></param>
        /// <returns>true success，false fail</returns>
        [DllImport(sdkDLL, EntryPoint = "RefreshLed")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RefreshLed([MarshalAs(UnmanagedType.I1)] bool bAuto, DEVICE_INDEX devIndex = DEVICE_INDEX.DEV_DEFAULT);

        /// <summary>
        /// set entire keyboard LED color
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="devIndex"></param>
        /// <returns>true Success，false Fail</returns>
        [DllImport(sdkDLL, EntryPoint = "SetFullLedColor")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SetFullLedColor(byte r, byte g, byte b, DEVICE_INDEX devIndex = DEVICE_INDEX.DEV_DEFAULT);

        /// <summary>
        /// Set Keyboard "every LED" color
        /// </summary>
        /// <param name="colorMatrix">structure，fill up RGB value according to LED Table</param>
        /// <param name="devIndex"></param>
        /// <returns>true Success，false Fail</returns>
        [DllImport(sdkDLL, EntryPoint = "SetAllLedColor")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SetAllLedColor(COLOR_MATRIX colorMatrix, DEVICE_INDEX devIndex = DEVICE_INDEX.DEV_DEFAULT);

        /// <summary>
        /// Set single Key LED color
        /// </summary>
        /// <param name="iRow"></param>
        /// <param name="iColumn"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="devIndex"></param>
        /// <returns>true Success，false Fail</returns>
        [DllImport(sdkDLL, EntryPoint = "SetLedColor")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SetLedColor(int iRow, int iColumn, byte r, byte g, byte b, DEVICE_INDEX devIndex = DEVICE_INDEX.DEV_DEFAULT);

        /// <summary>
        /// To enable the call back function
        /// Note: will call the call back function of SetKeyCallBack()
        /// </summary>
        /// <param name="bEnable">true enable ，false disable</param>
        /// <param name="devIndex"></param>
        /// <returns>true sucess, false fail</returns>
        [DllImport(sdkDLL, EntryPoint = "EnableKeyInterrupt")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool EnableKeyInterrupt([MarshalAs(UnmanagedType.I1)] bool bEnable, DEVICE_INDEX devIndex = DEVICE_INDEX.DEV_DEFAULT);

        /// <summary>
        /// Setup the call back function of button
        /// </summary>
        /// <param name="callback">callback call back setup，please reference the def of KEY_CALLBACK</param>
        /// <param name="devIndex"></param>
        [DllImport(sdkDLL, EntryPoint = "SetKeyCallBack")]
        public static extern void SetKeyCallBack(KEY_CALLBACK callback, DEVICE_INDEX devIndex = DEVICE_INDEX.DEV_DEFAULT);

    }

}