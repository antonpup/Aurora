using Aurora.Devices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Profiles.Minecraft {
    public class MinecraftKeyBinding {
        public int keyCode;
        public string modifier;
        public string context;

        /// <summary>
        /// Gets the device key from the KeyBinding's keyCode
        /// </summary>
        public DeviceKeys DeviceKey => JavaKeyboardCodeToDeviceKey(keyCode);

        /// <summary>
        /// Gets the keys for this binding, including modifiers.
        /// </summary>
        public DeviceKeys[] AffectedKeys {
            get {
                HashSet<DeviceKeys> keys = new HashSet<DeviceKeys>();
                keys.Add(DeviceKey);
                if (modifier == "SHIFT") {
                    keys.Add(DeviceKeys.LEFT_SHIFT); keys.Add(DeviceKeys.RIGHT_SHIFT);
                } else if (modifier == "CONTROL") {
                    keys.Add(DeviceKeys.LEFT_CONTROL); keys.Add(DeviceKeys.RIGHT_CONTROL);
                } else if (modifier == "ALT") {
                    keys.Add(DeviceKeys.LEFT_ALT); keys.Add(DeviceKeys.RIGHT_ALT);
                }
                return keys.ToArray();
            }
        }

        /// <summary>
        /// Method that calculates if this Keybinding conflicts with another Keybinding.
        /// </summary>
        public bool ConflictsWith(MinecraftKeyBinding other) {
            // Replicated version of MC's "conflicts" method in "KeyBinding.java".
            if (ContextConflicts(other.context) || other.ContextConflicts(context)) {
                if (ModifierMatchesKeyCode(other.keyCode) || other.ModifierMatchesKeyCode(keyCode))
                    return true;
                else if (keyCode == other.keyCode)
                    return modifier == other.modifier || (ContextConflicts("IN_GAME") && (modifier == "NONE" || other.modifier == "NONE"));
            }
            return false;
        }

        /// <summary>
        /// Method that calculates if this Keybinding conflicts with another Keybinding as a modifier-only
        /// conflict (shown in orange in Minecraft settings GUI).
        /// </summary>
        public bool ModifierConflictsWith(MinecraftKeyBinding other) {
            // Replicated version of MC's "hasKeyCodeModifierConflict" method in "KeyBinding.java".
            return (ContextConflicts(other.context) || other.ContextConflicts(context))
                && (ModifierMatchesKeyCode(other.keyCode) || other.ModifierMatchesKeyCode(keyCode));
        }

        /// <summary>
        /// Returns true if the context for this keybinding conflicts with another context.
        /// </summary>
        internal bool ContextConflicts(string other) {
            /* This is a simplified version of the default Minecraft contexts. This will
            not take into account any additional contexts added by other mods but that can't really be helped.
            As defined in MC's "KeyConflictContext.java", the "UNIVERSAL" conflict will also conflict, and the other
            two ("IN_GAME" and "GUI") will only conflict with themselves. */
            return context == "UNIVERSAL" || context == other;
        }

        /// <summary>
        /// Returns true if the modifier key for this keybinding matches the given Java keyCode.
        /// </summary>
        internal bool ModifierMatchesKeyCode(int keyCode) {
            switch (modifier) {
                case "CONTROL": return keyCode == 0x1D || keyCode == 0x9D; // 0x1D = LControl, 0x9D = RControl
                case "SHIFT": return keyCode == 0x2A || keyCode == 0x36; // 0x2A = LShift, 0x36 = RShift
                case "ALT": return keyCode == 0x38 || keyCode == 0xB8; // 0x38 = LAlt, 0xB8 = RAlt
                default: return false;
            }
        }

        /// <summary>
        /// The keyboard codes used by Minecraft do not line up with standard keyCodes or ascii keys.
        /// </summary>
        private static DeviceKeys JavaKeyboardCodeToDeviceKey(int keyCode) {
            switch (keyCode) {
                case 0x00: return DeviceKeys.NONE;
                case 0x01: return DeviceKeys.ESC;
                case 0x02: return DeviceKeys.ONE;
                case 0x03: return DeviceKeys.TWO;
                case 0x04: return DeviceKeys.THREE;
                case 0x05: return DeviceKeys.FOUR;
                case 0x06: return DeviceKeys.FIVE;
                case 0x07: return DeviceKeys.SIX;
                case 0x08: return DeviceKeys.SEVEN;
                case 0x09: return DeviceKeys.EIGHT;
                case 0x0A: return DeviceKeys.NINE;
                case 0x0B: return DeviceKeys.ZERO;
                case 0x0C: return DeviceKeys.MINUS;
                case 0x0D: return DeviceKeys.EQUALS;
                case 0x0E: return DeviceKeys.BACKSPACE;
                case 0x0F: return DeviceKeys.TAB;
                case 0x10: return DeviceKeys.Q;
                case 0x11: return DeviceKeys.W;
                case 0x12: return DeviceKeys.E;
                case 0x13: return DeviceKeys.R;
                case 0x14: return DeviceKeys.T;
                case 0x15: return DeviceKeys.Y;
                case 0x16: return DeviceKeys.U;
                case 0x17: return DeviceKeys.I;
                case 0x18: return DeviceKeys.O;
                case 0x19: return DeviceKeys.P;
                case 0x1A: return DeviceKeys.OPEN_BRACKET;
                case 0x1B: return DeviceKeys.CLOSE_BRACKET;
                case 0x1C: return DeviceKeys.ENTER;
                case 0x1D: return DeviceKeys.LEFT_CONTROL;
                case 0x1E: return DeviceKeys.A;
                case 0x1F: return DeviceKeys.S;
                case 0x20: return DeviceKeys.D;
                case 0x21: return DeviceKeys.F;
                case 0x22: return DeviceKeys.G;
                case 0x23: return DeviceKeys.H;
                case 0x24: return DeviceKeys.J;
                case 0x25: return DeviceKeys.K;
                case 0x26: return DeviceKeys.L;
                case 0x27: return DeviceKeys.SEMICOLON;
                case 0x28: return DeviceKeys.APOSTROPHE;
                case 0x29: return DeviceKeys.TILDE;
                case 0x2A: return DeviceKeys.LEFT_SHIFT;
                case 0x2B: return DeviceKeys.BACKSLASH;
                case 0x2C: return DeviceKeys.Z;
                case 0x2D: return DeviceKeys.X;
                case 0x2E: return DeviceKeys.C;
                case 0x2F: return DeviceKeys.V;
                case 0x30: return DeviceKeys.B;
                case 0x31: return DeviceKeys.N;
                case 0x32: return DeviceKeys.M;
                case 0x33: return DeviceKeys.COMMA;
                case 0x34: return DeviceKeys.PERIOD;
                case 0x35: return DeviceKeys.FORWARD_SLASH;
                case 0x36: return DeviceKeys.RIGHT_SHIFT;
                case 0x37: return DeviceKeys.NUM_ASTERISK;
                case 0x38: return DeviceKeys.LEFT_ALT;
                case 0x39: return DeviceKeys.SPACE;
                case 0x3A: return DeviceKeys.CAPS_LOCK;
                case 0x3B: return DeviceKeys.F1;
                case 0x3C: return DeviceKeys.F2;
                case 0x3D: return DeviceKeys.F3;
                case 0x3E: return DeviceKeys.F4;
                case 0x3F: return DeviceKeys.F5;
                case 0x40: return DeviceKeys.F6;
                case 0x41: return DeviceKeys.F7;
                case 0x42: return DeviceKeys.F8;
                case 0x43: return DeviceKeys.F9;
                case 0x44: return DeviceKeys.F10;
                case 0x45: return DeviceKeys.NUM_LOCK;
                case 0x46: return DeviceKeys.SCROLL_LOCK;
                case 0x47: return DeviceKeys.NUM_SEVEN;
                case 0x48: return DeviceKeys.NUM_EIGHT;
                case 0x49: return DeviceKeys.NUM_NINE;
                case 0x4A: return DeviceKeys.NUM_MINUS;
                case 0x4B: return DeviceKeys.NUM_FOUR;
                case 0x4C: return DeviceKeys.NUM_FIVE;
                case 0x4D: return DeviceKeys.NUM_SIX;
                case 0x4E: return DeviceKeys.NUM_PLUS;
                case 0x4F: return DeviceKeys.NUM_ONE;
                case 0x50: return DeviceKeys.NUM_TWO;
                case 0x51: return DeviceKeys.NUM_THREE;
                case 0x52: return DeviceKeys.NUM_ZERO;
                case 0x53: return DeviceKeys.NUM_PERIOD;
                case 0x57: return DeviceKeys.F11;
                case 0x58: return DeviceKeys.F12;
                case 0x9C: return DeviceKeys.NUM_ENTER;
                case 0x9D: return DeviceKeys.RIGHT_CONTROL;
                case 0xB5: return DeviceKeys.NUM_SLASH;
                case 0xB8: return DeviceKeys.RIGHT_ALT;
                case 0xC5: return DeviceKeys.PAUSE_BREAK;
                case 0xC7: return DeviceKeys.HOME;
                case 0xC8: return DeviceKeys.ARROW_UP;
                case 0xC9: return DeviceKeys.PAGE_UP;
                case 0xCB: return DeviceKeys.ARROW_LEFT;
                case 0xCD: return DeviceKeys.ARROW_RIGHT;
                case 0xCF: return DeviceKeys.END;
                case 0xD0: return DeviceKeys.ARROW_DOWN;
                case 0xD1: return DeviceKeys.PAGE_DOWN;
                case 0xD2: return DeviceKeys.INSERT;
                case 0xD3: return DeviceKeys.DELETE;
                case 0xDB: return DeviceKeys.LEFT_WINDOWS;
                default: return DeviceKeys.NONE;
            }
        }
    }
}
