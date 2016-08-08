using CUE.NET;
using CUE.NET.Devices.Generic.Enums;
using CUE.NET.Devices.Headset;
using CUE.NET.Devices.Headset.Enums;
using CUE.NET.Devices.Keyboard;
using CUE.NET.Devices.Keyboard.Enums;
using CUE.NET.Devices.Mouse;
using CUE.NET.Devices.Mouse.Enums;
using CUE.NET.Exceptions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Aurora.Devices.Corsair
{
    class CorsairDevice : Device
    {
        private String devicename = "Corsair";
        private bool isInitialized = false;

        private bool keyboard_updated = false;
        private bool peripheral_updated = false;

        CorsairKeyboard keyboard;
        CorsairMouse mouse;
        CorsairHeadset headset;

        private Dictionary<CorsairKeyboardKeyId, Color> saved_keys = new Dictionary<CorsairKeyboardKeyId, Color>();
        private Dictionary<CorsairMouseLedId, Color> saved_mouse = new Dictionary<CorsairMouseLedId, Color>();
        private Dictionary<CorsairHeadsetLedId, Color> saved_headset = new Dictionary<CorsairHeadsetLedId, Color>();

        //Previous data
        private Color previous_peripheral_Color = Color.Black;

        public string GetDeviceName()
        {
            return devicename;
        }

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                return devicename + ": " + (keyboard != null ? keyboard.DeviceInfo.Model + " " : "") + (mouse != null ? mouse.DeviceInfo.Model + " " : "") + (headset != null ? headset.DeviceInfo.Model + " " : "");
            }
            else
            {
                return devicename + ": Not initialized";
            }
        }

        public bool Initialize()
        {
            if (!isInitialized)
            {
                try
                {
                    CueSDK.Initialize(true);
                    Global.logger.LogLine("Corsair device, Initialized with " + CueSDK.LoadedArchitecture + "-SDK", Logging_Level.Info);

                    keyboard = CueSDK.KeyboardSDK;
                    mouse = CueSDK.MouseSDK;
                    headset = CueSDK.HeadsetSDK;

                    if (keyboard == null && mouse == null && headset == null)
                        throw new WrapperException("No devices found");
                    else
                    {
                        if (Global.Configuration.corsair_first_time)
                        {
                            CorsairInstallInstructions instructions = new CorsairInstallInstructions();
                            instructions.ShowDialog();

                            Global.Configuration.corsair_first_time = false;
                            Settings.ConfigManager.Save(Global.Configuration);
                        }

                        SaveLeds();
                        isInitialized = true;
                        return true;
                    }
                }
                catch (CUEException ex)
                {
                    Global.logger.LogLine("Corsair device, CUE Exception! ErrorCode: " + Enum.GetName(typeof(CorsairError), ex.Error), Logging_Level.Error);
                }
                catch (WrapperException ex)
                {
                    Global.logger.LogLine("Corsair device, Wrapper Exception! Message:" + ex.Message, Logging_Level.Error);
                }
                catch (Exception ex)
                {
                    Global.logger.LogLine("Corsair device, Exception! Message:" + ex, Logging_Level.Error);
                }

                isInitialized = false;
                return false;
            }

            return isInitialized;
        }

        private void SaveLeds()
        {
            Global.logger.LogLine("Corsair starting saving leds", Logging_Level.Info);

            if (keyboard != null)
            {
                CorsairKeyboardKeyId[] allkeys = Enum.GetValues(typeof(CorsairKeyboardKeyId)).Cast<CorsairKeyboardKeyId>().ToArray();
                foreach (CorsairKeyboardKeyId key in allkeys)
                {
                    if (key != CorsairKeyboardKeyId.Invalid && keyboard[key] != null)
                        saved_keys.Add(key, keyboard[key].Led.Color);
                }
            }

            if (mouse != null)
            {
                CorsairMouseLedId[] allbuttons = Enum.GetValues(typeof(CorsairMouseLedId)).Cast<CorsairMouseLedId>().ToArray();
                foreach (CorsairMouseLedId key in allbuttons)
                {
                    if (key != CorsairMouseLedId.Invalid && mouse[key] != null)
                        saved_mouse.Add(key, mouse[key].Color);
                }
            }

            if (headset != null)
            {
                CorsairHeadsetLedId[] allbuttons = Enum.GetValues(typeof(CorsairHeadsetLedId)).Cast<CorsairHeadsetLedId>().ToArray();
                foreach (CorsairHeadsetLedId key in allbuttons)
                {
                    if (key != CorsairHeadsetLedId.Invalid && headset[key] != null)
                        saved_headset.Add(key, headset[key].Color);
                }

            }

            Global.logger.LogLine("Corsair saved leds", Logging_Level.Info);
        }

        private void RestoreLeds()
        {
            Global.logger.LogLine("Corsair starting restoring leds", Logging_Level.Info);

            if (keyboard != null)
            {
                foreach (var key in saved_keys)
                    keyboard[key.Key].Led.Color = key.Value;
                keyboard.Update();
            }

            if (mouse != null)
            {
                foreach (var key in saved_mouse)
                    mouse[key.Key].Color = key.Value;
                mouse.Update();
            }

            if (headset != null)
            {
                foreach (var key in saved_headset)
                    headset[key.Key].Color = key.Value;
                headset.Update();
            }

            Global.logger.LogLine("Corsair restored leds", Logging_Level.Info);
        }

        public void Shutdown()
        {
        }

        public void Reset()
        {
            if (this.IsInitialized() && (keyboard_updated || peripheral_updated))
            {
                RestoreLeds();

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

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced = false)
        {
            CorsairKeyboardKeyId keyindex = CorsairKeyboardKeyId.Invalid;

            try
            {
                foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                {
                    CorsairKeyboardKeyId localKey = ToCorsair(key.Key);

                    if (localKey == CorsairKeyboardKeyId.Invalid && key.Key == DeviceKeys.Peripheral)
                    {
                        SendColorToPeripheral((Color)(key.Value), forced);
                    }
                    else if (localKey != CorsairKeyboardKeyId.Invalid)
                    {
                        SetOneKey(localKey, (Color)(key.Value));
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
                keyboard.Update(true);
                keyboard_updated = true;
            }
        }

        private void SetOneKey(CorsairKeyboardKeyId localKey, Color color)
        {
            //Apply and strip Alpha
            color = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));

            if (keyboard != null && keyboard[localKey] != null && keyboard[localKey].Led != null)
                keyboard[localKey].Led.Color = color;
        }

        private void SendColorToPeripheral(Color color, bool forced = false)
        {
            if ((!previous_peripheral_Color.Equals(color) || forced))
            {
                if (Global.Configuration.allow_peripheral_devices)
                {
                    //Apply and strip Alpha
                    color = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));

                    if (mouse != null)
                    {
                        if (mouse[CorsairMouseLedId.B1] != null)
                            mouse[CorsairMouseLedId.B1].Color = color;
                        if (mouse[CorsairMouseLedId.B2] != null)
                            mouse[CorsairMouseLedId.B2].Color = color;
                        if (mouse[CorsairMouseLedId.B3] != null)
                            mouse[CorsairMouseLedId.B3].Color = color;
                        if (mouse[CorsairMouseLedId.B4] != null)
                            mouse[CorsairMouseLedId.B4].Color = color;

                        mouse.Update(true);
                    }

                    if (headset != null)
                    {
                        if (headset[CorsairHeadsetLedId.LeftLogo] != null)
                            headset[CorsairHeadsetLedId.LeftLogo].Color = color;
                        if (headset[CorsairHeadsetLedId.RightLogo] != null)
                            headset[CorsairHeadsetLedId.RightLogo].Color = color;

                        headset.Update(true);
                    }

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

        private CorsairKeyboardKeyId ToCorsair(DeviceKeys key)
        {
            switch (key)
            {
                case (DeviceKeys.LOGO):
                    return CorsairKeyboardKeyId.CLK_Logo;
                case (DeviceKeys.BRIGHTNESS_SWITCH):
                    return CorsairKeyboardKeyId.Brightness;
                case (DeviceKeys.LOCK_SWITCH):
                    return CorsairKeyboardKeyId.WinLock;

                case (DeviceKeys.ESC):
                    return CorsairKeyboardKeyId.Escape;
                case (DeviceKeys.F1):
                    return CorsairKeyboardKeyId.F1;
                case (DeviceKeys.F2):
                    return CorsairKeyboardKeyId.F2;
                case (DeviceKeys.F3):
                    return CorsairKeyboardKeyId.F3;
                case (DeviceKeys.F4):
                    return CorsairKeyboardKeyId.F4;
                case (DeviceKeys.F5):
                    return CorsairKeyboardKeyId.F5;
                case (DeviceKeys.F6):
                    return CorsairKeyboardKeyId.F6;
                case (DeviceKeys.F7):
                    return CorsairKeyboardKeyId.F7;
                case (DeviceKeys.F8):
                    return CorsairKeyboardKeyId.F8;
                case (DeviceKeys.F9):
                    return CorsairKeyboardKeyId.F9;
                case (DeviceKeys.F10):
                    return CorsairKeyboardKeyId.F10;
                case (DeviceKeys.F11):
                    return CorsairKeyboardKeyId.F11;
                case (DeviceKeys.F12):
                    return CorsairKeyboardKeyId.F12;
                case (DeviceKeys.PRINT_SCREEN):
                    return CorsairKeyboardKeyId.PrintScreen;
                case (DeviceKeys.SCROLL_LOCK):
                    return CorsairKeyboardKeyId.ScrollLock;
                case (DeviceKeys.PAUSE_BREAK):
                    return CorsairKeyboardKeyId.PauseBreak;
                case (DeviceKeys.TILDE):
                    return CorsairKeyboardKeyId.GraveAccentAndTilde;
                case (DeviceKeys.ONE):
                    return CorsairKeyboardKeyId.D1;
                case (DeviceKeys.TWO):
                    return CorsairKeyboardKeyId.D2;
                case (DeviceKeys.THREE):
                    return CorsairKeyboardKeyId.D3;
                case (DeviceKeys.FOUR):
                    return CorsairKeyboardKeyId.D4;
                case (DeviceKeys.FIVE):
                    return CorsairKeyboardKeyId.D5;
                case (DeviceKeys.SIX):
                    return CorsairKeyboardKeyId.D6;
                case (DeviceKeys.SEVEN):
                    return CorsairKeyboardKeyId.D7;
                case (DeviceKeys.EIGHT):
                    return CorsairKeyboardKeyId.D8;
                case (DeviceKeys.NINE):
                    return CorsairKeyboardKeyId.D9;
                case (DeviceKeys.ZERO):
                    return CorsairKeyboardKeyId.D0;
                case (DeviceKeys.MINUS):
                    return CorsairKeyboardKeyId.MinusAndUnderscore;
                case (DeviceKeys.EQUALS):
                    return CorsairKeyboardKeyId.EqualsAndPlus;
                case (DeviceKeys.BACKSPACE):
                    return CorsairKeyboardKeyId.Backspace;
                case (DeviceKeys.INSERT):
                    return CorsairKeyboardKeyId.Insert;
                case (DeviceKeys.HOME):
                    return CorsairKeyboardKeyId.Home;
                case (DeviceKeys.PAGE_UP):
                    return CorsairKeyboardKeyId.PageUp;
                case (DeviceKeys.NUM_LOCK):
                    return CorsairKeyboardKeyId.NumLock;
                case (DeviceKeys.NUM_SLASH):
                    return CorsairKeyboardKeyId.KeypadSlash;
                case (DeviceKeys.NUM_ASTERISK):
                    return CorsairKeyboardKeyId.KeypadAsterisk;
                case (DeviceKeys.NUM_MINUS):
                    return CorsairKeyboardKeyId.KeypadMinus;
                case (DeviceKeys.TAB):
                    return CorsairKeyboardKeyId.Tab;
                case (DeviceKeys.Q):
                    return CorsairKeyboardKeyId.Q;
                case (DeviceKeys.W):
                    return CorsairKeyboardKeyId.W;
                case (DeviceKeys.E):
                    return CorsairKeyboardKeyId.E;
                case (DeviceKeys.R):
                    return CorsairKeyboardKeyId.R;
                case (DeviceKeys.T):
                    return CorsairKeyboardKeyId.T;
                case (DeviceKeys.Y):
                    return CorsairKeyboardKeyId.Y;
                case (DeviceKeys.U):
                    return CorsairKeyboardKeyId.U;
                case (DeviceKeys.I):
                    return CorsairKeyboardKeyId.I;
                case (DeviceKeys.O):
                    return CorsairKeyboardKeyId.O;
                case (DeviceKeys.P):
                    return CorsairKeyboardKeyId.P;
                case (DeviceKeys.OPEN_BRACKET):
                    return CorsairKeyboardKeyId.BracketLeft;
                case (DeviceKeys.CLOSE_BRACKET):
                    return CorsairKeyboardKeyId.BracketRight;
                case (DeviceKeys.BACKSLASH):
                    return CorsairKeyboardKeyId.Backslash;
                case (DeviceKeys.DELETE):
                    return CorsairKeyboardKeyId.Delete;
                case (DeviceKeys.END):
                    return CorsairKeyboardKeyId.End;
                case (DeviceKeys.PAGE_DOWN):
                    return CorsairKeyboardKeyId.PageDown;
                case (DeviceKeys.NUM_SEVEN):
                    return CorsairKeyboardKeyId.Keypad7;
                case (DeviceKeys.NUM_EIGHT):
                    return CorsairKeyboardKeyId.Keypad8;
                case (DeviceKeys.NUM_NINE):
                    return CorsairKeyboardKeyId.Keypad9;
                case (DeviceKeys.NUM_PLUS):
                    return CorsairKeyboardKeyId.KeypadPlus;
                case (DeviceKeys.CAPS_LOCK):
                    return CorsairKeyboardKeyId.CapsLock;
                case (DeviceKeys.A):
                    return CorsairKeyboardKeyId.A;
                case (DeviceKeys.S):
                    return CorsairKeyboardKeyId.S;
                case (DeviceKeys.D):
                    return CorsairKeyboardKeyId.D;
                case (DeviceKeys.F):
                    return CorsairKeyboardKeyId.F;
                case (DeviceKeys.G):
                    return CorsairKeyboardKeyId.G;
                case (DeviceKeys.H):
                    return CorsairKeyboardKeyId.H;
                case (DeviceKeys.J):
                    return CorsairKeyboardKeyId.J;
                case (DeviceKeys.K):
                    return CorsairKeyboardKeyId.K;
                case (DeviceKeys.L):
                    return CorsairKeyboardKeyId.L;
                case (DeviceKeys.SEMICOLON):
                    return CorsairKeyboardKeyId.SemicolonAndColon;
                case (DeviceKeys.APOSTROPHE):
                    return CorsairKeyboardKeyId.ApostropheAndDoubleQuote;
                case (DeviceKeys.HASHTAG):
                    return CorsairKeyboardKeyId.NonUsTilde;
                case (DeviceKeys.ENTER):
                    return CorsairKeyboardKeyId.Enter;
                case (DeviceKeys.NUM_FOUR):
                    return CorsairKeyboardKeyId.Keypad4;
                case (DeviceKeys.NUM_FIVE):
                    return CorsairKeyboardKeyId.Keypad5;
                case (DeviceKeys.NUM_SIX):
                    return CorsairKeyboardKeyId.Keypad6;
                case (DeviceKeys.LEFT_SHIFT):
                    return CorsairKeyboardKeyId.LeftShift;
                case (DeviceKeys.BACKSLASH_UK):
                    return CorsairKeyboardKeyId.NonUsBackslash;
                case (DeviceKeys.Z):
                    return CorsairKeyboardKeyId.Z;
                case (DeviceKeys.X):
                    return CorsairKeyboardKeyId.X;
                case (DeviceKeys.C):
                    return CorsairKeyboardKeyId.C;
                case (DeviceKeys.V):
                    return CorsairKeyboardKeyId.V;
                case (DeviceKeys.B):
                    return CorsairKeyboardKeyId.B;
                case (DeviceKeys.N):
                    return CorsairKeyboardKeyId.N;
                case (DeviceKeys.M):
                    return CorsairKeyboardKeyId.M;
                case (DeviceKeys.COMMA):
                    return CorsairKeyboardKeyId.CommaAndLessThan;
                case (DeviceKeys.PERIOD):
                    return CorsairKeyboardKeyId.PeriodAndBiggerThan;
                case (DeviceKeys.FORWARD_SLASH):
                    return CorsairKeyboardKeyId.SlashAndQuestionMark;
                case (DeviceKeys.RIGHT_SHIFT):
                    return CorsairKeyboardKeyId.RightShift;
                case (DeviceKeys.ARROW_UP):
                    return CorsairKeyboardKeyId.UpArrow;
                case (DeviceKeys.NUM_ONE):
                    return CorsairKeyboardKeyId.Keypad1;
                case (DeviceKeys.NUM_TWO):
                    return CorsairKeyboardKeyId.Keypad2;
                case (DeviceKeys.NUM_THREE):
                    return CorsairKeyboardKeyId.Keypad3;
                case (DeviceKeys.NUM_ENTER):
                    return CorsairKeyboardKeyId.KeypadEnter;
                case (DeviceKeys.LEFT_CONTROL):
                    return CorsairKeyboardKeyId.LeftCtrl;
                case (DeviceKeys.LEFT_WINDOWS):
                    return CorsairKeyboardKeyId.LeftGui;
                case (DeviceKeys.LEFT_ALT):
                    return CorsairKeyboardKeyId.LeftAlt;
                case (DeviceKeys.SPACE):
                    return CorsairKeyboardKeyId.Space;
                case (DeviceKeys.RIGHT_ALT):
                    return CorsairKeyboardKeyId.RightAlt;
                case (DeviceKeys.RIGHT_WINDOWS):
                    return CorsairKeyboardKeyId.RightGui;
                case (DeviceKeys.APPLICATION_SELECT):
                    return CorsairKeyboardKeyId.Application;
                case (DeviceKeys.RIGHT_CONTROL):
                    return CorsairKeyboardKeyId.RightCtrl;
                case (DeviceKeys.ARROW_LEFT):
                    return CorsairKeyboardKeyId.LeftArrow;
                case (DeviceKeys.ARROW_DOWN):
                    return CorsairKeyboardKeyId.DownArrow;
                case (DeviceKeys.ARROW_RIGHT):
                    return CorsairKeyboardKeyId.RightArrow;
                case (DeviceKeys.NUM_ZERO):
                    return CorsairKeyboardKeyId.Keypad0;
                case (DeviceKeys.NUM_PERIOD):
                    return CorsairKeyboardKeyId.KeypadPeriodAndDelete;

                case (DeviceKeys.FN_Key):
                    return CorsairKeyboardKeyId.Fn;

                case (DeviceKeys.G1):
                    return CorsairKeyboardKeyId.G1;
                case (DeviceKeys.G2):
                    return CorsairKeyboardKeyId.G2;
                case (DeviceKeys.G3):
                    return CorsairKeyboardKeyId.G3;
                case (DeviceKeys.G4):
                    return CorsairKeyboardKeyId.G4;
                case (DeviceKeys.G5):
                    return CorsairKeyboardKeyId.G5;
                case (DeviceKeys.G6):
                    return CorsairKeyboardKeyId.G6;
                case (DeviceKeys.G7):
                    return CorsairKeyboardKeyId.G7;
                case (DeviceKeys.G8):
                    return CorsairKeyboardKeyId.G8;
                case (DeviceKeys.G9):
                    return CorsairKeyboardKeyId.G9;
                case (DeviceKeys.G10):
                    return CorsairKeyboardKeyId.G10;
                case (DeviceKeys.G11):
                    return CorsairKeyboardKeyId.G11;
                case (DeviceKeys.G12):
                    return CorsairKeyboardKeyId.G12;
                case (DeviceKeys.G13):
                    return CorsairKeyboardKeyId.G13;
                case (DeviceKeys.G14):
                    return CorsairKeyboardKeyId.G14;
                case (DeviceKeys.G15):
                    return CorsairKeyboardKeyId.G15;
                case (DeviceKeys.G16):
                    return CorsairKeyboardKeyId.G16;
                case (DeviceKeys.G17):
                    return CorsairKeyboardKeyId.G17;
                case (DeviceKeys.G18):
                    return CorsairKeyboardKeyId.G18;

                default:
                    return CorsairKeyboardKeyId.Invalid;
            }
        }

        public bool IsKeyboardConnected()
        {
            return keyboard != null;
        }

        public bool IsPeripheralConnected()
        {
            return mouse != null;
        }
    }
}
