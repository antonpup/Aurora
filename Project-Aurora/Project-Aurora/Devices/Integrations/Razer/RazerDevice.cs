using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using Corale.Colore.Core;
using Corale.Colore.Razer.Keyboard;
using Corale.Colore.Razer.Mouse;
using KeyboardCustom = Corale.Colore.Razer.Keyboard.Effects.Custom;
using MouseCustom = Corale.Colore.Razer.Mouse.Effects.CustomGrid;
using MousepadCustom = Corale.Colore.Razer.Mousepad.Effects.Custom;
using Aurora.Settings;
using Aurora.Devices.Layout;
using Aurora.Devices.Layout.Layouts;
using LEDINT = System.Int16;

namespace Aurora.Devices.Razer
{
    class RazerDevice : Device
    {
        private String devicename = "Razer";
        private bool isInitialized = false;

        private bool keyboard_updated = false;
        private bool mouse_updated = false;
        private bool peripheral_updated = false;
        private KeyboardCustom grid = KeyboardCustom.Create();
        private MouseCustom MouseGrid = MouseCustom.Create();
        private MousepadCustom MousepadGrid = MousepadCustom.Create();
        //private bool bladeLayout = true;

        IKeyboard keyboard = null;
        IMouse mouse = null;
        IHeadset headset = null;
        IMousepad mousepad = null;
        IKeypad keypad = null;
        IChromaLink chromalink = null;

        private readonly object action_lock = new object();

        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

