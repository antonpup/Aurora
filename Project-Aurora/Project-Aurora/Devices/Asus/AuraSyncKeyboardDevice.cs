using System;
using System.Collections.Generic;
using System.Drawing;
using AuraServiceLib;

namespace Aurora.Devices.Asus
{
    public class AuraSyncKeyboardDevice : AuraSyncDevice
    {
        private readonly IAuraSyncKeyboard keyboard;
        
        private readonly Dictionary<ushort, IAuraRgbKey> idToKey
            = new Dictionary<ushort, IAuraRgbKey>();

        private readonly Dictionary<DeviceKeys, ushort> deviceKeyToKeyId
            = new Dictionary<DeviceKeys, ushort>();

        /// <inheritdoc />
        public AuraSyncKeyboardDevice(AsusHandler asusHandler, IAuraSyncKeyboard device, int frameRate = 30) : base(asusHandler, device, frameRate)
        {
            keyboard = device;

            foreach (IAuraRgbKey key in device.Keys)
                idToKey[key.Code] = key;

            foreach (var deviceKey in (DeviceKeys[])Enum.GetValues(typeof(DeviceKeys)))
            {
                var key = DeviceKeyToAuraKeyboardKeyId(deviceKey);
                if (key == 0)
                    continue;

                deviceKeyToKeyId[deviceKey] = key;
            }
        }

        /// <inheritdoc />
        protected override void ApplyColors(Dictionary<DeviceKeys, Color> colors)
        {
            foreach (var keyPair in colors)
            {
                if (!deviceKeyToKeyId.TryGetValue(keyPair.Key, out ushort keyId))
                    continue;
            
                if (!idToKey.TryGetValue(keyId, out var light))
                    continue;
            
                SetRgbLight(light, keyPair.Value);
            }
        }

