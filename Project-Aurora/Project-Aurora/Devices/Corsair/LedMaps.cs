using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Utils;
using CorsairRGB.NET.Enums;
using CorsairRGB.NET.Structures;

namespace Aurora.Devices.Corsair
{
    internal static class LedMaps
    {
        internal static readonly Dictionary<DeviceKeys, CorsairLedId> KeyboardLedMap = new Dictionary<DeviceKeys, CorsairLedId>()
        {
            [DeviceKeys.ESC] = CorsairLedId.K_Escape,
            [DeviceKeys.F1] = CorsairLedId.K_F1,
            [DeviceKeys.F2] = CorsairLedId.K_F2,
            [DeviceKeys.F3] = CorsairLedId.K_F3,
            [DeviceKeys.F4] = CorsairLedId.K_F4,
            [DeviceKeys.F5] = CorsairLedId.K_F5,
            [DeviceKeys.F6] = CorsairLedId.K_F6,
            [DeviceKeys.F7] = CorsairLedId.K_F7,
            [DeviceKeys.F8] = CorsairLedId.K_F8,
            [DeviceKeys.F9] = CorsairLedId.K_F9,
            [DeviceKeys.F10] = CorsairLedId.K_F10,
            [DeviceKeys.F11] = CorsairLedId.K_F11,
            [DeviceKeys.TILDE] = CorsairLedId.K_GraveAccentAndTilde,
            [DeviceKeys.ONE] = CorsairLedId.K_1,
            [DeviceKeys.TWO] = CorsairLedId.K_2,
            [DeviceKeys.THREE] = CorsairLedId.K_3,
            [DeviceKeys.FOUR] = CorsairLedId.K_4,
            [DeviceKeys.FIVE] = CorsairLedId.K_5,
            [DeviceKeys.SIX] = CorsairLedId.K_6,
            [DeviceKeys.SEVEN] = CorsairLedId.K_7,
            [DeviceKeys.EIGHT] = CorsairLedId.K_8,
            [DeviceKeys.NINE] = CorsairLedId.K_9,
            [DeviceKeys.ZERO] = CorsairLedId.K_0,
            [DeviceKeys.MINUS] = CorsairLedId.K_MinusAndUnderscore,
            [DeviceKeys.TAB] = CorsairLedId.K_Tab,
            [DeviceKeys.Q] = CorsairLedId.K_Q,
            [DeviceKeys.W] = CorsairLedId.K_W,
            [DeviceKeys.E] = CorsairLedId.K_E,
            [DeviceKeys.R] = CorsairLedId.K_R,
            [DeviceKeys.T] = CorsairLedId.K_T,
            [DeviceKeys.Y] = CorsairLedId.K_Y,
            [DeviceKeys.U] = CorsairLedId.K_U,
            [DeviceKeys.I] = CorsairLedId.K_I,
            [DeviceKeys.O] = CorsairLedId.K_O,
            [DeviceKeys.P] = CorsairLedId.K_P,
            [DeviceKeys.OPEN_BRACKET] = CorsairLedId.K_BracketLeft,
            [DeviceKeys.CAPS_LOCK] = CorsairLedId.K_CapsLock,
            [DeviceKeys.A] = CorsairLedId.K_A,
            [DeviceKeys.S] = CorsairLedId.K_S,
            [DeviceKeys.D] = CorsairLedId.K_D,
            [DeviceKeys.F] = CorsairLedId.K_F,
            [DeviceKeys.G] = CorsairLedId.K_G,
            [DeviceKeys.H] = CorsairLedId.K_H,
            [DeviceKeys.J] = CorsairLedId.K_J,
            [DeviceKeys.K] = CorsairLedId.K_K,
            [DeviceKeys.L] = CorsairLedId.K_L,
            [DeviceKeys.SEMICOLON] = CorsairLedId.K_SemicolonAndColon,
            [DeviceKeys.APOSTROPHE] = CorsairLedId.K_ApostropheAndDoubleQuote,
            [DeviceKeys.LEFT_SHIFT] = CorsairLedId.K_LeftShift,
            [DeviceKeys.BACKSLASH_UK] = CorsairLedId.K_NonUsBackslash,
            [DeviceKeys.Z] = CorsairLedId.K_Z,
            [DeviceKeys.X] = CorsairLedId.K_X,
            [DeviceKeys.C] = CorsairLedId.K_C,
            [DeviceKeys.V] = CorsairLedId.K_V,
            [DeviceKeys.B] = CorsairLedId.K_B,
            [DeviceKeys.N] = CorsairLedId.K_N,
            [DeviceKeys.M] = CorsairLedId.K_M,
            [DeviceKeys.COMMA] = CorsairLedId.K_CommaAndLessThan,
            [DeviceKeys.PERIOD] = CorsairLedId.K_PeriodAndBiggerThan,
            [DeviceKeys.FORWARD_SLASH] = CorsairLedId.K_SlashAndQuestionMark,
            [DeviceKeys.LEFT_CONTROL] = CorsairLedId.K_LeftCtrl,
            [DeviceKeys.LEFT_WINDOWS] = CorsairLedId.K_LeftGui,
            [DeviceKeys.LEFT_ALT] = CorsairLedId.K_LeftAlt,
            //[DeviceKeys.Lang2] = CorsairLedId.K_Lang2,
            [DeviceKeys.SPACE] = CorsairLedId.K_Space,
            //[DeviceKeys.Lang1] = CorsairLedId.K_Lang1,
            //[DeviceKeys.International2] = CorsairLedId.K_International2,
            [DeviceKeys.RIGHT_ALT] = CorsairLedId.K_RightAlt,
            [DeviceKeys.RIGHT_WINDOWS] = CorsairLedId.K_RightGui,
            [DeviceKeys.APPLICATION_SELECT] = CorsairLedId.K_Application,
            //[DeviceKeys.LedProgramming] = CorsairLedId.K_LedProgramming,
            [DeviceKeys.BRIGHTNESS_SWITCH] = CorsairLedId.K_Brightness,
            [DeviceKeys.F12] = CorsairLedId.K_F12,
            [DeviceKeys.PRINT_SCREEN] = CorsairLedId.K_PrintScreen,
            [DeviceKeys.SCROLL_LOCK] = CorsairLedId.K_ScrollLock,
            [DeviceKeys.PAUSE_BREAK] = CorsairLedId.K_PauseBreak,
            [DeviceKeys.INSERT] = CorsairLedId.K_Insert,
            [DeviceKeys.HOME] = CorsairLedId.K_Home,
            [DeviceKeys.PAGE_UP] = CorsairLedId.K_PageUp,
            [DeviceKeys.CLOSE_BRACKET] = CorsairLedId.K_BracketRight,
            [DeviceKeys.BACKSLASH] = CorsairLedId.K_Backslash,
            [DeviceKeys.HASHTAG] = CorsairLedId.K_NonUsTilde,
            [DeviceKeys.ENTER] = CorsairLedId.K_Enter,
            //[DeviceKeys.International1] = CorsairLedId.K_International1,
            [DeviceKeys.EQUALS] = CorsairLedId.K_EqualsAndPlus,
            //[DeviceKeys.International3] = CorsairLedId.K_International3,
            [DeviceKeys.BACKSPACE] = CorsairLedId.K_Backspace,
            [DeviceKeys.DELETE] = CorsairLedId.K_Delete,
            [DeviceKeys.END] = CorsairLedId.K_End,
            [DeviceKeys.PAGE_DOWN] = CorsairLedId.K_PageDown,
            [DeviceKeys.RIGHT_SHIFT] = CorsairLedId.K_RightShift,
            [DeviceKeys.RIGHT_CONTROL] = CorsairLedId.K_RightCtrl,
            [DeviceKeys.ARROW_UP] = CorsairLedId.K_UpArrow,
            [DeviceKeys.ARROW_LEFT] = CorsairLedId.K_LeftArrow,
            [DeviceKeys.ARROW_DOWN] = CorsairLedId.K_DownArrow,
            [DeviceKeys.ARROW_RIGHT] = CorsairLedId.K_RightArrow,
            [DeviceKeys.LOCK_SWITCH] = CorsairLedId.K_WinLock,
            [DeviceKeys.VOLUME_MUTE] = CorsairLedId.K_Mute,
            [DeviceKeys.MEDIA_STOP] = CorsairLedId.K_Stop,
            [DeviceKeys.MEDIA_PREVIOUS] = CorsairLedId.K_ScanPreviousTrack,
            [DeviceKeys.MEDIA_PLAY_PAUSE] = CorsairLedId.K_PlayPause,
            [DeviceKeys.MEDIA_NEXT] = CorsairLedId.K_ScanNextTrack,
            [DeviceKeys.NUM_LOCK] = CorsairLedId.K_NumLock,
            [DeviceKeys.NUM_SLASH] = CorsairLedId.K_KeypadSlash,
            [DeviceKeys.NUM_ASTERISK] = CorsairLedId.K_KeypadAsterisk,
            [DeviceKeys.NUM_MINUS] = CorsairLedId.K_KeypadMinus,
            [DeviceKeys.NUM_PLUS] = CorsairLedId.K_KeypadPlus,
            [DeviceKeys.NUM_ENTER] = CorsairLedId.K_KeypadEnter,
            [DeviceKeys.NUM_SEVEN] = CorsairLedId.K_Keypad7,
            [DeviceKeys.NUM_EIGHT] = CorsairLedId.K_Keypad8,
            [DeviceKeys.NUM_NINE] = CorsairLedId.K_Keypad9,
            [DeviceKeys.NUM_ZEROZERO] = CorsairLedId.K_KeypadComma,
            [DeviceKeys.NUM_FOUR] = CorsairLedId.K_Keypad4,
            [DeviceKeys.NUM_FIVE] = CorsairLedId.K_Keypad5,
            [DeviceKeys.NUM_SIX] = CorsairLedId.K_Keypad6,
            [DeviceKeys.NUM_ONE] = CorsairLedId.K_Keypad1,
            [DeviceKeys.NUM_TWO] = CorsairLedId.K_Keypad2,
            [DeviceKeys.NUM_THREE] = CorsairLedId.K_Keypad3,
            [DeviceKeys.NUM_ZERO] = CorsairLedId.K_Keypad0,
            [DeviceKeys.NUM_PERIOD] = CorsairLedId.K_KeypadPeriodAndDelete,
            [DeviceKeys.G1] = CorsairLedId.K_G1,
            [DeviceKeys.G2] = CorsairLedId.K_G2,
            [DeviceKeys.G3] = CorsairLedId.K_G3,
            [DeviceKeys.G4] = CorsairLedId.K_G4,
            [DeviceKeys.G5] = CorsairLedId.K_G5,
            [DeviceKeys.G6] = CorsairLedId.K_G6,
            [DeviceKeys.G7] = CorsairLedId.K_G7,
            [DeviceKeys.G8] = CorsairLedId.K_G8,
            [DeviceKeys.G9] = CorsairLedId.K_G9,
            [DeviceKeys.G10] = CorsairLedId.K_G10,
            [DeviceKeys.VOLUME_UP] = CorsairLedId.K_VolumeUp,
            [DeviceKeys.VOLUME_DOWN] = CorsairLedId.K_VolumeDown,
            //[DeviceKeys.MR] = CorsairLedId.K_MR,
            //[DeviceKeys.M1] = CorsairLedId.K_M1,
            //[DeviceKeys.M2] = CorsairLedId.K_M2,
            //[DeviceKeys.M3] = CorsairLedId.K_M3,
            [DeviceKeys.G11] = CorsairLedId.K_G11,
            [DeviceKeys.G12] = CorsairLedId.K_G12,
            [DeviceKeys.G13] = CorsairLedId.K_G13,
            [DeviceKeys.G14] = CorsairLedId.K_G14,
            [DeviceKeys.G15] = CorsairLedId.K_G15,
            [DeviceKeys.G16] = CorsairLedId.K_G16,
            [DeviceKeys.G17] = CorsairLedId.K_G17,
            [DeviceKeys.G18] = CorsairLedId.K_G18,
            //[DeviceKeys.International5] = CorsairLedId.K_International5,
            //[DeviceKeys.International4] = CorsairLedId.K_International4,
            [DeviceKeys.FN_Key] = CorsairLedId.K_Fn,
            [DeviceKeys.LOCK_SWITCH] = CorsairLedId.K_WinLock,
            [DeviceKeys.BRIGHTNESS_SWITCH] = CorsairLedId.K_Brightness,
            [DeviceKeys.ADDITIONALLIGHT1] = CorsairLedId.KLP_Zone1,
            [DeviceKeys.ADDITIONALLIGHT2] = CorsairLedId.KLP_Zone2,
            [DeviceKeys.ADDITIONALLIGHT3] = CorsairLedId.KLP_Zone3,
            [DeviceKeys.ADDITIONALLIGHT4] = CorsairLedId.KLP_Zone4,
            [DeviceKeys.ADDITIONALLIGHT5] = CorsairLedId.KLP_Zone5,
            [DeviceKeys.ADDITIONALLIGHT6] = CorsairLedId.KLP_Zone6,
            [DeviceKeys.ADDITIONALLIGHT7] = CorsairLedId.KLP_Zone7,
            [DeviceKeys.ADDITIONALLIGHT8] = CorsairLedId.KLP_Zone8,
            [DeviceKeys.ADDITIONALLIGHT9] = CorsairLedId.KLP_Zone9,
            [DeviceKeys.ADDITIONALLIGHT10] = CorsairLedId.KLP_Zone10,
            [DeviceKeys.ADDITIONALLIGHT11] = CorsairLedId.KLP_Zone11,
            [DeviceKeys.ADDITIONALLIGHT12] = CorsairLedId.KLP_Zone12,
            [DeviceKeys.ADDITIONALLIGHT13] = CorsairLedId.KLP_Zone13,
            [DeviceKeys.ADDITIONALLIGHT14] = CorsairLedId.KLP_Zone14,
            [DeviceKeys.ADDITIONALLIGHT15] = CorsairLedId.KLP_Zone15,
            [DeviceKeys.ADDITIONALLIGHT16] = CorsairLedId.KLP_Zone16,
            [DeviceKeys.ADDITIONALLIGHT17] = CorsairLedId.KLP_Zone17,
            [DeviceKeys.ADDITIONALLIGHT18] = CorsairLedId.KLP_Zone18,
            [DeviceKeys.ADDITIONALLIGHT19] = CorsairLedId.KLP_Zone19
        };

