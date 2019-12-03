using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Devices.SteelSeries
{
    public partial class SteelSeriesDevice
    {
        public bool TryGetHid(DeviceKeys key, out byte hid)
        {
            hid = 0;
            switch (key)
            {
                case DeviceKeys.ESC:
                    hid = (byte) HidCodes.ESC;
                    break;
                case DeviceKeys.F1:
                    hid = (byte) HidCodes.F1;
                    break;
                case DeviceKeys.F2:
                    hid = (byte) HidCodes.F2;
                    break;
                case DeviceKeys.F3:
                    hid = (byte) HidCodes.F3;
                    break;
                case DeviceKeys.F4:
                    hid = (byte) HidCodes.F4;
                    break;
                case DeviceKeys.F5:
                    hid = (byte) HidCodes.F5;
                    break;
                case DeviceKeys.F6:
                    hid = (byte) HidCodes.F6;
                    break;
                case DeviceKeys.F7:
                    hid = (byte) HidCodes.F7;
                    break;
                case DeviceKeys.F8:
                    hid = (byte) HidCodes.F8;
                    break;
                case DeviceKeys.F9:
                    hid = (byte) HidCodes.F9;
                    break;
                case DeviceKeys.F10:
                    hid = (byte) HidCodes.F10;
                    break;
                case DeviceKeys.F11:
                    hid = (byte) HidCodes.F11;
                    break;
                case DeviceKeys.F12:
                    hid = (byte) HidCodes.F12;
                    break;
                case DeviceKeys.PRINT_SCREEN:
                    hid = (byte) HidCodes.PRINT_SCREEN;
                    break;
                case DeviceKeys.SCROLL_LOCK:
                    hid = (byte) HidCodes.SCROLL_LOCK;
                    break;
                case DeviceKeys.PAUSE_BREAK:
                    hid = (byte) HidCodes.PAUSE_BREAK;
                    break;
                case DeviceKeys.JPN_HALFFULLWIDTH:
                case DeviceKeys.OEM5:
                case DeviceKeys.TILDE:
                    hid = (byte) HidCodes.TILDE;
                    break;
                case DeviceKeys.ONE:
                    hid = (byte) HidCodes.ONE;
                    break;
                case DeviceKeys.TWO:
                    hid = (byte) HidCodes.TWO;
                    break;
                case DeviceKeys.THREE:
                    hid = (byte) HidCodes.THREE;
                    break;
                case DeviceKeys.FOUR:
                    hid = (byte) HidCodes.FOUR;
                    break;
                case DeviceKeys.FIVE:
                    hid = (byte) HidCodes.FIVE;
                    break;
                case DeviceKeys.SIX:
                    hid = (byte) HidCodes.SIX;
                    break;
                case DeviceKeys.SEVEN:
                    hid = (byte) HidCodes.SEVEN;
                    break;
                case DeviceKeys.EIGHT:
                    hid = (byte) HidCodes.EIGHT;
                    break;
                case DeviceKeys.NINE:
                    hid = (byte) HidCodes.NINE;
                    break;
                case DeviceKeys.ZERO:
                    hid = (byte) HidCodes.ZERO;
                    break;
                case DeviceKeys.MINUS:
                    hid = (byte) HidCodes.MINUS;
                    break;
                case DeviceKeys.EQUALS:
                    hid = (byte) HidCodes.EQUALS;
                    break;
                case DeviceKeys.BACKSPACE:
                    hid = (byte) HidCodes.BACKSPACE;
                    break;
                case DeviceKeys.INSERT:
                    hid = (byte) HidCodes.INSERT;
                    break;
                case DeviceKeys.HOME:
                    hid = (byte) HidCodes.HOME;
                    break;
                case DeviceKeys.PAGE_UP:
                    hid = (byte) HidCodes.PAGE_UP;
                    break;
                case DeviceKeys.NUM_LOCK:
                    hid = (byte) HidCodes.NUM_LOCK;
                    break;
                case DeviceKeys.NUM_SLASH:
                    hid = (byte) HidCodes.NUM_SLASH;
                    break;
                case DeviceKeys.NUM_ASTERISK:
                    hid = (byte) HidCodes.NUM_ASTERISK;
                    break;
                case DeviceKeys.NUM_MINUS:
                    hid = (byte) HidCodes.NUM_MINUS;
                    break;
                case DeviceKeys.TAB:
                    hid = (byte) HidCodes.TAB;
                    break;
                case DeviceKeys.Q:
                    hid = (byte) HidCodes.Q;
                    break;
                case DeviceKeys.W:
                    hid = (byte) HidCodes.W;
                    break;
                case DeviceKeys.E:
                    hid = (byte) HidCodes.E;
                    break;
                case DeviceKeys.R:
                    hid = (byte) HidCodes.R;
                    break;
                case DeviceKeys.T:
                    hid = (byte) HidCodes.T;
                    break;
                case DeviceKeys.Y:
                    hid = (byte) HidCodes.Y;
                    break;
                case DeviceKeys.U:
                    hid = (byte) HidCodes.U;
                    break;
                case DeviceKeys.I:
                    hid = (byte) HidCodes.I;
                    break;
                case DeviceKeys.O:
                    hid = (byte) HidCodes.O;
                    break;
                case DeviceKeys.P:
                    hid = (byte) HidCodes.P;
                    break;
                case DeviceKeys.OPEN_BRACKET:
                    hid = (byte) HidCodes.OPEN_BRACKET;
                    break;
                case DeviceKeys.CLOSE_BRACKET:
                    hid = (byte) HidCodes.CLOSE_BRACKET;
                    break;
                case DeviceKeys.BACKSLASH:
                    hid = (byte) HidCodes.BACKSLASH;
                    break;
                case DeviceKeys.DELETE:
                    hid = (byte) HidCodes.DELETE;
                    break;
                case DeviceKeys.END:
                    hid = (byte) HidCodes.END;
                    break;
                case DeviceKeys.PAGE_DOWN:
                    hid = (byte) HidCodes.PAGE_DOWN;
                    break;
                case DeviceKeys.NUM_SEVEN:
                    hid = (byte) HidCodes.NUM_SEVEN;
                    break;
                case DeviceKeys.NUM_EIGHT:
                    hid = (byte) HidCodes.NUM_EIGHT;
                    break;
                case DeviceKeys.NUM_NINE:
                    hid = (byte) HidCodes.NUM_NINE;
                    break;
                case DeviceKeys.NUM_PLUS:
                    hid = (byte) HidCodes.NUM_PLUS;
                    break;
                case DeviceKeys.CAPS_LOCK:
                    hid = (byte) HidCodes.CAPS_LOCK;
                    break;
                case DeviceKeys.A:
                    hid = (byte) HidCodes.A;
                    break;
                case DeviceKeys.S:
                    hid = (byte) HidCodes.S;
                    break;
                case DeviceKeys.D:
                    hid = (byte) HidCodes.D;
                    break;
                case DeviceKeys.F:
                    hid = (byte) HidCodes.F;
                    break;
                case DeviceKeys.G:
                    hid = (byte) HidCodes.G;
                    break;
                case DeviceKeys.H:
                    hid = (byte) HidCodes.H;
                    break;
                case DeviceKeys.J:
                    hid = (byte) HidCodes.J;
                    break;
                case DeviceKeys.K:
                    hid = (byte) HidCodes.K;
                    break;
                case DeviceKeys.L:
                    hid = (byte) HidCodes.L;
                    break;
                case DeviceKeys.SEMICOLON:
                    hid = (byte) HidCodes.SEMICOLON;
                    break;
                case DeviceKeys.APOSTROPHE:
                    hid = (byte) HidCodes.APOSTROPHE;
                    break;
                case DeviceKeys.HASHTAG:
                    hid = (byte) HidCodes.HASHTAG;
                    break;
                case DeviceKeys.ENTER:
                    hid = (byte) HidCodes.ENTER;
                    break;
                case DeviceKeys.NUM_FOUR:
                    hid = (byte) HidCodes.NUM_FOUR;
                    break;
                case DeviceKeys.NUM_FIVE:
                    hid = (byte) HidCodes.NUM_FIVE;
                    break;
                case DeviceKeys.NUM_SIX:
                    hid = (byte) HidCodes.NUM_SIX;
                    break;
                case DeviceKeys.LEFT_SHIFT:
                    hid = (byte) HidCodes.LEFT_SHIFT;
                    break;
                case DeviceKeys.BACKSLASH_UK:
                    hid = (byte) HidCodes.BACKSLASH_UK;
                    break;
                case DeviceKeys.Z:
                    hid = (byte) HidCodes.Z;
                    break;
                case DeviceKeys.X:
                    hid = (byte) HidCodes.X;
                    break;
                case DeviceKeys.C:
                    hid = (byte) HidCodes.C;
                    break;
                case DeviceKeys.V:
                    hid = (byte) HidCodes.V;
                    break;
                case DeviceKeys.B:
                    hid = (byte) HidCodes.B;
                    break;
                case DeviceKeys.N:
                    hid = (byte) HidCodes.N;
                    break;
                case DeviceKeys.M:
                    hid = (byte) HidCodes.M;
                    break;
                case DeviceKeys.COMMA:
                    hid = (byte) HidCodes.COMMA;
                    break;
                case DeviceKeys.PERIOD:
                    hid = (byte) HidCodes.PERIOD;
                    break;
                case DeviceKeys.FORWARD_SLASH:
                case DeviceKeys.OEM8:
                    hid = (byte) HidCodes.FORWARD_SLASH;
                    break;
                case DeviceKeys.RIGHT_SHIFT:
                    hid = (byte) HidCodes.RIGHT_SHIFT;
                    break;
                case DeviceKeys.ARROW_UP:
                    hid = (byte) HidCodes.ARROW_UP;
                    break;
                case DeviceKeys.NUM_ONE:
                    hid = (byte) HidCodes.NUM_ONE;
                    break;
                case DeviceKeys.NUM_TWO:
                    hid = (byte) HidCodes.NUM_TWO;
                    break;
                case DeviceKeys.NUM_THREE:
                    hid = (byte) HidCodes.NUM_THREE;
                    break;
                case DeviceKeys.NUM_ENTER:
                    hid = (byte) HidCodes.NUM_ENTER;
                    break;
                case DeviceKeys.LEFT_CONTROL:
                    hid = (byte) HidCodes.LEFT_CONTROL;
                    break;
                case DeviceKeys.LEFT_WINDOWS:
                    hid = (byte) HidCodes.LEFT_WINDOWS;
                    break;
                case DeviceKeys.LEFT_ALT:
                    hid = (byte) HidCodes.LEFT_ALT;
                    break;
                case DeviceKeys.JPN_MUHENKAN:
                    hid = (byte) HidCodes.JPN_MUHENKAN;
                    break;
                case DeviceKeys.SPACE:
                    hid = (byte) HidCodes.SPACE;
                    break;
                case DeviceKeys.JPN_HENKAN:
                    hid = (byte) HidCodes.JPN_HENKAN;
                    break;
                case DeviceKeys.JPN_HIRAGANA_KATAKANA:
                    hid = (byte) HidCodes.JPN_HIRAGANA_KATAKANA;
                    break;
                case DeviceKeys.RIGHT_ALT:
                    hid = (byte) HidCodes.RIGHT_ALT;
                    break;
                case DeviceKeys.RIGHT_WINDOWS:
                    hid = (byte) HidCodes.RIGHT_WINDOWS;
                    break;
                case DeviceKeys.APPLICATION_SELECT:
                    hid = (byte) HidCodes.APPLICATION_SELECT;
                    break;
                case DeviceKeys.RIGHT_CONTROL:
                    hid = (byte) HidCodes.RIGHT_CONTROL;
                    break;
                case DeviceKeys.ARROW_LEFT:
                    hid = (byte) HidCodes.ARROW_LEFT;
                    break;
                case DeviceKeys.ARROW_DOWN:
                    hid = (byte) HidCodes.ARROW_DOWN;
                    break;
                case DeviceKeys.ARROW_RIGHT:
                    hid = (byte) HidCodes.ARROW_RIGHT;
                    break;
                case DeviceKeys.NUM_ZERO:
                    hid = (byte) HidCodes.NUM_ZERO;
                    break;
                case DeviceKeys.NUM_PERIOD:
                    hid = (byte) HidCodes.NUM_PERIOD;
                    break;
                case DeviceKeys.FN_Key:
                    hid = (byte) HidCodes.SS_KEY;
                    break;
                case DeviceKeys.G0:
                    hid = (byte) HidCodes.G0;
                    break;
                case DeviceKeys.G1:
                    hid = (byte) HidCodes.G1;
                    break;
                case DeviceKeys.G2:
                    hid = (byte) HidCodes.G2;
                    break;
                case DeviceKeys.G3:
                    hid = (byte) HidCodes.G3;
                    break;
                case DeviceKeys.G4:
                    hid = (byte) HidCodes.G4;
                    break;
                case DeviceKeys.G5:
                    hid = (byte) HidCodes.G5;
                    break;
                case DeviceKeys.LOGO:
                    hid = (byte) HidCodes.LOGO;
                    break;
                default:
                    return false;
            }
            return true;
        }

        public enum HidCodes
        {
            //SteelSeries Specific
            LOGO = 0x00,
            SS_KEY = 0xEF,
            G0 = 0xE8,
            G1 = 0xE9,
            G2 = 0xEA,
            G3 = 0xEB,
            G4 = 0xEC,
            G5 = 0xED,

            //USB HID Codes List
            A = 0x04,
            B = 0x05,
            C = 0x06,
            D = 0x07,
            E = 0x08,
            F = 0x09,
            G = 0x0A,
            H = 0x0B,
            I = 0x0C,
            J = 0x0D,
            K = 0x0E,
            L = 0x0F,
            M = 0x10,
            N = 0x11,
            O = 0x12,
            P = 0x13,
            Q = 0x14,
            R = 0x15,
            S = 0x16,
            T = 0x17,
            U = 0x18,
            V = 0x19,
            W = 0x1A,
            X = 0x1B,
            Y = 0x1C,
            Z = 0x1D,

            ONE = 0x1E,
            TWO = 0x1F,
            THREE = 0x20,
            FOUR = 0x21,
            FIVE = 0x22,
            SIX = 0x23,
            SEVEN = 0x24,
            EIGHT = 0x25,
            NINE = 0x26,
            ZERO = 0x27,

            ENTER = 0x28,
            ESC = 0x29,
            BACKSPACE = 0x2A,
            TAB = 0x2B,
            SPACE = 0x2C,
            MINUS = 0x2D,
            EQUALS = 0x2E,
            OPEN_BRACKET = 0x2F,
            CLOSE_BRACKET = 0x30,
            BACKSLASH = 0x31,
            HASHTAG = 0x32, // Keyboard Non-US # and ~
            SEMICOLON = 0x33,
            APOSTROPHE = 0x34,
            TILDE = 0x35,
            COMMA = 0x36,
            PERIOD = 0x37,
            FORWARD_SLASH = 0x38,
            CAPS_LOCK = 0x39,

            F1 = 0x3A,
            F2 = 0x3B,
            F3 = 0x3C,
            F4 = 0x3D,
            F5 = 0x3E,
            F6 = 0x3F,
            F7 = 0x40,
            F8 = 0x41,
            F9 = 0x42,
            F10 = 0x43,
            F11 = 0x44,
            F12 = 0x45,

            PRINT_SCREEN = 0x46,
            SCROLL_LOCK = 0x47,
            PAUSE_BREAK = 0x48,
            INSERT = 0x49,
            HOME = 0x4A,
            PAGE_UP = 0x4B,
            DELETE = 0x4C,
            END = 0x4D,
            PAGE_DOWN = 0x4E,

            ARROW_RIGHT = 0x4F,
            ARROW_LEFT = 0x50,
            ARROW_DOWN = 0x51,
            ARROW_UP = 0x52,

            NUM_LOCK = 0x53,
            NUM_SLASH = 0x54,
            NUM_ASTERISK = 0x55,
            NUM_MINUS = 0x56,
            NUM_PLUS = 0x57,
            NUM_ENTER = 0x58,
            NUM_ONE = 0x59,
            NUM_TWO = 0x5A,
            NUM_THREE = 0x5B,
            NUM_FOUR = 0x5C,
            NUM_FIVE = 0x5D,
            NUM_SIX = 0x5E,
            NUM_SEVEN = 0x5F,
            NUM_EIGHT = 0x60,
            NUM_NINE = 0x61,
            NUM_ZERO = 0x62,
            NUM_PERIOD = 0x63,

            BACKSLASH_UK = 0x64, // Keyboard Non-US \ and |
            APPLICATION_SELECT = 0x65,

            // skip unused special keys from 0x66 to 0xA4
            //0x66	Keyboard Power
            //0x67	Keypad =
            //0x68  Keyboard F13
            //0x69	Keyboard F14
            //0x6A	Keyboard F15
            //0x6B	Keyboard F16
            //0x6C	Keyboard F17
            //0x6D	Keyboard F18
            //0x6E	Keyboard F19
            //0x6F	Keyboard F20
            //0x70	Keyboard F21
            //0x71	Keyboard F22
            //0x72	Keyboard F23
            //0x73	Keyboard F24
            //0x74	Keyboard Execute
            //0x75	Keyboard Help
            //0x76	Keyboard Menu
            //0x77	Keyboard Select
            //0x78	Keyboard Stop
            //0x79	Keyboard Again
            //0x7A	Keyboard Undo
            //0x7B	Keyboard Cut
            //0x7C	Keyboard Copy
            //0x7D	Keyboard Paste
            //0x7E	Keyboard Find
            //0x7F	Keyboard Mute
            //0x80	Keyboard Volume Up
            //0x81	Keyboard Volume Down
            //0x82	Keyboard Locking Caps Lock
            //0x83	Keyboard Locking Num Lock
            //0x84	Keyboard Locking Scroll Lock
            //0x85	Keypad Comma
            //0x86	Keypad Equal Sign
            //0x87	Keyboard International1
            JPN_HIRAGANA_KATAKANA = 0x88, // Keyboard International2
            //0x89	Keyboard International3
            JPN_HENKAN = 0x8a,   // Keyboard International4
            JPN_MUHENKAN = 0x8b, // Keyboard International5
            //0x8C	Keyboard International6
            //0x8D	Keyboard International7
            //0x8E	Keyboard International8
            //0x8F	Keyboard International9
            //0x90	Keyboard LANG1
            //0x91	Keyboard LANG2
            //0x92	Keyboard LANG3
            //0x93	Keyboard LANG4
            //0x94	Keyboard LANG5
            //0x95	Keyboard LANG6
            //0x96	Keyboard LANG7
            //0x97	Keyboard LANG8
            //0x98	Keyboard LANG9
            //0x99	Keyboard Alternate Erase
            //0x9A	Keyboard SysReq/Attention
            //0x9B	Keyboard Cancel
            //0x9C	Keyboard Clear
            //0x9D	Keyboard Prior
            //0x9E	Keyboard Return
            //0x9F	Keyboard Separator
            //0xA0	Keyboard Out
            //0xA1	Keyboard Oper
            //0xA2	Keyboard Clear/Again
            //0xA3	Keyboard CrSel/Props
            //0xA4	Keyboard ExSel

            LEFT_CONTROL = 0xE0,
            LEFT_SHIFT = 0xE1,
            LEFT_ALT = 0xE2,
            LEFT_WINDOWS = 0xE3,
            RIGHT_CONTROL = 0xE4,
            RIGHT_SHIFT = 0xE5,
            RIGHT_ALT = 0xE6,
            RIGHT_WINDOWS = 0xE7,

            //OEM102 = 384,  
        }
    }
}