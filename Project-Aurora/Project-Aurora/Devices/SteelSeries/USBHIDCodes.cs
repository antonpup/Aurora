// USBHIDCodes.cs by brainbug89 is licensed under CC BY-NC-SA 4.0

//reference: http://www.freebsddiary.org/APC/usb_hid_usages.php

using System;

public enum USBHIDCodes
{
    //NONE = 0x00,
    ERROR = 0x03,

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
    HASHTAG = 0x32,         // Keyboard Non-US # and ~
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
    KEYBOARD_DELETE = 0x4C,
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

    BACKSLASH_UK = 0x64,    // Keyboard Non-US \ and |
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
    JPN_HENKAN = 0x8a,      // Keyboard International4
    JPN_MUHENKAN = 0x8b,    // Keyboard International5
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
};