        internal static readonly Dictionary<DeviceKeys, CorsairLedId> MouseMatLedMap = new Dictionary<DeviceKeys, CorsairLedId>()
        {
            [DeviceKeys.MOUSEPADLIGHT1] = CorsairLedId.MM_Zone1,
            [DeviceKeys.MOUSEPADLIGHT2] = CorsairLedId.MM_Zone2,
            [DeviceKeys.MOUSEPADLIGHT3] = CorsairLedId.MM_Zone3,
            [DeviceKeys.MOUSEPADLIGHT4] = CorsairLedId.MM_Zone4,
            [DeviceKeys.MOUSEPADLIGHT5] = CorsairLedId.MM_Zone5,
            [DeviceKeys.MOUSEPADLIGHT6] = CorsairLedId.MM_Zone6,
            [DeviceKeys.MOUSEPADLIGHT7] = CorsairLedId.MM_Zone7,
            [DeviceKeys.MOUSEPADLIGHT8] = CorsairLedId.MM_Zone8,
            [DeviceKeys.MOUSEPADLIGHT9] = CorsairLedId.MM_Zone9,
            [DeviceKeys.MOUSEPADLIGHT10] = CorsairLedId.MM_Zone10,
            [DeviceKeys.MOUSEPADLIGHT11] = CorsairLedId.MM_Zone11,
            [DeviceKeys.MOUSEPADLIGHT12] = CorsairLedId.MM_Zone12,
            [DeviceKeys.MOUSEPADLIGHT13] = CorsairLedId.MM_Zone13,
            [DeviceKeys.MOUSEPADLIGHT14] = CorsairLedId.MM_Zone14,
            [DeviceKeys.MOUSEPADLIGHT15] = CorsairLedId.MM_Zone15
        };

