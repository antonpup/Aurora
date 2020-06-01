using System.Collections.Generic;
using System.Drawing;
using AuraServiceLib;

namespace Aurora.Devices.Asus
{
    public class AuraSyncKeyboardDevice : AuraSyncDevice
    {
        private readonly IAuraSyncKeyboard keyboard;

        private readonly Dictionary<DeviceKeys, IAuraRgbLight> deviceKeyToKey
            = new Dictionary<DeviceKeys, IAuraRgbLight>();

        /// <inheritdoc />
        public AuraSyncKeyboardDevice(AsusHandler asusHandler, IAuraSyncKeyboard device, int frameRate = 30) : base(asusHandler, device, frameRate)
        {
            keyboard = device;

            foreach (IAuraRgbKey key in device.Keys)
                deviceKeyToKey[AsusLedIdMapper(key.Code)] = device.Key[key.Code];

            // These keys are selected by inspecting the lights 2D array
            // This is because the keycodes for a UK keyboard and American keyboard do not align for whatever reason
            // Hashtag Key
            deviceKeyToKey[DeviceKeys.HASHTAG] = keyboard.Lights[(int)(3 * keyboard.Width + 13)];
            // BackSlash Key
            deviceKeyToKey[DeviceKeys.BACKSLASH_UK] = keyboard.Lights[(int)(4 * keyboard.Width + 1)];
            
            if (Global.Configuration.keyboard_brand == Settings.PreferredKeyboard.Asus_Strix_Scope)
            {
                // Left Windows Key
                deviceKeyToKey[DeviceKeys.LEFT_WINDOWS] = keyboard.Lights[(int)(5 * keyboard.Width + 2)];
                // Left Alt Key
                deviceKeyToKey[DeviceKeys.LEFT_ALT] = keyboard.Lights[(int)(5 * keyboard.Width + 3)];
            }

        }

