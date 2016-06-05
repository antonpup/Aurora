using Aurora.EffectsEngine;
using Corale.Colore;
using Corale.Colore.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Aurora.Devices.SteelSeries
{
    class SteelSeriesDevice : Device
    {
        private String devicename = "Steeleries";
        private bool isInitialized = false;

        private bool keyboard_updated = false;
        private bool peripheral_updated = false;

        IKeyboard keyboard = null;
        IMouse mouse = null;
        IHeadset headset = null;
        IMousepad mousepad = null;

        private System.Drawing.Color previous_peripheral_Color = System.Drawing.Color.Black;

        public string GetDeviceName()
        {
            return devicename;
        }

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                return devicename + ": " + (keyboard != null ? "Keyboard Connected " : "") + (mouse != null ? "Mouse Connected " : "") + (headset != null ? "Headset Connected " : "") + (mousepad != null ? "Mousepad Connected " : "");
            }
            else
            {
                return devicename + ": Not initialized";
            }
        }

        public bool Initialize()
        {
            try
            {
                if (!Chroma.IsSdkAvailable())
                {
                    Global.logger.LogLine("No Chroma SDK available", Logging_Level.Info);
                    throw new Exception("No Chroma SDK available");
                    //return false;
                }

                Chroma.Instance.Initialize();

                Global.logger.LogLine("Razer device, Initialized", Logging_Level.Info);

                keyboard = Chroma.Instance.Keyboard;
                mouse = Chroma.Instance.Mouse;
                headset = Chroma.Instance.Headset;
                mousepad = Chroma.Instance.Mousepad;

                if (keyboard == null &&
                    mouse == null &&
                    headset == null &&
                    mousepad == null
                    )
                {
                    throw new Exception("No devices connected");
                }
                else
                {
                    if (Global.Configuration.razer_first_time)
                    {
                        RazerInstallInstructions instructions = new RazerInstallInstructions();
                        instructions.ShowDialog();

                        Global.Configuration.razer_first_time = false;
                        Settings.ConfigManager.Save(Global.Configuration);
                    }

                    isInitialized = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Global.logger.LogLine("Razer device, Exception! Message:" + ex, Logging_Level.Error);
            }


            isInitialized = false;
            return false;
        }

        public void Shutdown()
        {
        }

        public void Reset()
        {
            if (this.IsInitialized() && (keyboard_updated || peripheral_updated))
            {
                keyboard_updated = false;
                peripheral_updated = false;
            }
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public bool IsInitialized()
        {
            return isInitialized;
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, System.Drawing.Color> keyColors, bool forced = false)
        {
            Key keyindex = Key.Invalid;

            try
            {
                foreach (KeyValuePair<DeviceKeys, System.Drawing.Color> key in keyColors)
                {
                    Key localKey = ToRazer(key.Key);

                    if (localKey == Key.Invalid && key.Key == DeviceKeys.Peripheral)
                    {
                        SendColorToPeripheral(key.Value, forced);
                    }
                    else if (localKey != Key.Invalid)
                    {
                        SetOneKey(localKey, key.Value);
                    }

                    keyindex = localKey;
                }

                SendColorsToKeyboard(forced);
                return true;
            }
            catch (Exception e)
            {
                Global.logger.LogLine("Corsair device, error when updating device. Error: " + e, Logging_Level.Error);
                Console.WriteLine(e);
                return false;
            }
        }

        private void SendColorsToKeyboard(bool forced = false)
        {
            if (keyboard != null)
            {
                keyboard_updated = true;
            }
        }

        private void SetOneKey(Key localKey, System.Drawing.Color color)
        {
            if (keyboard != null && keyboard[localKey] != null)
                keyboard.SetKey(localKey, color);
        }

        private void SendColorToPeripheral(System.Drawing.Color color, bool forced = false)
        {
            if ((!previous_peripheral_Color.Equals(color) || forced) && mouse != null)
            {
                if (Global.Configuration.allow_peripheral_devices)
                {
                    mouse.SetAll(color);

                    previous_peripheral_Color = color;
                    peripheral_updated = true;
                }
                else
                {
                    if (peripheral_updated)
                    {
                        peripheral_updated = false;
                    }
                }
            }
        }

        private Key ToRazer(DeviceKeys key)
        {
            switch (key)
            {
                case (DeviceKeys.ESC):
                    return Key.Escape;
                case (DeviceKeys.F1):
                    return Key.F1;
                case (DeviceKeys.F2):
                    return Key.F2;
                case (DeviceKeys.F3):
                    return Key.F3;
                case (DeviceKeys.F4):
                    return Key.F4;
                case (DeviceKeys.F5):
                    return Key.F5;
                case (DeviceKeys.F6):
                    return Key.F6;
                case (DeviceKeys.F7):
                    return Key.F7;
                case (DeviceKeys.F8):
                    return Key.F8;
                case (DeviceKeys.F9):
                    return Key.F9;
                case (DeviceKeys.F10):
                    return Key.F10;
                case (DeviceKeys.F11):
                    return Key.F11;
                case (DeviceKeys.F12):
                    return Key.F12;
                case (DeviceKeys.PRINT_SCREEN):
                    return Key.PrintScreen;
                case (DeviceKeys.SCROLL_LOCK):
                    return Key.Scroll;
                case (DeviceKeys.PAUSE_BREAK):
                    return Key.Pause;
                case (DeviceKeys.TILDE):
                    return Key.OemTilde;
                case (DeviceKeys.ONE):
                    return Key.D1;
                case (DeviceKeys.TWO):
                    return Key.D2;
                case (DeviceKeys.THREE):
                    return Key.D3;
                case (DeviceKeys.FOUR):
                    return Key.D4;
                case (DeviceKeys.FIVE):
                    return Key.D5;
                case (DeviceKeys.SIX):
                    return Key.D6;
                case (DeviceKeys.SEVEN):
                    return Key.D7;
                case (DeviceKeys.EIGHT):
                    return Key.D8;
                case (DeviceKeys.NINE):
                    return Key.D9;
                case (DeviceKeys.ZERO):
                    return Key.D0;
                case (DeviceKeys.MINUS):
                    return Key.OemMinus;
                case (DeviceKeys.EQUALS):
                    return Key.OemEquals;
                case (DeviceKeys.BACKSPACE):
                    return Key.Backspace;
                case (DeviceKeys.INSERT):
                    return Key.Insert;
                case (DeviceKeys.HOME):
                    return Key.Home;
                case (DeviceKeys.PAGE_UP):
                    return Key.PageUp;
                case (DeviceKeys.NUM_LOCK):
                    return Key.NumLock;
                case (DeviceKeys.NUM_SLASH):
                    return Key.NumDivide;
                case (DeviceKeys.NUM_ASTERISK):
                    return Key.NumMultiply;
                case (DeviceKeys.NUM_MINUS):
                    return Key.NumSubtract;
                case (DeviceKeys.TAB):
                    return Key.Tab;
                case (DeviceKeys.Q):
                    return Key.Q;
                case (DeviceKeys.W):
                    return Key.W;
                case (DeviceKeys.E):
                    return Key.E;
                case (DeviceKeys.R):
                    return Key.R;
                case (DeviceKeys.T):
                    return Key.T;
                case (DeviceKeys.Y):
                    return Key.Y;
                case (DeviceKeys.U):
                    return Key.U;
                case (DeviceKeys.I):
                    return Key.I;
                case (DeviceKeys.O):
                    return Key.O;
                case (DeviceKeys.P):
                    return Key.P;
                case (DeviceKeys.OPEN_BRACKET):
                    return Key.OemLeftBracket;
                case (DeviceKeys.CLOSE_BRACKET):
                    return Key.OemRightBracket;
                case (DeviceKeys.BACKSLASH):
                    return Key.OemBackslash;
                case (DeviceKeys.DELETE):
                    return Key.Delete;
                case (DeviceKeys.END):
                    return Key.End;
                case (DeviceKeys.PAGE_DOWN):
                    return Key.PageDown;
                case (DeviceKeys.NUM_SEVEN):
                    return Key.Num7;
                case (DeviceKeys.NUM_EIGHT):
                    return Key.Num8;
                case (DeviceKeys.NUM_NINE):
                    return Key.Num9;
                case (DeviceKeys.NUM_PLUS):
                    return Key.NumAdd;
                case (DeviceKeys.CAPS_LOCK):
                    return Key.CapsLock;
                case (DeviceKeys.A):
                    return Key.A;
                case (DeviceKeys.S):
                    return Key.S;
                case (DeviceKeys.D):
                    return Key.D;
                case (DeviceKeys.F):
                    return Key.F;
                case (DeviceKeys.G):
                    return Key.G;
                case (DeviceKeys.H):
                    return Key.H;
                case (DeviceKeys.J):
                    return Key.J;
                case (DeviceKeys.K):
                    return Key.K;
                case (DeviceKeys.L):
                    return Key.L;
                case (DeviceKeys.SEMICOLON):
                    return Key.OemSemicolon;
                case (DeviceKeys.APOSTROPHE):
                    return Key.OemApostrophe;
                case (DeviceKeys.HASHTAG):
                    return Key.EurPound;
                case (DeviceKeys.ENTER):
                    return Key.Enter;
                case (DeviceKeys.NUM_FOUR):
                    return Key.Num4;
                case (DeviceKeys.NUM_FIVE):
                    return Key.Num5;
                case (DeviceKeys.NUM_SIX):
                    return Key.Num6;
                case (DeviceKeys.LEFT_SHIFT):
                    return Key.LeftShift;
                case (DeviceKeys.BACKSLASH_UK):
                    return Key.EurBackslash;
                case (DeviceKeys.Z):
                    return Key.Z;
                case (DeviceKeys.X):
                    return Key.X;
                case (DeviceKeys.C):
                    return Key.C;
                case (DeviceKeys.V):
                    return Key.V;
                case (DeviceKeys.B):
                    return Key.B;
                case (DeviceKeys.N):
                    return Key.N;
                case (DeviceKeys.M):
                    return Key.M;
                case (DeviceKeys.COMMA):
                    return Key.OemComma;
                case (DeviceKeys.PERIOD):
                    return Key.OemPeriod;
                case (DeviceKeys.FORWARD_SLASH):
                    return Key.OemSlash;
                case (DeviceKeys.RIGHT_SHIFT):
                    return Key.RightShift;
                case (DeviceKeys.ARROW_UP):
                    return Key.Up;
                case (DeviceKeys.NUM_ONE):
                    return Key.Num1;
                case (DeviceKeys.NUM_TWO):
                    return Key.Num2;
                case (DeviceKeys.NUM_THREE):
                    return Key.Num3;
                case (DeviceKeys.NUM_ENTER):
                    return Key.NumEnter;
                case (DeviceKeys.LEFT_CONTROL):
                    return Key.LeftControl;
                case (DeviceKeys.LEFT_WINDOWS):
                    return Key.LeftWindows;
                case (DeviceKeys.LEFT_ALT):
                    return Key.LeftAlt;
                case (DeviceKeys.SPACE):
                    return Key.Space;
                case (DeviceKeys.RIGHT_ALT):
                    return Key.RightAlt;
                //case (DeviceKeys.RIGHT_WINDOWS):
                //    return Key.Right;
                case (DeviceKeys.APPLICATION_SELECT):
                    return Key.RightMenu;
                case (DeviceKeys.RIGHT_CONTROL):
                    return Key.RightControl;
                case (DeviceKeys.ARROW_LEFT):
                    return Key.Left;
                case (DeviceKeys.ARROW_DOWN):
                    return Key.Down;
                case (DeviceKeys.ARROW_RIGHT):
                    return Key.Right;
                case (DeviceKeys.NUM_ZERO):
                    return Key.Num0;
                case (DeviceKeys.NUM_PERIOD):
                    return Key.NumDecimal;

                case (DeviceKeys.FN_Key):
                    return Key.Function;

                case (DeviceKeys.G1):
                    return Key.Macro1;
                case (DeviceKeys.G2):
                    return Key.Macro2;
                case (DeviceKeys.G3):
                    return Key.Macro3;
                case (DeviceKeys.G4):
                    return Key.Macro4;
                case (DeviceKeys.G5):
                    return Key.Macro5;

                default:
                    return Key.Invalid;
            }
        }

        public bool IsKeyboardConnected()
        {
            return keyboard != null;
        }

        public bool IsPeripheralConnected()
        {
            return (mouse != null || headset != null || mousepad != null);
        }
    }
}
