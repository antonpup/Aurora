﻿using Corale.Colore.Core;
using Corale.Colore.Razer.Keyboard;
using System;
using System.Collections.Generic;
using Aurora.Settings;
using KeyboardCustom = Corale.Colore.Razer.Keyboard.Effects.Custom;

namespace Aurora.Devices.Razer
{
    class RazerDevice : Device
    {
        

        private String devicename = "Razer";
        private bool isInitialized = false;

        private bool keyboard_updated = false;
        private bool peripheral_updated = false;
        private KeyboardCustom grid = KeyboardCustom.Create();
        //private bool bladeLayout = true;

        IKeyboard keyboard = null;
        IMouse mouse = null;
        IHeadset headset = null;
        IMousepad mousepad = null;
        IKeypad keypad = null;

        private readonly object action_lock = new object();

        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

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
            lock (action_lock)
            {
                if (!IsInitialized())
                {
                    try
                    {
                        if (!Chroma.SdkAvailable)
                            throw new Exception("No Chroma SDK available");

                        Chroma.Instance.Initialize();

                        if (Global.writeLogFile) Global.logger.LogLine("Razer device, Initialized", Logging_Level.Info);

                        keyboard = Chroma.Instance.Keyboard;
                        mouse = Chroma.Instance.Mouse;
                        headset = Chroma.Instance.Headset;
                        mousepad = Chroma.Instance.Mousepad;
                        keypad = Chroma.Instance.Keypad;

                        if (keyboard == null &&
                            mouse == null &&
                            headset == null &&
                            mousepad == null &&
                            keypad == null
                            )
                        {
                            throw new Exception("No devices connected");
                        }
                        else
                        {

                            /*if (Chroma.Instance.Query(Corale.Colore.Razer.Devices.BladeStealth).Connected || Chroma.Instance.Query(Corale.Colore.Razer.Devices.Blade14).Connected)
                                bladeLayout = true;*/

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
                        if (Global.writeLogFile) Global.logger.LogLine("Razer device, Exception! Message:" + ex, Logging_Level.Error);
                    }

                    isInitialized = false;
                    return false;
                }

                return isInitialized;
            }
        }

        public void Shutdown()
        {
            lock (action_lock)
            {
                try
                {
                    if (IsInitialized())
                    {
                        //Chroma.Instance.Uninitialize();
                        isInitialized = false;
                    }
                }
                catch (Exception exc)
                {
                    if (Global.writeLogFile) Global.logger.LogLine("Razer device, Exception during Shutdown. Message: " + exc, Logging_Level.Error);
                    isInitialized = false;
                }
            }
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
            return isInitialized && Chroma.Instance.Initialized;
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        private int[] GetKeyCoord(DeviceKeys key)
        {
            Dictionary<DeviceKeys, int[]> layout = RazerLayoutMap.GenericKeyboard;

            if (Global.Configuration.keyboard_brand == PreferredKeyboard.Razer_Blade)
                layout = RazerLayoutMap.Blade;

            if (layout.ContainsKey(key))
                return layout[key];

            return null;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, System.Drawing.Color> keyColors, bool forced = false)
        {
            try
            {
                foreach (KeyValuePair<DeviceKeys, System.Drawing.Color> key in keyColors)
                {
                    //Key localKey = ToRazer(key.Key);

                    int[] coord = null;
                    if (key.Key == DeviceKeys.Peripheral_Logo || key.Key == DeviceKeys.Peripheral)
                    {
                        SendColorToPeripheral(key.Value, forced);
                    }
                    else if ((coord = GetKeyCoord(key.Key))!= null)
                    {
                        SetOneKey(coord, key.Value);
                    }
                    else
                    {
                        Key localKey = ToRazer(key.Key);
                        SetOneKey(localKey, key.Value);
                    }
                }

                SendColorsToKeyboard(forced);
                return true;
            }
            catch (Exception e)
            {
                if (Global.writeLogFile) Global.logger.LogLine("Razer device, error when updating device. Error: " + e, Logging_Level.Error);
                Console.WriteLine(e);
                return false;
            }
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, bool forced = false)
        {
            watch.Restart();

            bool update_result = UpdateDevice(colorComposition.keyColors, forced);

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return update_result;
        }

        private void SendColorsToKeyboard(bool forced = false)
        {
            if (keyboard != null && !Global.Configuration.devices_disable_keyboard)
            {
                keyboard.SetCustom(grid);
                keyboard_updated = true;
            }
        }

        private void SetOneKey(int[] coords, System.Drawing.Color color)
        {
            if (!Global.Configuration.devices_disable_keyboard)
                grid[coords[0], coords[1]] = new Color(color.R, color.G, color.B);
        }

        private void SetOneKey(Key key, System.Drawing.Color color)
        {
            if (key == Key.Invalid)
                return;

            try
            {
                if (!Global.Configuration.devices_disable_keyboard)
                    grid[key] = new Color(color.R, color.G, color.B);
            }
            catch(Exception exc)
            {

            }
        }

        private void SendColorToPeripheral(System.Drawing.Color color, bool forced = false)
        {
            if ((!previous_peripheral_Color.Equals(color) || forced))
            {
                if (Global.Configuration.allow_peripheral_devices)
                {
                    if (mouse != null && !Global.Configuration.devices_disable_mouse)
                        mouse.SetAll(new Color(color.R, color.G, color.B));

                    if (mousepad != null && !Global.Configuration.devices_disable_mouse)
                        mousepad.SetAll(new Color(color.R, color.G, color.B));

                    if (headset != null && !Global.Configuration.devices_disable_headset)
                        headset.SetAll(new Color(color.R, color.G, color.B));

                    if (keypad != null && !Global.Configuration.devices_disable_keyboard)
                        keypad.SetAll(new Color(color.R, color.G, color.B));

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
                case (DeviceKeys.OEM8):
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
                case (DeviceKeys.LOGO):
                    return Key.Logo;
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

        public string GetDeviceUpdatePerformance()
        {
            return (isInitialized ? lastUpdateTime + " ms" : "");
        }

        public VariableRegistry GetRegisteredVariables()
        {
            return new VariableRegistry();
        }
    }
}