        /// <inheritdoc />
        protected override void ApplyColors(Dictionary<DeviceKeys, Color> colors)
        {
            if (Global.Configuration.devices_disable_keyboard)
                return;

            foreach (var keyPair in colors)
            {
                if (!deviceKeyToKey.TryGetValue(keyPair.Key, out var light))
                    continue;
            
                SetRgbLight(light, keyPair.Value);
            }
        }
        /// <summary>
        /// Determines the DeviceKeys from a ushort hashcode
        /// </summary>
        /// <param name="asusKey">The key hashcode to translate</param>
        /// <returns>the DeviceKeys, or DeviceKeys.None if invalid</returns>
        private DeviceKeys AsusLedIdMapper(ushort asusKey)
        {
            switch (asusKey)
            {
                #region KeyBinding
                case 0x01:
                    return DeviceKeys.ESC;
                case 0x3B:
                    return DeviceKeys.F1;
                case 0x3C:
                    return DeviceKeys.F2;
                case 0x3D:
                    return DeviceKeys.F3;
                case 0x3E:
                    return DeviceKeys.F4;
                case 0x3F:
                    return DeviceKeys.F5;
                case 0x40:
                    return DeviceKeys.F6;
                case 0x41:
                    return DeviceKeys.F7;
                case 0x42:
                    return DeviceKeys.F8;
                case 0x43:
                    return DeviceKeys.F9;
                case 0x44:
                    return DeviceKeys.F10;
                case 0x57:
                    return DeviceKeys.F11;
                case 0x58:
                    return DeviceKeys.F12;
                case 0x02:
                    return DeviceKeys.ONE;
                case 0x03:
                    return DeviceKeys.TWO;
                case 0x04:
                    return DeviceKeys.THREE;
                case 0x05:
                    return DeviceKeys.FOUR;
                case 0x06:
                    return DeviceKeys.FIVE;
                case 0x07:
                    return DeviceKeys.SIX;
                case 0x08:
                    return DeviceKeys.SEVEN;
                case 0x09:
                    return DeviceKeys.EIGHT;
                case 0x0A:
                    return DeviceKeys.NINE;
                case 0x0B:
                    return DeviceKeys.ZERO;
                case 0x0C:
                    return DeviceKeys.MINUS;
                case 0x0D:
                    return DeviceKeys.EQUALS;
                case 0x0E:
                    return DeviceKeys.BACKSPACE;
                case 0x0F:
                    return DeviceKeys.TAB;
                case 0x10:
                    return DeviceKeys.Q;
                case 0x11:
                    return DeviceKeys.W;
                case 0x12:
                    return DeviceKeys.E;
                case 0x13:
                    return DeviceKeys.R;
                case 0x14:
                    return DeviceKeys.T;
                case 0x15:
                    return DeviceKeys.Y;
                case 0x16:
                    return DeviceKeys.U;
                case 0x17:
                    return DeviceKeys.I;
                case 0x18:
                    return DeviceKeys.O;
                case 0x19:
                    return DeviceKeys.P;
                case 0x1A:
                    return DeviceKeys.OPEN_BRACKET;
                case 0x1B:
                    return DeviceKeys.CLOSE_BRACKET;
                case 0x1C:
                    return DeviceKeys.ENTER;
                case 0x3A:
                    return DeviceKeys.CAPS_LOCK;
                case 0x1E:
                    return DeviceKeys.A;
                case 0x1F:
                    return DeviceKeys.S;
                case 0x20:
                    return DeviceKeys.D;
                case 0x21:
                    return DeviceKeys.F;
                case 0x22:
                    return DeviceKeys.G;
                case 0x23:
                    return DeviceKeys.H;
                case 0x24:
                    return DeviceKeys.J;
                case 0x25:
                    return DeviceKeys.K;
                case 0x26:
                    return DeviceKeys.L;
                case 0x27:
                    return DeviceKeys.SEMICOLON;
                case 0x28:
                    return DeviceKeys.APOSTROPHE;
                case 0x29:
                    return DeviceKeys.TILDE;
                case 0x2A:
                    return DeviceKeys.LEFT_SHIFT;
                case 0x2B:
                    return DeviceKeys.BACKSLASH;
                case 0x2C:
                    return DeviceKeys.Z;
                case 0x2D:
                    return DeviceKeys.X;
                case 0x2E:
                    return DeviceKeys.C;
                case 0x2F:
                    return DeviceKeys.V;
                case 0x30:
                    return DeviceKeys.B;
                case 0x31:
                    return DeviceKeys.N;
                case 0x32:
                    return DeviceKeys.M;
                case 0x33:
                    return DeviceKeys.COMMA;
                case 0x34:
                    return DeviceKeys.PERIOD;
                case 0x35:
                    return DeviceKeys.FORWARD_SLASH;
                case 0x36:
                    return DeviceKeys.RIGHT_SHIFT;
                case 0x1D:
                    return DeviceKeys.LEFT_CONTROL;
                case 0xDB:
                    return DeviceKeys.LEFT_WINDOWS;
                case 0x38:
                    return DeviceKeys.LEFT_ALT;
                case 0x39:
                    return DeviceKeys.SPACE;
                case 0xB8:
                    return DeviceKeys.RIGHT_ALT;
                // Right Windows
                case 0x100:
                    return DeviceKeys.FN_Key;
                case 0xDD:
                    return DeviceKeys.APPLICATION_SELECT;
                case 0x9D:
                    return DeviceKeys.RIGHT_CONTROL;
                case 0xB7:
                    return DeviceKeys.PRINT_SCREEN;
                case 0x46:
                    return DeviceKeys.SCROLL_LOCK;
                case 0xC5:
                    return DeviceKeys.PAUSE_BREAK;
                case 0xD2:
                    return DeviceKeys.INSERT;
                case 0xC7:
                    return DeviceKeys.HOME;
                case 0xC9:
                    return DeviceKeys.PAGE_UP;
                case 0xD3:
                    return DeviceKeys.DELETE;
                case 0xCF:
                    return DeviceKeys.END;
                case 0xD1:
                    return DeviceKeys.PAGE_DOWN;
                case 0xC8:
                    return DeviceKeys.ARROW_UP;
                case 0xCB:
                    return DeviceKeys.ARROW_LEFT;
                case 0xD0:
                    return DeviceKeys.ARROW_DOWN;
                case 0xCD:
                    return DeviceKeys.ARROW_RIGHT;
                case 0x45:
                    return DeviceKeys.NUM_LOCK;
                case 0xB5:
                    return DeviceKeys.NUM_SLASH;
                case 0x37:
                    return DeviceKeys.NUM_ASTERISK;
                case 0x4A:
                    return DeviceKeys.NUM_MINUS;
                case 0x47:
                    return DeviceKeys.NUM_SEVEN;
                case 0x48:
                    return DeviceKeys.NUM_EIGHT;
                case 0x49:
                    return DeviceKeys.NUM_NINE;
                case 0x53:
                    return DeviceKeys.NUM_PERIOD;
                case 0x4E:
                    return DeviceKeys.NUM_PLUS;
                case 0x4B:
                    return DeviceKeys.NUM_FOUR;
                case 0x4C:
                    return DeviceKeys.NUM_FIVE;
                case 0x4D:
                    return DeviceKeys.NUM_SIX;
                case 0x4F:
                    return DeviceKeys.NUM_ONE;
                case 0x50:
                    return DeviceKeys.NUM_TWO;
                case 0x51:
                    return DeviceKeys.NUM_THREE;
                case 0x52:
                    return DeviceKeys.NUM_ZERO;
                case 0x9C:
                    return DeviceKeys.NUM_ENTER;
                case 0x59:
                    return DeviceKeys.BACKSLASH_UK;
                case 0x56:
                    return DeviceKeys.HASHTAG;
                case 0x101:
                    return DeviceKeys.LOGO;
                case 0x102:
                    // LEFT OF STRIX FLARE KEYBOARD
                    return DeviceKeys.ADDITIONALLIGHT1;
                case 0x103:
                    // RIGHT OF STRIX FLARE KEYBOARD
                    return DeviceKeys.ADDITIONALLIGHT2;
                default:
                    return DeviceKeys.NONE;
                    #endregion
            }
        }
    }
}