        private System.Drawing.Color previous_peripheral_Color = System.Drawing.Color.Black;

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                return devicename + ": " + (keyboard != null ? "Keyboard Connected " : "") + (mouse != null ? "Mouse Connected " : "") + (headset != null ? "Headset Connected " : "") + (mousepad != null ? "Mousepad Connected " : "") + (chromalink != null ? "ChromaLink Connected " : "");
            }
            else
            {
                return devicename + ": Not initialized";
            }
        }

        public string GetDeviceName()
        {
            return devicename;
        }

        public string GetDeviceUpdatePerformance()
        {
            return (isInitialized ? lastUpdateTime + " ms" : "");
        }

        public VariableRegistry GetRegisteredVariables()
        {
            return new VariableRegistry();
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

                        Global.logger.Info("Razer device, Initialized");

                        keyboard = Chroma.Instance.Keyboard;
                        mouse = Chroma.Instance.Mouse;
                        headset = Chroma.Instance.Headset;
                        mousepad = Chroma.Instance.Mousepad;
                        keypad = Chroma.Instance.Keypad;
                        chromalink = Chroma.Instance.ChromaLink;

                        if (keyboard == null &&
                            mouse == null &&
                            headset == null &&
                            mousepad == null &&
                            keypad == null &&
                            chromalink == null
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
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    RazerInstallInstructions instructions = new RazerInstallInstructions();
                                    instructions.ShowDialog();
                                });
                                Global.Configuration.razer_first_time = false;
                                Settings.ConfigManager.Save(Global.Configuration);
                            }

                            isInitialized = true;
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Global.logger.Error("Razer device, Exception! Message:" + ex);
                    }

                    isInitialized = false;
                    return false;
                }

                return isInitialized;
            }
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool IsInitialized()
        {
            return isInitialized && Chroma.Instance.Initialized;
        }

        public bool IsKeyboardConnected()
        {
            return keyboard != null;
        }

        public bool IsPeripheralConnected()
        {
            return (mouse != null || headset != null || mousepad != null);
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            if (this.IsInitialized() && (keyboard_updated || peripheral_updated))
            {
                keyboard_updated = false;
                peripheral_updated = false;
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
                    Global.logger.Error("Razer device, Exception during Shutdown. Message: " + exc);
                    isInitialized = false;
                }
            }
        }

        public bool UpdateDevice(System.Drawing.Color GlobalColor, List<DeviceLayout> devices, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            bool updateResult = true;

            try
            {

                foreach (DeviceLayout layout in devices)
                {
                    switch (layout)
                    {
                        case KeyboardDeviceLayout kb:
                            if (!UpdateDevice(kb, e, forced))
                                updateResult = false;
                            break;
                        case MouseDeviceLayout mouse:
                            if (!UpdateDevice(mouse, e, forced))
                                updateResult = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Global.logger.Error("Razer device, error when updating device: " + ex);
                return false;
            }

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return updateResult;
        }

        public bool UpdateDevice(KeyboardDeviceLayout device, DoWorkEventArgs e, bool forced = false)
        {
            try
            {
                foreach (KeyValuePair<LEDINT, System.Drawing.Color> key in device.DeviceColours.deviceColours)
                {
                    if (e.Cancel) return false;
                    int[] coord = null;

                    if ((coord = GetKeyCoord((KeyboardKeys)key.Key, device.Style)) != null)
                    {
                        grid[coord[0], coord[1]] = new Corale.Colore.Core.Color(key.Value.R, key.Value.G, key.Value.B);
                    }
                    else
                    {
                        Key localKey = ToRazer((KeyboardKeys)key.Key);
                        if(localKey != Key.Invalid)
                            grid[localKey] = new Corale.Colore.Core.Color(key.Value.R, key.Value.G, key.Value.B);
                    }
                }
                if (e.Cancel) return false;

                keyboard.SetCustom(grid);
                keyboard_updated = true;

                return true;
            }
            catch (Exception exc)
            {
                Global.logger.Error("Razer device, error when updating device. Error: " + exc);
                Console.WriteLine(exc);
                return false;
            }
        }

        private Key ToRazer(KeyboardKeys key)
        {
            switch (key)
            {
                case (KeyboardKeys.ESC):
                    return Key.Escape;
                case (KeyboardKeys.F1):
                    return Key.F1;
                case (KeyboardKeys.F2):
                    return Key.F2;
                case (KeyboardKeys.F3):
                    return Key.F3;
                case (KeyboardKeys.F4):
                    return Key.F4;
                case (KeyboardKeys.F5):
                    return Key.F5;
                case (KeyboardKeys.F6):
                    return Key.F6;
                case (KeyboardKeys.F7):
                    return Key.F7;
                case (KeyboardKeys.F8):
                    return Key.F8;
                case (KeyboardKeys.F9):
                    return Key.F9;
                case (KeyboardKeys.F10):
                    return Key.F10;
                case (KeyboardKeys.F11):
                    return Key.F11;
                case (KeyboardKeys.F12):
                    return Key.F12;
                case (KeyboardKeys.PRINT_SCREEN):
                    return Key.PrintScreen;
                case (KeyboardKeys.SCROLL_LOCK):
                    return Key.Scroll;
                case (KeyboardKeys.PAUSE_BREAK):
                    return Key.Pause;
                case (KeyboardKeys.TILDE):
                    return Key.OemTilde;
                case (KeyboardKeys.ONE):
                    return Key.D1;
                case (KeyboardKeys.TWO):
                    return Key.D2;
                case (KeyboardKeys.THREE):
                    return Key.D3;
                case (KeyboardKeys.FOUR):
                    return Key.D4;
                case (KeyboardKeys.FIVE):
                    return Key.D5;
                case (KeyboardKeys.SIX):
                    return Key.D6;
                case (KeyboardKeys.SEVEN):
                    return Key.D7;
                case (KeyboardKeys.EIGHT):
                    return Key.D8;
                case (KeyboardKeys.NINE):
                    return Key.D9;
                case (KeyboardKeys.ZERO):
                    return Key.D0;
                case (KeyboardKeys.MINUS):
                    return Key.OemMinus;
                case (KeyboardKeys.EQUALS):
                    return Key.OemEquals;
                case (KeyboardKeys.BACKSPACE):
                    return Key.Backspace;
                case (KeyboardKeys.INSERT):
                    return Key.Insert;
                case (KeyboardKeys.HOME):
                    return Key.Home;
                case (KeyboardKeys.PAGE_UP):
                    return Key.PageUp;
                case (KeyboardKeys.NUM_LOCK):
                    return Key.NumLock;
                case (KeyboardKeys.NUM_SLASH):
                    return Key.NumDivide;
                case (KeyboardKeys.NUM_ASTERISK):
                    return Key.NumMultiply;
                case (KeyboardKeys.NUM_MINUS):
                    return Key.NumSubtract;
                case (KeyboardKeys.TAB):
                    return Key.Tab;
                case (KeyboardKeys.Q):
                    return Key.Q;
                case (KeyboardKeys.W):
                    return Key.W;
                case (KeyboardKeys.E):
                    return Key.E;
                case (KeyboardKeys.R):
                    return Key.R;
                case (KeyboardKeys.T):
                    return Key.T;
                case (KeyboardKeys.Y):
                    return Key.Y;
                case (KeyboardKeys.U):
                    return Key.U;
                case (KeyboardKeys.I):
                    return Key.I;
                case (KeyboardKeys.O):
                    return Key.O;
                case (KeyboardKeys.P):
                    return Key.P;
                case (KeyboardKeys.OPEN_BRACKET):
                    return Key.OemLeftBracket;
                case (KeyboardKeys.CLOSE_BRACKET):
                    return Key.OemRightBracket;
                case (KeyboardKeys.BACKSLASH):
                    return Key.OemBackslash;
                case (KeyboardKeys.DELETE):
                    return Key.Delete;
                case (KeyboardKeys.END):
                    return Key.End;
                case (KeyboardKeys.PAGE_DOWN):
                    return Key.PageDown;
                case (KeyboardKeys.NUM_SEVEN):
                    return Key.Num7;
                case (KeyboardKeys.NUM_EIGHT):
                    return Key.Num8;
                case (KeyboardKeys.NUM_NINE):
                    return Key.Num9;
                case (KeyboardKeys.NUM_PLUS):
                    return Key.NumAdd;
                case (KeyboardKeys.CAPS_LOCK):
                    return Key.CapsLock;
                case (KeyboardKeys.A):
                    return Key.A;
                case (KeyboardKeys.S):
                    return Key.S;
                case (KeyboardKeys.D):
                    return Key.D;
                case (KeyboardKeys.F):
                    return Key.F;
                case (KeyboardKeys.G):
                    return Key.G;
                case (KeyboardKeys.H):
                    return Key.H;
                case (KeyboardKeys.J):
                    return Key.J;
                case (KeyboardKeys.K):
                    return Key.K;
                case (KeyboardKeys.L):
                    return Key.L;
                case (KeyboardKeys.SEMICOLON):
                    return Key.OemSemicolon;
                case (KeyboardKeys.APOSTROPHE):
                    return Key.OemApostrophe;
                case (KeyboardKeys.HASH):
                    return Key.EurPound;
                case (KeyboardKeys.ENTER):
                    return Key.Enter;
                case (KeyboardKeys.NUM_FOUR):
                    return Key.Num4;
                case (KeyboardKeys.NUM_FIVE):
                    return Key.Num5;
                case (KeyboardKeys.NUM_SIX):
                    return Key.Num6;
                case (KeyboardKeys.LEFT_SHIFT):
                    return Key.LeftShift;
                case (KeyboardKeys.BACKSLASH_UK):
                    return Key.EurBackslash;
                case (KeyboardKeys.Z):
                    return Key.Z;
                case (KeyboardKeys.X):
                    return Key.X;
                case (KeyboardKeys.C):
                    return Key.C;
                case (KeyboardKeys.V):
                    return Key.V;
                case (KeyboardKeys.B):
                    return Key.B;
                case (KeyboardKeys.N):
                    return Key.N;
                case (KeyboardKeys.M):
                    return Key.M;
                case (KeyboardKeys.COMMA):
                    return Key.OemComma;
                case (KeyboardKeys.PERIOD):
                    return Key.OemPeriod;
                case (KeyboardKeys.FORWARD_SLASH):
                    return Key.OemSlash;
                case (KeyboardKeys.OEM8):
                    return Key.OemSlash;
                case (KeyboardKeys.RIGHT_SHIFT):
                    return Key.RightShift;
                case (KeyboardKeys.ARROW_UP):
                    return Key.Up;
                case (KeyboardKeys.NUM_ONE):
                    return Key.Num1;
                case (KeyboardKeys.NUM_TWO):
                    return Key.Num2;
                case (KeyboardKeys.NUM_THREE):
                    return Key.Num3;
                case (KeyboardKeys.NUM_ENTER):
                    return Key.NumEnter;
                case (KeyboardKeys.LEFT_CONTROL):
                    return Key.LeftControl;
                case (KeyboardKeys.LEFT_WINDOWS):
                    return Key.LeftWindows;
                case (KeyboardKeys.LEFT_ALT):
                    return Key.LeftAlt;
                case (KeyboardKeys.SPACE):
                    return Key.Space;
                case (KeyboardKeys.RIGHT_ALT):
                    return Key.RightAlt;
                //case (KeyboardKeys.RIGHT_WINDOWS):
                //    return Key.Right;
                case (KeyboardKeys.APPLICATION_SELECT):
                    return Key.RightMenu;
                case (KeyboardKeys.RIGHT_CONTROL):
                    return Key.RightControl;
                case (KeyboardKeys.ARROW_LEFT):
                    return Key.Left;
                case (KeyboardKeys.ARROW_DOWN):
                    return Key.Down;
                case (KeyboardKeys.ARROW_RIGHT):
                    return Key.Right;
                case (KeyboardKeys.NUM_ZERO):
                    return Key.Num0;
                case (KeyboardKeys.NUM_PERIOD):
                    return Key.NumDecimal;
                case (KeyboardKeys.FN_Key):
                    return Key.Function;
                case (KeyboardKeys.G1):
                    return Key.Macro1;
                case (KeyboardKeys.G2):
                    return Key.Macro2;
                case (KeyboardKeys.G3):
                    return Key.Macro3;
                case (KeyboardKeys.G4):
                    return Key.Macro4;
                case (KeyboardKeys.G5):
                    return Key.Macro5;
                case (KeyboardKeys.LOGO):
                    return Key.Logo;
                default:
                    return Key.Invalid;
            }
        }

        private int[] GetKeyCoord(KeyboardKeys key, KeyboardDeviceLayout.PreferredKeyboard keyLayout)
        {
            Dictionary<KeyboardKeys, int[]> layout = RazerLayoutMap.GenericKeyboard;

            if (keyLayout == KeyboardDeviceLayout.PreferredKeyboard.Razer_Blade)
                layout = RazerLayoutMap.Blade;

            if (layout.ContainsKey(key))
                return layout[key];

            return null;
        }

        public bool UpdateDevice(MouseDeviceLayout device, DoWorkEventArgs e, bool forced = false)
        {
            try
            {
                foreach (KeyValuePair<LEDINT, System.Drawing.Color> key in device.DeviceColours.deviceColours)
                {
                    if (e.Cancel) return false;

                    GridLed localLed = ToRazer((MouseLights)key.Key);
                    if (localLed != 0)
                        MouseGrid[localLed] = new Corale.Colore.Core.Color(key.Value.R, key.Value.G, key.Value.B);
                }
                if (e.Cancel) return false;

                mouse.SetGrid(MouseGrid);
                mouse_updated = true;

                return true;
            }
            catch (Exception exc)
            {
                Global.logger.Error("Razer device, error when updating device. Error: " + exc);
                Console.WriteLine(exc);
                return false;
            }
        }

        private GridLed ToRazer(MouseLights light)
        {
            switch (light)
            {
                case MouseLights.Peripheral_Logo:
                    return GridLed.Logo;
                case MouseLights.Peripheral_ScrollWheel:
                    return GridLed.ScrollWheel;
                case MouseLights.Peripheral_BackLight:
                    return GridLed.Backlight;  
                case MouseLights.Peripheral_ExtraLightIndex:
                    return GridLed.Bottom1;
                case MouseLights.Peripheral_ExtraLightIndex+1:
                    return GridLed.Bottom2;
                case MouseLights.Peripheral_ExtraLightIndex+2:
                    return GridLed.Bottom3;
                case MouseLights.Peripheral_ExtraLightIndex+3:
                    return GridLed.Bottom4;
                case MouseLights.Peripheral_ExtraLightIndex+4:
                    return GridLed.Bottom5;
                case MouseLights.Peripheral_ExtraLightLeftIndex:
                    return GridLed.LeftSide1;
                case MouseLights.Peripheral_ExtraLightLeftIndex+1:
                    return GridLed.LeftSide2;
                case MouseLights.Peripheral_ExtraLightLeftIndex+2:
                    return GridLed.LeftSide3;
                case MouseLights.Peripheral_ExtraLightLeftIndex+3:
                    return GridLed.LeftSide4;
                case MouseLights.Peripheral_ExtraLightLeftIndex+4:
                    return GridLed.LeftSide5;
                case MouseLights.Peripheral_ExtraLightLeftIndex+5:
                    return GridLed.LeftSide6;
                case MouseLights.Peripheral_ExtraLightLeftIndex+6:
                    return GridLed.LeftSide7;
                case MouseLights.Peripheral_ExtraLightRightIndex:
                    return GridLed.RightSide1;
                case MouseLights.Peripheral_ExtraLightRightIndex+1:
                    return GridLed.RightSide2;
                case MouseLights.Peripheral_ExtraLightRightIndex+2:
                    return GridLed.RightSide3;
                case MouseLights.Peripheral_ExtraLightRightIndex+3:
                    return GridLed.RightSide4;
                case MouseLights.Peripheral_ExtraLightRightIndex+4:
                    return GridLed.RightSide5;
                case MouseLights.Peripheral_ExtraLightRightIndex+5:
                    return GridLed.RightSide6;
                case MouseLights.Peripheral_ExtraLightRightIndex+6:
                    return GridLed.RightSide7;
                default:
                    return 0;

            }
        }
    }
}
