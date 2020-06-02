using System.Collections.Generic;
using System.Drawing;
using AuraServiceLib;

namespace Aurora.Devices.Asus
{
    /// <summary>
    /// The Claymore doesn't work with <see cref="IAuraSyncKeyboard"/> so we have to settle for a generic device with a mapper
    /// </summary>
    public class AsusSyncClaymoreDevice : AuraSyncDevice
    {
        /// <inheritdoc />
        public AsusSyncClaymoreDevice(AsusHandler asusHandler, IAuraSyncDevice device, int frameRate = 30) : base(
            asusHandler, device, frameRate) { }

        protected override void ApplyColors(Dictionary<DeviceKeys, Color> colors)
        {
            if (Global.Configuration.devices_disable_keyboard)
                return;
            
            for (int i = 0; i < Device.Lights.Count; i++)
            {
                var light = DeviceKeyToClaymore(i);
                
                if (colors.ContainsKey(light))
                    SetRgbLight(Device.Lights[i], colors[light]);
            }
        }

        private static DeviceKeys DeviceKeyToClaymore(int keyId)
        {
            switch (keyId)
            {
                case 0:
                    return DeviceKeys.ESC;
                case 2:
                    return DeviceKeys.F1;
                case 3:
                    return DeviceKeys.F2;
                case 4:
                    return DeviceKeys.F3;
                case 5:
                    return DeviceKeys.F4;
                case 7:
                    return DeviceKeys.F5;
                case 8:
                    return DeviceKeys.F6;
                case 9:
                    return DeviceKeys.F7;
                case 10:
                    return DeviceKeys.F8;
                case 12:
                    return DeviceKeys.F9;
                case 13:
                    return DeviceKeys.F10;
                case 14:
                    return DeviceKeys.F11;
                case 15:
                    return DeviceKeys.F12;
                case 16:
                    return DeviceKeys.PRINT_SCREEN;
                case 17:
                    return DeviceKeys.SCROLL_LOCK;
                case 18:
                    return DeviceKeys.PAUSE_BREAK;
                case 23:
                    return DeviceKeys.TILDE;
                case 24:
                    return DeviceKeys.ONE;
                case 25:
                    return DeviceKeys.TWO;
                case 26:
                    return DeviceKeys.THREE;
                case 27:
                    return DeviceKeys.FOUR;
                case 28:
                    return DeviceKeys.FIVE;
                case 29:
                    return DeviceKeys.SIX;
                case 30:
                    return DeviceKeys.SEVEN;
                case 31:
                    return DeviceKeys.EIGHT;
                case 32:
                    return DeviceKeys.NINE;
                case 33:
                    return DeviceKeys.ZERO;
                case 34:
                    return DeviceKeys.MINUS;
                case 35:
                    return DeviceKeys.EQUALS;
                case 36:
                    return DeviceKeys.BACKSPACE;
                case 39:
                    return DeviceKeys.INSERT;
                case 40:
                    return DeviceKeys.HOME;
                case 41:
                    return DeviceKeys.PAGE_UP;
                case 42:
                    return DeviceKeys.NUM_LOCK;
                case 43:
                    return DeviceKeys.NUM_SLASH;
                case 44:
                    return DeviceKeys.NUM_ASTERISK;
                case 45:
                    return DeviceKeys.NUM_MINUS;
                case 46:
                    return DeviceKeys.TAB;
                case 47:
                    return DeviceKeys.Q;
                case 48:
                    return DeviceKeys.W;
                case 49:
                    return DeviceKeys.E;
                case 50:
                    return DeviceKeys.R;
                case 51:
                    return DeviceKeys.T;
                case 52:
                    return DeviceKeys.Y;
                case 53:
                    return DeviceKeys.U;
                case 54:
                    return DeviceKeys.I;
                case 55:
                    return DeviceKeys.O;
                case 56:
                    return DeviceKeys.P;
                case 57:
                    return DeviceKeys.OPEN_BRACKET;
                case 58:
                    return DeviceKeys.CLOSE_BRACKET;
                case 62:
                    return DeviceKeys.DELETE;
                case 63:
                    return DeviceKeys.END;
                case 64:
                    return DeviceKeys.PAGE_DOWN;
                case 65:
                    return DeviceKeys.NUM_SEVEN;
                case 66:
                    return DeviceKeys.NUM_EIGHT;
                case 67:
                    return DeviceKeys.NUM_NINE;
                case 68:
                    return DeviceKeys.NUM_PLUS;
                case 69:
                    return DeviceKeys.CAPS_LOCK;
                case 70:
                    return DeviceKeys.A;
                case 71:
                    return DeviceKeys.S;
                case 72:
                    return DeviceKeys.D;
                case 73:
                    return DeviceKeys.F;
                case 74:
                    return DeviceKeys.G;
                case 75:
                    return DeviceKeys.H;
                case 76:
                    return DeviceKeys.J;
                case 77:
                    return DeviceKeys.K;
                case 78:
                    return DeviceKeys.L;
                case 79:
                    return DeviceKeys.SEMICOLON;
                case 80:
                    return DeviceKeys.APOSTROPHE;
                case 81:
                    return DeviceKeys.HASHTAG;
                case 82:
                    return DeviceKeys.ENTER;
                case 83:
                    return DeviceKeys.NONE;
                case 88:
                    return DeviceKeys.NUM_FIVE;
                case 89:
                    return DeviceKeys.NUM_FIVE;
                case 90:
                    return DeviceKeys.NUM_SIX;
                case 92:
                    return DeviceKeys.LEFT_SHIFT;
                case 93:
                    return DeviceKeys.BACKSLASH_UK;
                case 94:
                    return DeviceKeys.Z;
                case 95:
                    return DeviceKeys.X;
                case 96:
                    return DeviceKeys.C;
                case 97:
                    return DeviceKeys.V;
                case 98:
                    return DeviceKeys.B;
                case 99:
                    return DeviceKeys.N;
                case 100:
                    return DeviceKeys.M;
                case 101:
                    return DeviceKeys.COMMA;
                case 102:
                    return DeviceKeys.PERIOD;
                case 103:
                    return DeviceKeys.FORWARD_SLASH;
                case 104:
                    return DeviceKeys.NONE;
                case 105:
                    return DeviceKeys.RIGHT_SHIFT;
                case 109:
                    return DeviceKeys.ARROW_UP;
                case 111:
                    return DeviceKeys.NUM_ONE;
                case 112:
                    return DeviceKeys.NUM_TWO;
                case 113:
                    return DeviceKeys.NUM_THREE;
                case 114:
                    return DeviceKeys.NUM_ENTER;
                case 115:
                    return DeviceKeys.LEFT_CONTROL;
                case 116:
                    return DeviceKeys.LEFT_WINDOWS;
                case 117:
                    return DeviceKeys.LEFT_ALT;
                case 119:
                    return DeviceKeys.SPACE;
                case 123:
                    return DeviceKeys.LOGO;
                case 124:
                    return DeviceKeys.RIGHT_ALT;
                case 125:
                    return DeviceKeys.RIGHT_WINDOWS;
                case 126:
                    return DeviceKeys.APPLICATION_SELECT;
                case 127:
                    return DeviceKeys.NONE;
                case 128:
                    return DeviceKeys.RIGHT_CONTROL;
                case 131:
                    return DeviceKeys.ARROW_LEFT;
                case 132:
                    return DeviceKeys.ARROW_DOWN;
                case 133:
                    return DeviceKeys.ARROW_RIGHT;
                case 134:
                    return DeviceKeys.NUM_ZERO;
                case 135:
                    return DeviceKeys.NUM_PERIOD;
            }

            return DeviceKeys.NONE;
        }
    }
}