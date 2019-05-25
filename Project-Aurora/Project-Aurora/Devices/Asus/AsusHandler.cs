using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AuraServiceLib;

namespace Aurora.Devices.Asus
{
    /// <summary>
    /// The main point of entry for the Asus SDK
    /// </summary>
    public class AsusHandler
    {
        private bool _initializing = false;
        private IAuraSdk2 _sdk;

        private List<AsusSdkDeviceWrapper> _devices = new List<AsusSdkDeviceWrapper>();

        #region Windows Handlers
        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_CLOSE = 0xF060;
   
        #endregion

        /// <summary>
        /// Get control of all aura devices
        /// </summary>
        /// <returns>True if succeeded</returns>
        public void GetControl(Action<bool> OnComplete)
        {
            if (_initializing)
                return;

            // Do this async because it may take a while to initialise
            Task.Run(() =>
            {
                try
                {
                    _initializing = true;
                    _sdk = (IAuraSdk2)new AuraSdk();
                    // run this async to close any pop up windows that this brings up
                    Task.Run(() => ClosePopUpWindows());
                    // get the devices (this can be slow
                    _sdk.SwitchMode();

                    // enumerate all devices
                    IAuraSyncDeviceCollection devices = _sdk.Enumerate(0);
                    foreach (IAuraSyncDevice device in devices)
                    {
                        Console.WriteLine(device.Name);
                        Console.WriteLine((AsusDeviceType)device.Type);

                        switch ((AsusDeviceType)device.Type)
                        {
                            case AsusDeviceType.Keyboard:
                                _devices.Add(new AuraSdkKeyboardWrapper(device));
                                break;
                        }
                    }

                    OnComplete?.Invoke(true);
                    _initializing = false;
                }
                catch
                {
                    OnComplete?.Invoke(false);
                }
            });
        }

        /// <summary>
        /// Tell the aura sdk device to set the device back to it's previous state
        /// </summary>
        public void ReleaseControl()
        {
            _sdk.ReleaseControl(0);
        }

        /// <summary>
        /// Sometimes the Aura SDK picks up devices that do not have a RGB and spits out
        /// an error popup. This is suppress these annoying notifications
        /// </summary>
        private void ClosePopUpWindows()
        {
            while (_initializing)
            {
                var window = FindWindow(null, "Message");
                SendMessage(window, WM_SYSCOMMAND, SC_CLOSE, 0);
            }
        }

        public void UpdateDevices(Dictionary<DeviceKeys, Color> keyColors)
        {
            foreach (var device in _devices)
            {
                device.ApplyColors(keyColors);
            }
        }

        private enum AsusDeviceType : int
        {
            All = 0x00000000,
            Motherboard = 0x00010000,
            MotherboardLedStrip = 0x00011000,
            AllInOnePc = 0x00012000,
            Vga = 0x00020000,
            Display = 0x00030000,
            Headset = 0x00040000,
            Microphone = 0x00050000,
            ExternalHdd = 0x00060000,
            ExternalBdDrive = 0x00061000,
            Dram = 0x00070000,
            Keyboard = 0x00080000,
            NotebookKeyboard = 0x00081000,
            NotebookKeyboard4ZoneType = 0x00081001,
            Mouse = 0x00090000,
            Chassis = 0x000B0000,
            Projector = 0x000C0000,
        }
        
        private abstract class AsusSdkDeviceWrapper
        {
            /// <summary>
            /// Apply Colors to the device
            /// </summary>
            /// <param name="keyColors"></param>
            public abstract void ApplyColors(Dictionary<DeviceKeys, Color> keyColors);

            /// <summary>
            /// Sets an Aura RGB Light 
            /// </summary>
            /// <param name="rgbLight">The light to set</param>
            /// <param name="color">Color to set with</param>
            protected static void SetRgbLight(IAuraRgbKey rgbLight, Color color)
            {
                rgbLight.Red = color.R;
                rgbLight.Green = color.G;
                rgbLight.Blue = color.B;
            }
        }

//        /// <summary>
//        /// A wrapper for Asus Mice
//        /// </summary>
//        private class AuraSdkMouseWrapper
//        {
//            public AuraSdkKeyboardWrapper(IAuraSyncDevice mouse)
//            {
//                _mouse = mouse;
//
//                foreach (IAuraRgbKey key in _keyboard.Keys)
//                    _idToKey[key.Code] = key;
//            }
//        }

