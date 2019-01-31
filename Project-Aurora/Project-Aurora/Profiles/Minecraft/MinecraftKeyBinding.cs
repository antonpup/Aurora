using Aurora.Devices;
using Aurora.Devices.Layout.Layouts;
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
        public KeyboardKeys DeviceKey => JavaKeyboardCodeToDeviceKey(keyCode);

        /// <summary>
        /// Gets the keys for this binding, including modifiers.
        /// </summary>
        public KeyboardKeys[] AffectedKeys {
            get {
                HashSet<KeyboardKeys> keys = new HashSet<KeyboardKeys>();
                keys.Add(DeviceKey);
                if (modifier == "SHIFT") {
                    keys.Add(KeyboardKeys.LEFT_SHIFT); keys.Add(KeyboardKeys.RIGHT_SHIFT);
                } else if (modifier == "CONTROL") {
                    keys.Add(KeyboardKeys.LEFT_CONTROL); keys.Add(KeyboardKeys.RIGHT_CONTROL);
                } else if (modifier == "ALT") {
                    keys.Add(KeyboardKeys.LEFT_ALT); keys.Add(KeyboardKeys.RIGHT_ALT);
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
        private static KeyboardKeys JavaKeyboardCodeToDeviceKey(int keyCode) {
            switch (keyCode) {
                case 0x00: return KeyboardKeys.NONE;
                case 0x01: return KeyboardKeys.ESC;
                case 0x02: return KeyboardKeys.ONE;
                case 0x03: return KeyboardKeys.TWO;
                case 0x04: return KeyboardKeys.THREE;
                case 0x05: return KeyboardKeys.FOUR;
                case 0x06: return KeyboardKeys.FIVE;
                case 0x07: return KeyboardKeys.SIX;
                case 0x08: return KeyboardKeys.SEVEN;
                case 0x09: return KeyboardKeys.EIGHT;
                case 0x0A: return KeyboardKeys.NINE;
                case 0x0B: return KeyboardKeys.ZERO;
                case 0x0C: return KeyboardKeys.MINUS;
                case 0x0D: return KeyboardKeys.EQUALS;
                case 0x0E: return KeyboardKeys.BACKSPACE;
                case 0x0F: return KeyboardKeys.TAB;
                case 0x10: return KeyboardKeys.Q;
                case 0x11: return KeyboardKeys.W;
                case 0x12: return KeyboardKeys.E;
                case 0x13: return KeyboardKeys.R;
                case 0x14: return KeyboardKeys.T;
                case 0x15: return KeyboardKeys.Y;
                case 0x16: return KeyboardKeys.U;
                case 0x17: return KeyboardKeys.I;
                case 0x18: return KeyboardKeys.O;
                case 0x19: return KeyboardKeys.P;
                case 0x1A: return KeyboardKeys.OPEN_BRACKET;
                case 0x1B: return KeyboardKeys.CLOSE_BRACKET;
                case 0x1C: return KeyboardKeys.ENTER;
                case 0x1D: return KeyboardKeys.LEFT_CONTROL;
                case 0x1E: return KeyboardKeys.A;
                case 0x1F: return KeyboardKeys.S;
                case 0x20: return KeyboardKeys.D;
                case 0x21: return KeyboardKeys.F;
                case 0x22: return KeyboardKeys.G;
                case 0x23: return KeyboardKeys.H;
                case 0x24: return KeyboardKeys.J;
                case 0x25: return KeyboardKeys.K;
                case 0x26: return KeyboardKeys.L;
                case 0x27: return KeyboardKeys.SEMICOLON;
                case 0x28: return KeyboardKeys.APOSTROPHE;
                case 0x29: return KeyboardKeys.TILDE;
                case 0x2A: return KeyboardKeys.LEFT_SHIFT;
                case 0x2B: return KeyboardKeys.BACKSLASH;
                case 0x2C: return KeyboardKeys.Z;
                case 0x2D: return KeyboardKeys.X;
                case 0x2E: return KeyboardKeys.C;
                case 0x2F: return KeyboardKeys.V;
                case 0x30: return KeyboardKeys.B;
                case 0x31: return KeyboardKeys.N;
                case 0x32: return KeyboardKeys.M;
                case 0x33: return KeyboardKeys.COMMA;
                case 0x34: return KeyboardKeys.PERIOD;
                case 0x35: return KeyboardKeys.FORWARD_SLASH;
                case 0x36: return KeyboardKeys.RIGHT_SHIFT;
                case 0x37: return KeyboardKeys.NUM_ASTERISK;
                case 0x38: return KeyboardKeys.LEFT_ALT;
                case 0x39: return KeyboardKeys.SPACE;
                case 0x3A: return KeyboardKeys.CAPS_LOCK;
                case 0x3B: return KeyboardKeys.F1;
                case 0x3C: return KeyboardKeys.F2;
                case 0x3D: return KeyboardKeys.F3;
                case 0x3E: return KeyboardKeys.F4;
                case 0x3F: return KeyboardKeys.F5;
                case 0x40: return KeyboardKeys.F6;
                case 0x41: return KeyboardKeys.F7;
                case 0x42: return KeyboardKeys.F8;
                case 0x43: return KeyboardKeys.F9;
                case 0x44: return KeyboardKeys.F10;
                case 0x45: return KeyboardKeys.NUM_LOCK;
                case 0x46: return KeyboardKeys.SCROLL_LOCK;
                case 0x47: return KeyboardKeys.NUM_SEVEN;
                case 0x48: return KeyboardKeys.NUM_EIGHT;
                case 0x49: return KeyboardKeys.NUM_NINE;
                case 0x4A: return KeyboardKeys.NUM_MINUS;
                case 0x4B: return KeyboardKeys.NUM_FOUR;
                case 0x4C: return KeyboardKeys.NUM_FIVE;
                case 0x4D: return KeyboardKeys.NUM_SIX;
                case 0x4E: return KeyboardKeys.NUM_PLUS;
                case 0x4F: return KeyboardKeys.NUM_ONE;
                case 0x50: return KeyboardKeys.NUM_TWO;
                case 0x51: return KeyboardKeys.NUM_THREE;
                case 0x52: return KeyboardKeys.NUM_ZERO;
                case 0x53: return KeyboardKeys.NUM_PERIOD;
                case 0x57: return KeyboardKeys.F11;
                case 0x58: return KeyboardKeys.F12;
                case 0x9C: return KeyboardKeys.NUM_ENTER;
                case 0x9D: return KeyboardKeys.RIGHT_CONTROL;
                case 0xB5: return KeyboardKeys.NUM_SLASH;
                case 0xB8: return KeyboardKeys.RIGHT_ALT;
                case 0xC5: return KeyboardKeys.PAUSE_BREAK;
                case 0xC7: return KeyboardKeys.HOME;
                case 0xC8: return KeyboardKeys.ARROW_UP;
                case 0xC9: return KeyboardKeys.PAGE_UP;
                case 0xCB: return KeyboardKeys.ARROW_LEFT;
                case 0xCD: return KeyboardKeys.ARROW_RIGHT;
                case 0xCF: return KeyboardKeys.END;
                case 0xD0: return KeyboardKeys.ARROW_DOWN;
                case 0xD1: return KeyboardKeys.PAGE_DOWN;
                case 0xD2: return KeyboardKeys.INSERT;
                case 0xD3: return KeyboardKeys.DELETE;
                case 0xDB: return KeyboardKeys.LEFT_WINDOWS;
                default: return KeyboardKeys.NONE;
            }
        }
    }
}