        internal static readonly Dictionary<DeviceKeys, CorsairLedId> HeadsetStandLedMap = new Dictionary<DeviceKeys, CorsairLedId>()
        {
            [DeviceKeys.Peripheral_Logo] = CorsairLedId.HSS_Zone1,
            [DeviceKeys.MOUSEPADLIGHT15] = CorsairLedId.HSS_Zone2,
            [DeviceKeys.MOUSEPADLIGHT13] = CorsairLedId.HSS_Zone3,
            [DeviceKeys.MOUSEPADLIGHT11] = CorsairLedId.HSS_Zone4,
            [DeviceKeys.MOUSEPADLIGHT9] = CorsairLedId.HSS_Zone5,
            [DeviceKeys.MOUSEPADLIGHT7] = CorsairLedId.HSS_Zone6,
            [DeviceKeys.MOUSEPADLIGHT5] = CorsairLedId.HSS_Zone7,
            [DeviceKeys.MOUSEPADLIGHT3] = CorsairLedId.HSS_Zone8,
            [DeviceKeys.MOUSEPADLIGHT1] = CorsairLedId.HSS_Zone9,
        };

        internal static readonly Dictionary<DeviceKeys, CorsairLedId> MouseLedMap = new Dictionary<DeviceKeys, CorsairLedId>()
        {
            [DeviceKeys.Peripheral_Logo] = CorsairLedId.M_1,
            [DeviceKeys.Peripheral_FrontLight] = CorsairLedId.M_2,
            [DeviceKeys.Peripheral_ScrollWheel] = CorsairLedId.M_3,
            [DeviceKeys.ADDITIONALLIGHT1] = CorsairLedId.M_4,//TODO
            [DeviceKeys.ADDITIONALLIGHT2] = CorsairLedId.M_5,
            [DeviceKeys.ADDITIONALLIGHT3] = CorsairLedId.M_6
        };