        /// <summary>
        /// A wrapper for Asus Keyboards
        /// </summary>
        private class AuraSdkKeyboardWrapper : AsusSdkDeviceWrapper
        {
            private readonly IAuraSyncKeyboard _keyboard;

            private readonly Dictionary<ushort, IAuraRgbKey> _idToKey
                = new Dictionary<ushort, IAuraRgbKey>();

            public AuraSdkKeyboardWrapper(IAuraSyncDevice keyboard)
            {
                _keyboard = (IAuraSyncKeyboard) keyboard;

                foreach (IAuraRgbKey key in _keyboard.Keys)
                    _idToKey[key.Code] = key;
            }
            
            /// <inheritdoc />
            public override void ApplyColors(Dictionary<DeviceKeys, Color> keyColors)
            {
                foreach (var keyColor in keyColors)
                {
                    var deviceKey = keyColor.Key;
                    var color = keyColor.Value;

                    var keyId = DeviceKeyToAuraKeyboardKeyId(deviceKey);

                    // if key is invalid
                    if (keyId == ushort.MaxValue || !_idToKey.ContainsKey(keyId))
                        continue;

                    var key = _idToKey[keyId];
                    SetRgbLight(key, color);
                }
                _keyboard.Apply();
            }
            
            /// <summary>
            /// Determines the ushort ID from a DeviceKeys
            /// </summary>
            /// <param name="key">The key to translate</param>
            /// <returns>the ushort id, or ushort.MaxValue if invalid</returns>
            private static ushort DeviceKeyToAuraKeyboardKeyId(DeviceKeys key)
            {
                switch (key)
                {
                    case DeviceKeys.ESC:
                        return 1;
                    case DeviceKeys.F1:
                        return 59;
                    case DeviceKeys.F2:
                        return 60;
                    case DeviceKeys.F3:
                        return 61;
                    case DeviceKeys.F4:
                        return 62;
                    case DeviceKeys.F5:
                        return 63;
                    case DeviceKeys.F6:
                        return 64;
                    case DeviceKeys.F7:
                        return 65;
                    case DeviceKeys.F8:
                        return 66;
                    case DeviceKeys.F9:
                        return 67;
                    case DeviceKeys.F10:
                        return 68;
                    case DeviceKeys.F11:
                        return 87;
                    case DeviceKeys.F12:
                        return 88;
                    case DeviceKeys.PRINT_SCREEN:
                        return 183;
                    case DeviceKeys.SCROLL_LOCK:
                        return 70;
                    case DeviceKeys.PAUSE_BREAK:
                        return 197;
                    case DeviceKeys.OEM5:
                        return 6;
                    case DeviceKeys.TILDE:
                        return 41;
                    case DeviceKeys.ONE:
                        return 2;
                    case DeviceKeys.TWO:
                        return 3;
                    case DeviceKeys.THREE:
                        return 4;
                    case DeviceKeys.FOUR:
                        return 5;
                    case DeviceKeys.FIVE:
                        return 6;
                    case DeviceKeys.SIX:
                        return 7;
                    case DeviceKeys.SEVEN:
                        return 8;
                    case DeviceKeys.EIGHT:
                        return 9;
                    case DeviceKeys.NINE:
                        return 10;
                    case DeviceKeys.ZERO:
                        return 11;
                    case DeviceKeys.MINUS:
                        return 12;
                    case DeviceKeys.EQUALS:
                        return 13;
                    case DeviceKeys.OEM6:
                        return 7;
                    case DeviceKeys.BACKSPACE:
                        return 14;
                    case DeviceKeys.INSERT:
                        return 210;
                    case DeviceKeys.HOME:
                        return 199;
                    case DeviceKeys.PAGE_UP:
                        return 201;
                    case DeviceKeys.NUM_LOCK:
                        return 69;
                    case DeviceKeys.NUM_SLASH:
                        return 181;
                    case DeviceKeys.NUM_ASTERISK:
                        return 55;
                    case DeviceKeys.NUM_MINUS:
                        return 74;
                    case DeviceKeys.TAB:
                        return 15;
                    case DeviceKeys.Q:
                        return 16;
                    case DeviceKeys.W:
                        return 17;
                    case DeviceKeys.E:
                        return 18;
                    case DeviceKeys.R:
                        return 19;
                    case DeviceKeys.T:
                        return 20;
                    case DeviceKeys.Y:
                        return 21;
                    case DeviceKeys.U:
                        return 22;
                    case DeviceKeys.I:
                        return 23;
                    case DeviceKeys.O:
                        return 24;
                    case DeviceKeys.P:
                        return 25;
                    case DeviceKeys.OEM1:
                        return 2;
                    case DeviceKeys.OPEN_BRACKET:
                        return 26;
                    case DeviceKeys.OEMPlus:
                        return 13;
                    case DeviceKeys.CLOSE_BRACKET:
                        return 27;
                    case DeviceKeys.BACKSLASH:
                        return 43;
                    case DeviceKeys.DELETE:
                        return 211;
                    case DeviceKeys.END:
                        return 207;
                    case DeviceKeys.PAGE_DOWN:
                        return 209;
                    case DeviceKeys.NUM_SEVEN:
                        return 71;
                    case DeviceKeys.NUM_EIGHT:
                        return 72;
                    case DeviceKeys.NUM_NINE:
                        return 73;
                    case DeviceKeys.NUM_PLUS:
                        return 78;
                    case DeviceKeys.CAPS_LOCK:
                        return 58;
                    case DeviceKeys.A:
                        return 30;
                    case DeviceKeys.S:
                        return 31;
                    case DeviceKeys.D:
                        return 32;
                    case DeviceKeys.F:
                        return 33;
                    case DeviceKeys.G:
                        return 34;
                    case DeviceKeys.H:
                        return 35;
                    case DeviceKeys.J:
                        return 36;
                    case DeviceKeys.K:
                        return 37;
                    case DeviceKeys.L:
                        return 38;
                    case DeviceKeys.OEMTilde:
                        return 41;
                    case DeviceKeys.SEMICOLON:
                        return 39;
                    case DeviceKeys.APOSTROPHE:
                        return 40;
                    case DeviceKeys.HASHTAG:
                        return 3;
                    case DeviceKeys.ENTER:
                        return 28;
                    case DeviceKeys.NUM_FOUR:
                        return 75;
                    case DeviceKeys.NUM_FIVE:
                        return 76;
                    case DeviceKeys.NUM_SIX:
                        return 77;
                    case DeviceKeys.LEFT_SHIFT:
                        return 42;
                    case DeviceKeys.BACKSLASH_UK:
                        return 43;
                    case DeviceKeys.Z:
                        return 44;
                    case DeviceKeys.X:
                        return 45;
                    case DeviceKeys.C:
                        return 46;
                    case DeviceKeys.V:
                        return 47;
                    case DeviceKeys.B:
                        return 48;
                    case DeviceKeys.N:
                        return 49;
                    case DeviceKeys.M:
                        return 50;
                    case DeviceKeys.COMMA:
                        return 51;
                    case DeviceKeys.PERIOD:
                        return 52;
                    case DeviceKeys.FORWARD_SLASH:
                        return 53;
                    case DeviceKeys.OEM8:
                        return 9;
                    case DeviceKeys.RIGHT_SHIFT:
                        return 54;
                    case DeviceKeys.ARROW_UP:
                        return 200;
                    case DeviceKeys.NUM_ONE:
                        return 79;
                    case DeviceKeys.NUM_TWO:
                        return 80;
                    case DeviceKeys.NUM_THREE:
                        return 81;
                    case DeviceKeys.NUM_ENTER:
                        return 156;
                    case DeviceKeys.LEFT_CONTROL:
                        return 29;
                    case DeviceKeys.LEFT_WINDOWS:
                        return 219;
                    case DeviceKeys.LEFT_ALT:
                        return 56;
                    case DeviceKeys.SPACE:
                        return 57;
                    case DeviceKeys.RIGHT_ALT:
                        return 184;
                    case DeviceKeys.APPLICATION_SELECT:
                        return 221;
                    case DeviceKeys.RIGHT_CONTROL:
                        return 157;
                    case DeviceKeys.ARROW_LEFT:
                        return 203;
                    case DeviceKeys.ARROW_DOWN:
                        return 208;
                    case DeviceKeys.ARROW_RIGHT:
                        return 205;
                    case DeviceKeys.NUM_ZERO:
                        return 82;
                    case DeviceKeys.NUM_PERIOD:
                        return 83;
                    case DeviceKeys.FN_Key:
                        return 256;
                    case DeviceKeys.LOGO:
                        return 257;
                    case DeviceKeys.ADDITIONALLIGHT1:
                        // LEFT OF STRIX FLARE KEYBOARD
                        return 258;
                    case DeviceKeys.ADDITIONALLIGHT2:
                        //RIGHT OF STRIX FLARE KEYBOARD
                        return 259;
                    default:
                        return ushort.MaxValue;
                }
            }

        }
    }
}