        /// <summary>
        /// Determines the ushort ID from a DeviceKeys
        /// </summary>
        /// <param name="key">The key to translate</param>
        /// <returns>the ushort id, or ushort.MaxValue if invalid</returns>
        private ushort DeviceKeyToAuraKeyboardKeyId(DeviceKeys key)
        {
            switch (key)
            {
                case DeviceKeys.ESC:
                    return 0x0001;
                case DeviceKeys.F1:
                    return 0x003B;
                case DeviceKeys.F2:
                    return 0x003C;
                case DeviceKeys.F3:
                    return 0x003D;
                case DeviceKeys.F4:
                    return 0x003E;
                case DeviceKeys.F5:
                    return 0x003F;
                case DeviceKeys.F6:
                    return 0x0040;
                case DeviceKeys.F7:
                    return 0x0041;
                case DeviceKeys.F8:
                    return 0x0042;
                case DeviceKeys.F9:
                    return 0x0043;
                case DeviceKeys.F10:
                    return 0x0044;
                case DeviceKeys.F11:
                    return 0x0057;
                case DeviceKeys.F12:
                    return 0x0058;
                case DeviceKeys.PRINT_SCREEN:
                    return 0x00B7;
                case DeviceKeys.SCROLL_LOCK:
                    return 0x0046;
                case DeviceKeys.PAUSE_BREAK:
                    return 0x00C5;
                case DeviceKeys.OEM5:
                    return 0x0006;
                case DeviceKeys.TILDE:
                    return 0x0029;
                case DeviceKeys.ONE:
                    return 0x0002;
                case DeviceKeys.TWO:
                    return 0x0003;
                case DeviceKeys.THREE:
                    return 0x0004;
                case DeviceKeys.FOUR:
                    return 0x0005;
                case DeviceKeys.FIVE:
                    return 0x0006;
                case DeviceKeys.SIX:
                    return 0x0007;
                case DeviceKeys.SEVEN:
                    return 0x0008;
                case DeviceKeys.EIGHT:
                    return 0x0009;
                case DeviceKeys.NINE:
                    return 0x000A;
                case DeviceKeys.ZERO:
                    return 0x000B;
                case DeviceKeys.MINUS:
                    return 0x000C;
                case DeviceKeys.EQUALS:
                    return 0x000D;
                case DeviceKeys.OEM6:
                    return 0x0007;
                case DeviceKeys.BACKSPACE:
                    return 0x000E;
                case DeviceKeys.INSERT:
                    return 0x00D2;
                case DeviceKeys.HOME:
                    return 0x00C7;
                case DeviceKeys.PAGE_UP:
                    return 0x00C9;
                case DeviceKeys.NUM_LOCK:
                    return 0x0045;
                case DeviceKeys.NUM_SLASH:
                    return 0x00B5;
                case DeviceKeys.NUM_ASTERISK:
                    return 0x0037;
                case DeviceKeys.NUM_MINUS:
                    return 0x004A;
                case DeviceKeys.TAB:
                    return 0x000F;
                case DeviceKeys.Q:
                    return 0x0010;
                case DeviceKeys.W:
                    return 0x0011;
                case DeviceKeys.E:
                    return 0x0012;
                case DeviceKeys.R:
                    return 0x0013;
                case DeviceKeys.T:
                    return 0x0014;
                case DeviceKeys.Y:
                    return 0x0015;
                case DeviceKeys.U:
                    return 0x0016;
                case DeviceKeys.I:
                    return 0x0017;
                case DeviceKeys.O:
                    return 0x0018;
                case DeviceKeys.P:
                    return 0x0019;
                case DeviceKeys.OEM1:
                    return 0x0002;
                case DeviceKeys.OPEN_BRACKET:
                    return 0x001A;
                case DeviceKeys.OEMPlus:
                    return 0x000D;
                case DeviceKeys.CLOSE_BRACKET:
                    return 0x001B;
                case DeviceKeys.BACKSLASH:
                    return 0x002B;
                case DeviceKeys.DELETE:
                    return 0x00D3;
                case DeviceKeys.END:
                    return 0x00CF;
                case DeviceKeys.PAGE_DOWN:
                    return 0x00D1;
                case DeviceKeys.NUM_SEVEN:
                    return 0x0047;
                case DeviceKeys.NUM_EIGHT:
                    return 0x0048;
                case DeviceKeys.NUM_NINE:
                    return 0x0049;
                case DeviceKeys.NUM_PLUS:
                    return 0x004E;
                case DeviceKeys.CAPS_LOCK:
                    return 0x003A;
                case DeviceKeys.A:
                    return 0x001E;
                case DeviceKeys.S:
                    return 0x001F;
                case DeviceKeys.D:
                    return 0x0020;
                case DeviceKeys.F:
                    return 0x0021;
                case DeviceKeys.G:
                    return 0x0022;
                case DeviceKeys.H:
                    return 0x0023;
                case DeviceKeys.J:
                    return 0x0024;
                case DeviceKeys.K:
                    return 0x0025;
                case DeviceKeys.L:
                    return 0x0026;
                case DeviceKeys.OEMTilde:
                    return 0x0029;
                case DeviceKeys.SEMICOLON:
                    return 0x0027;
                case DeviceKeys.APOSTROPHE:
                    return 0x0028;
                case DeviceKeys.HASHTAG:
                    return 0x0003;
                case DeviceKeys.ENTER:
                    return 0x001C;
                case DeviceKeys.NUM_FOUR:
                    return 0x004B;
                case DeviceKeys.NUM_FIVE:
                    return 0x004C;
                case DeviceKeys.NUM_SIX:
                    return 0x004D;
                case DeviceKeys.LEFT_SHIFT:
                    return 0x002A;
                case DeviceKeys.BACKSLASH_UK:
                    return 0x002B;
                case DeviceKeys.Z:
                    return 0x002C;
                case DeviceKeys.X:
                    return 0x002D;
                case DeviceKeys.C:
                    return 0x002E;
                case DeviceKeys.V:
                    return 0x002F;
                case DeviceKeys.B:
                    return 0x0030;
                case DeviceKeys.N:
                    return 0x0031;
                case DeviceKeys.M:
                    return 0x0032;
                case DeviceKeys.COMMA:
                    return 0x0033;
                case DeviceKeys.PERIOD:
                    return 0x0034;
                case DeviceKeys.FORWARD_SLASH:
                    return 0x0035;
                case DeviceKeys.OEM8:
                    return 0x0009;
                case DeviceKeys.RIGHT_SHIFT:
                    return 0x0036;
                case DeviceKeys.ARROW_UP:
                    return 0x00C8;
                case DeviceKeys.NUM_ONE:
                    return 0x004F;
                case DeviceKeys.NUM_TWO:
                    return 0x0050;
                case DeviceKeys.NUM_THREE:
                    return 0x0051;
                case DeviceKeys.NUM_ENTER:
                    return 0x009C;
                case DeviceKeys.LEFT_CONTROL:
                    return 0x001D;
                case DeviceKeys.LEFT_WINDOWS:
                    return 0x00DB;
                case DeviceKeys.LEFT_ALT:
                    return 0x0038;
                case DeviceKeys.SPACE:
                    return 0x0039;
                case DeviceKeys.RIGHT_ALT:
                    return 0x00B8;
                case DeviceKeys.APPLICATION_SELECT:
                    return 0x00DD;
                case DeviceKeys.RIGHT_CONTROL:
                    return 0x009D;
                case DeviceKeys.ARROW_LEFT:
                    return 0x00CB;
                case DeviceKeys.ARROW_DOWN:
                    return 0x00D0;
                case DeviceKeys.ARROW_RIGHT:
                    return 0x00CD;
                case DeviceKeys.NUM_ZERO:
                    return 0x0052;
                case DeviceKeys.NUM_PERIOD:
                    return 0x0053;
                case DeviceKeys.FN_Key:
                    return 0x0100;
                case DeviceKeys.LOGO:
                    return 0x0101;
                case DeviceKeys.ADDITIONALLIGHT1:
                    // LEFT OF STRIX FLARE KEYBOARD
                    return 0x0102;
                case DeviceKeys.ADDITIONALLIGHT2:
                    //RIGHT OF STRIX FLARE KEYBOARD
                    return 0x0103;
                default:
                    return 0x0000;
            }
        }
    }
}