        internal static readonly Dictionary<CorsairDeviceType, Dictionary<DeviceKeys, CorsairLedId>> MapsMap = new Dictionary<CorsairDeviceType, Dictionary<DeviceKeys, CorsairLedId>>()
        {
            [CorsairDeviceType.Keyboard] = KeyboardLedMap,
            [CorsairDeviceType.Mouse] = MouseLedMap,
            [CorsairDeviceType.MouseMat] = MouseMatLedMap,
            [CorsairDeviceType.HeadsetStand] = HeadsetStandLedMap,
        };

        internal static readonly List<CorsairLedId> Channel1LedIds = EnumUtils.GetEnumValues<CorsairLedId>()
            .Where(id => Enum.GetName(typeof(CorsairLedId), id).StartsWith("D_C1_"))
            .ToList();

        internal static readonly List<CorsairLedId> Channel2LedIds = EnumUtils.GetEnumValues<CorsairLedId>()
            .Where(id => Enum.GetName(typeof(CorsairLedId), id).StartsWith("D_C2_"))
            .ToList();

        internal static readonly List<CorsairLedId> Channel3LedIds = EnumUtils.GetEnumValues<CorsairLedId>()
            .Where(id => Enum.GetName(typeof(CorsairLedId), id).StartsWith("D_C3_"))
            .ToList();

        internal static readonly List<List<CorsairLedId>> ChannelLeds = new List<List<CorsairLedId>> 
        {
            Channel1LedIds,
            Channel2LedIds,
            Channel3LedIds
        };

        public static string ToString(this CorsairLedColor corsairLedColor) => $"{corsairLedColor.LedId}, ({corsairLedColor.R},{corsairLedColor.G},{corsairLedColor.B})";
    }
}
