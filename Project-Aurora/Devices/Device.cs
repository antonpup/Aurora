using Aurora.EffectsEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Aurora.Devices
{
    public enum DeviceKeys
    {
        [Description("Peripheral Device")]
        Peripheral,
        [Description("Escape")]
        ESC,
        [Description("F1")]
        F1,
        [Description("F2")]
        F2,
        [Description("F3")]
        F3,
        [Description("F4")]
        F4,
        [Description("F5")]
        F5,
        [Description("F6")]
        F6,
        [Description("F7")]
        F7,
        [Description("F8")]
        F8,
        [Description("F9")]
        F9,
        [Description("F10")]
        F10,
        [Description("F11")]
        F11,
        [Description("F12")]
        F12,
        [Description("Print Screen")]
        PRINT_SCREEN,
        [Description("Scroll Lock")]
        SCROLL_LOCK,
        [Description("Pause")]
        PAUSE_BREAK,

        [Description("Tilde")]
        TILDE,
        [Description("1")]
        ONE,
        [Description("2")]
        TWO,
        [Description("3")]
        THREE,
        [Description("4")]
        FOUR,
        [Description("5")]
        FIVE,
        [Description("6")]
        SIX,
        [Description("7")]
        SEVEN,
        [Description("8")]
        EIGHT,
        [Description("9")]
        NINE,
        [Description("0")]
        ZERO,
        [Description("-")]
        MINUS,
        [Description("=")]
        EQUALS,
        [Description("Backspace")]
        BACKSPACE,
        [Description("Insert")]
        INSERT,
        [Description("Home")]
        HOME,
        [Description("Page Up")]
        PAGE_UP,
        [Description("Numpad Lock")]
        NUM_LOCK,
        [Description("Numpad /")]
        NUM_SLASH,
        [Description("Numpad *")]
        NUM_ASTERISK,
        [Description("Numpad -")]
        NUM_MINUS,

        [Description("Tab")]
        TAB,
        [Description("Q")]
        Q,
        [Description("W")]
        W,
        [Description("E")]
        E,
        [Description("R")]
        R,
        [Description("T")]
        T,
        [Description("Y")]
        Y,
        [Description("U")]
        U,
        [Description("I")]
        I,
        [Description("O")]
        O,
        [Description("P")]
        P,
        [Description("{")]
        OPEN_BRACKET,
        [Description("}")]
        CLOSE_BRACKET,
        [Description("\\")]
        BACKSLASH,
        [Description("Delete")]
        DELETE,
        [Description("End")]
        END,
        [Description("Page Down")]
        PAGE_DOWN,
        [Description("Numpad 7")]
        NUM_SEVEN,
        [Description("Numpad 8")]
        NUM_EIGHT,
        [Description("Numpad 9")]
        NUM_NINE,
        [Description("Numpad +")]
        NUM_PLUS,

        [Description("Caps Lock")]
        CAPS_LOCK,
        [Description("A")]
        A,
        [Description("S")]
        S,
        [Description("D")]
        D,
        [Description("F")]
        F,
        [Description("G")]
        G,
        [Description("H")]
        H,
        [Description("J")]
        J,
        [Description("K")]
        K,
        [Description("L")]
        L,
        [Description("Semicolon")]
        SEMICOLON,
        [Description("Apostrophe")]
        APOSTROPHE,
        [Description("#")]
        HASHTAG,
        [Description("Enter")]
        ENTER,
        [Description("Numpad 4")]
        NUM_FOUR,
        [Description("Numpad 5")]
        NUM_FIVE,
        [Description("Numpad 6")]
        NUM_SIX,

        [Description("Left Shift")]
        LEFT_SHIFT,
        [Description("Non-US Backslash")]
        BACKSLASH_UK,
        [Description("Z")]
        Z,
        [Description("X")]
        X,
        [Description("C")]
        C,
        [Description("V")]
        V,
        [Description("B")]
        B,
        [Description("N")]
        N,
        [Description("M")]
        M,
        [Description("Comma")]
        COMMA,
        [Description("Period")]
        PERIOD,
        [Description("Forward Slash")]
        FORWARD_SLASH,
        [Description("Right Shift")]
        RIGHT_SHIFT,
        [Description("Arrow Up")]
        ARROW_UP,
        [Description("Numpad 1")]
        NUM_ONE,
        [Description("Numpad 2")]
        NUM_TWO,
        [Description("Numpad 3")]
        NUM_THREE,
        [Description("Numpad Enter")]
        NUM_ENTER,

        [Description("Left Control")]
        LEFT_CONTROL,
        [Description("Left Windows Key")]
        LEFT_WINDOWS,
        [Description("Left Alt")]
        LEFT_ALT,
        [Description("Spacebar")]
        SPACE,
        [Description("Right Alt")]
        RIGHT_ALT,
        [Description("Right Windows Key")]
        RIGHT_WINDOWS,
        [Description("Application Select Key")]
        APPLICATION_SELECT,
        [Description("Right Control")]
        RIGHT_CONTROL,
        [Description("Arrow Left")]
        ARROW_LEFT,
        [Description("Arrow Down")]
        ARROW_DOWN,
        [Description("Arrow Right")]
        ARROW_RIGHT,
        [Description("Numpad 0")]
        NUM_ZERO,
        [Description("Numpad Period")]
        NUM_PERIOD,

        [Description("FN Key")]
        FN_Key,

        [Description("G1")]
        G1,
        [Description("G2")]
        G2,
        [Description("G3")]
        G3,
        [Description("G4")]
        G4,
        [Description("G5")]
        G5,
        [Description("G6")]
        G6,
        [Description("G7")]
        G7,
        [Description("G8")]
        G8,
        [Description("G9")]
        G9,
        [Description("G10")]
        G10,
        [Description("G11")]
        G11,
        [Description("G12")]
        G12,
        [Description("G13")]
        G13,
        [Description("G14")]
        G14,
        [Description("G15")]
        G15,
        [Description("G16")]
        G16,
        [Description("G17")]
        G17,
        [Description("G18")]
        G18,
        [Description("G19")]
        G19,
        [Description("G20")]
        G20,

        [Description("Brand Logo")]
        LOGO,
        [Description("Brand Logo #2")]
        LOGO2,
        [Description("Brand Logo #3")]
        LOGO3,
        [Description("Brightness Switch")]
        BRIGHTNESS_SWITCH,
        [Description("Lock Switch")]
        LOCK_SWITCH,

        [Description("Media Play/Pause")]
        MEDIA_PLAY_PAUSE,
        [Description("Media Play")]
        MEDIA_PLAY,
        [Description("Media Pause")]
        MEDIA_PAUSE,
        [Description("Media Stop")]
        MEDIA_STOP,
        [Description("Media Previous")]
        MEDIA_PREVIOUS,
        [Description("Media Next")]
        MEDIA_NEXT,

        [Description("Volume Mute")]
        VOLUME_MUTE,
        [Description("Volume Down")]
        VOLUME_DOWN,
        [Description("Volume Up")]
        VOLUME_UP,

        [Description("Additional Light 1")]
        ADDITIONALLIGHT1,
        [Description("Additional Light 2")]
        ADDITIONALLIGHT2,
        [Description("Additional Light 3")]
        ADDITIONALLIGHT3,
        [Description("Additional Light 4")]
        ADDITIONALLIGHT4,
        [Description("Additional Light 5")]
        ADDITIONALLIGHT5,
        [Description("Additional Light 6")]
        ADDITIONALLIGHT6,
        [Description("Additional Light 7")]
        ADDITIONALLIGHT7,
        [Description("Additional Light 8")]
        ADDITIONALLIGHT8,
        [Description("Additional Light 9")]
        ADDITIONALLIGHT9,
        [Description("Additional Light 10")]
        ADDITIONALLIGHT10,

        [Description("None")]
        NONE,
    };

    public interface Device
    {
        String GetDeviceName();

        String GetDeviceDetails();

        bool Initialize();

        void Shutdown();

        void Reset();

        bool Reconnect();

        bool IsInitialized();

        bool IsConnected();

        bool IsKeyboardConnected();

        bool IsPeripheralConnected();

        bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced = false);
    }
}
