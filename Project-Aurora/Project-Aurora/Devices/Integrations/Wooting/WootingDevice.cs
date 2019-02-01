using Aurora.Devices.Layout;
using Aurora.Devices.Layout.Layouts;
using Aurora.Settings;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wooting;
using LEDINT = System.Int16;

namespace Aurora.Devices.Wooting
{
    class WootingDevice : Device
    {
        private String devicename = "Wooting";
        private bool isInitialized = false;

        private bool keyboard_updated = false;
        private VariableRegistry default_registry = null;

        private readonly object action_lock = new object();

        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

        public bool Initialize()
        {
            lock (action_lock)
            {
                if (!isInitialized)
                {
                    try
                    {
                        if (RGBControl.IsConnected())
                        {
                            isInitialized = true;
                        }
                    }
                    catch (Exception exc)
                    {
                        Global.logger.Error("There was an error initializing Wooting SDK.\r\n" + exc.Message);

                        return false;
                    }
                }

                if (!isInitialized)
                    Global.logger.Info("No Wooting devices successfully Initialized!");

                return isInitialized;
            }
        }

        ~WootingDevice()
        {
            this.Shutdown();
        }

        public void Shutdown()
        {
            lock (action_lock)
            {
                if (isInitialized)
                {
                    RGBControl.Reset();
                    isInitialized = false;
                }
            }
        }

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                string devString = devicename + ": ";
                devString += "Connected";
                return devString;
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

        public void Reset()
        {
            if (this.IsInitialized() && keyboard_updated)
            {
                keyboard_updated = false;
            }
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool IsInitialized()
        {
            return this.isInitialized;
        }

        public bool UpdateKeyboard(KeyboardDeviceLayout device, DoWorkEventArgs e, bool forced = false)
        {
            try
            {
                //Do this to prevent setting lighting again after the keyboard has been shutdown and reset
                lock (action_lock)
                {
                    if (!this.isInitialized)
                        return false;

                    foreach (KeyValuePair<LEDINT, Color> key in device.DeviceColours.deviceColours)
                    {
                        if (e.Cancel) return false;


                        Color clr = Color.FromArgb(255, ColorUtils.MultiplyColorByScalar(key.Value, key.Value.A / 255.0D));
                        WootingKey.Keys devKey = DeviceKeyToWootingKey((KeyboardKeys)key.Key);
                        if (devKey == WootingKey.Keys.None)
                            continue;
                        //(byte row, byte column) coordinates = WootingRgbControl.KeyMap[devKey];
                        //colourMap[coordinates.row, coordinates.column] = new KeyColour(clr.red, clr.green, clr.blue);
                        
                        RGBControl.SetKey(devKey, (byte)(clr.R * Global.Configuration.VarRegistry.GetVariable<int>($"{devicename}_scalar_r")/100),
                                                  (byte)(clr.G * Global.Configuration.VarRegistry.GetVariable<int>($"{devicename}_scalar_g")/100),
                                                  (byte)(clr.B * Global.Configuration.VarRegistry.GetVariable<int>($"{devicename}_scalar_b")/100));
                    }
                    if (e.Cancel) return false;
                    //AlsoWootingRgbControl.SetFull(colourMap);
                    RGBControl.UpdateKeyboard();
                }
                return true;
            }
            catch (Exception exc)
            {
                Global.logger.Error("Failed to Update Device" + exc.ToString());
                return false;
            }
        }

        public bool UpdateDevice(List<DeviceLayout> devices, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            bool update_result = UpdateKeyboard((KeyboardDeviceLayout)devices.First(s => s is KeyboardDeviceLayout), e, forced);

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return update_result;
        }

        public bool IsKeyboardConnected()
        {
            return isInitialized;
        }

        public bool IsPeripheralConnected()
        {
            return isInitialized;
        }

        public string GetDeviceUpdatePerformance()
        {
            return (isInitialized ? lastUpdateTime + " ms" : "");
        }

        public VariableRegistry GetRegisteredVariables()
        {
            if (default_registry == null)
            {
                default_registry = new VariableRegistry();
                default_registry.Register($"{devicename}_scalar_r", 100, "Red Scalar", 100, 0);
                default_registry.Register($"{devicename}_scalar_g", 100, "Green Scalar", 100, 0);
                default_registry.Register($"{devicename}_scalar_b", 100, "Blue Scalar", 100, 0,"In percent");
            }
            return default_registry;
        }

        public static Dictionary<KeyboardKeys, WootingKey.Keys> KeyMap = new Dictionary<KeyboardKeys, WootingKey.Keys> {
            //Row 0
            { KeyboardKeys.ESC, WootingKey.Keys.Esc },
            { KeyboardKeys.F1, WootingKey.Keys.F1 },
            { KeyboardKeys.F2, WootingKey.Keys.F2 },
            { KeyboardKeys.F3, WootingKey.Keys.F3 },
            { KeyboardKeys.F4, WootingKey.Keys.F4 },
            { KeyboardKeys.F5, WootingKey.Keys.F5 },
            { KeyboardKeys.F6, WootingKey.Keys.F6 },
            { KeyboardKeys.F7, WootingKey.Keys.F7 },
            { KeyboardKeys.F8, WootingKey.Keys.F8 },
            { KeyboardKeys.F9, WootingKey.Keys.F9 },
            { KeyboardKeys.F10, WootingKey.Keys.F10 },
            { KeyboardKeys.F11, WootingKey.Keys.F11 },
            { KeyboardKeys.F12, WootingKey.Keys.F12 },
            { KeyboardKeys.PRINT_SCREEN, WootingKey.Keys.PrintScreen },
            { KeyboardKeys.PAUSE_BREAK, WootingKey.Keys.PauseBreak },
            { KeyboardKeys.SCROLL_LOCK, WootingKey.Keys.Mode_ScrollLock },
            { KeyboardKeys.Profile_Key1, WootingKey.Keys.A1 },
            { KeyboardKeys.Profile_Key2,  WootingKey.Keys.A2 },
            { KeyboardKeys.Profile_Key3, WootingKey.Keys.A3 },
            { KeyboardKeys.Profile_Key4, WootingKey.Keys.Mode },

            //Row 1
            { KeyboardKeys.TILDE, WootingKey.Keys.Tilda },
            { KeyboardKeys.ONE, WootingKey.Keys.N1 },
            { KeyboardKeys.TWO, WootingKey.Keys.N2 },
            { KeyboardKeys.THREE, WootingKey.Keys.N3 },
            { KeyboardKeys.FOUR, WootingKey.Keys.N4 },
            { KeyboardKeys.FIVE, WootingKey.Keys.N5 },
            { KeyboardKeys.SIX, WootingKey.Keys.N6 },
            { KeyboardKeys.SEVEN, WootingKey.Keys.N7 },
            { KeyboardKeys.EIGHT, WootingKey.Keys.N8 },
            { KeyboardKeys.NINE, WootingKey.Keys.N9 },
            { KeyboardKeys.ZERO, WootingKey.Keys.N0 },
            { KeyboardKeys.MINUS, WootingKey.Keys.Minus },
            { KeyboardKeys.EQUALS, WootingKey.Keys.Equals },
            { KeyboardKeys.BACKSPACE, WootingKey.Keys.Backspace },
            { KeyboardKeys.INSERT, WootingKey.Keys.Insert },
            { KeyboardKeys.HOME, WootingKey.Keys.Home },
            { KeyboardKeys.PAGE_UP, WootingKey.Keys.PageUp },
            { KeyboardKeys.NUM_LOCK, WootingKey.Keys.NumLock },
            { KeyboardKeys.NUM_SLASH, WootingKey.Keys.NumSlash },
            { KeyboardKeys.NUM_ASTERISK, WootingKey.Keys.NumMulti },
            { KeyboardKeys.NUM_MINUS, WootingKey.Keys.NumMinus },

            //Row2
            { KeyboardKeys.TAB, WootingKey.Keys.Tab },
            { KeyboardKeys.Q, WootingKey.Keys.Q },
            { KeyboardKeys.W, WootingKey.Keys.W },
            { KeyboardKeys.E, WootingKey.Keys.E },
            { KeyboardKeys.R, WootingKey.Keys.R},
            { KeyboardKeys.T, WootingKey.Keys.T },
            { KeyboardKeys.Y, WootingKey.Keys.Y },
            { KeyboardKeys.U, WootingKey.Keys.U },
            { KeyboardKeys.I, WootingKey.Keys.I },
            { KeyboardKeys.O, WootingKey.Keys.O },
            { KeyboardKeys.P, WootingKey.Keys.P },
            { KeyboardKeys.OPEN_BRACKET, WootingKey.Keys.OpenBracket },
            { KeyboardKeys.CLOSE_BRACKET, WootingKey.Keys.CloseBracket },
            { KeyboardKeys.BACKSLASH, WootingKey.Keys.ANSI_Backslash },
            { KeyboardKeys.DELETE, WootingKey.Keys.Delete },
            { KeyboardKeys.END, WootingKey.Keys.End },
            { KeyboardKeys.PAGE_DOWN, WootingKey.Keys.PageDown },
            { KeyboardKeys.NUM_SEVEN, WootingKey.Keys.Num7 },
            { KeyboardKeys.NUM_EIGHT, WootingKey.Keys.Num8 },
            { KeyboardKeys.NUM_NINE, WootingKey.Keys.Num9 },
            { KeyboardKeys.NUM_PLUS, WootingKey.Keys.NumPlus },

            //Row3
            { KeyboardKeys.CAPS_LOCK, WootingKey.Keys.CapsLock },
            { KeyboardKeys.A, WootingKey.Keys.A },
            { KeyboardKeys.S, WootingKey.Keys.S },
            { KeyboardKeys.D, WootingKey.Keys.D },
            { KeyboardKeys.F, WootingKey.Keys.F },
            { KeyboardKeys.G, WootingKey.Keys.G },
            { KeyboardKeys.H, WootingKey.Keys.H },
            { KeyboardKeys.J, WootingKey.Keys.J },
            { KeyboardKeys.K, WootingKey.Keys.K },
            { KeyboardKeys.L, WootingKey.Keys.L },
            { KeyboardKeys.SEMICOLON, WootingKey.Keys.SemiColon },
            { KeyboardKeys.APOSTROPHE, WootingKey.Keys.Apostophe },
            { KeyboardKeys.HASHTAG, WootingKey.Keys.ISO_Hash },
            { KeyboardKeys.ENTER, WootingKey.Keys.Enter },
            { KeyboardKeys.NUM_FOUR, WootingKey.Keys.Num4 },
            { KeyboardKeys.NUM_FIVE, WootingKey.Keys.Num5 },
            { KeyboardKeys.NUM_SIX, WootingKey.Keys.Num6 },

            //Row4
            { KeyboardKeys.LEFT_SHIFT, WootingKey.Keys.LeftShift },
            { KeyboardKeys.BACKSLASH_UK, WootingKey.Keys.ISO_Blackslash },
            { KeyboardKeys.Z, WootingKey.Keys.Z },
            { KeyboardKeys.X, WootingKey.Keys.X },
            { KeyboardKeys.C, WootingKey.Keys.C },
            { KeyboardKeys.V, WootingKey.Keys.V },
            { KeyboardKeys.B, WootingKey.Keys.B },
            { KeyboardKeys.N, WootingKey.Keys.N },
            { KeyboardKeys.M, WootingKey.Keys.M },
            { KeyboardKeys.COMMA, WootingKey.Keys.Comma },
            { KeyboardKeys.PERIOD, WootingKey.Keys.Period },
            { KeyboardKeys.FORWARD_SLASH, WootingKey.Keys.Slash },

            { KeyboardKeys.RIGHT_SHIFT, WootingKey.Keys.RightShift },

            { KeyboardKeys.ARROW_UP, WootingKey.Keys.Up },

            { KeyboardKeys.NUM_ONE, WootingKey.Keys.Num1 },
            { KeyboardKeys.NUM_TWO, WootingKey.Keys.Num2 },
            { KeyboardKeys.NUM_THREE, WootingKey.Keys.Num3 },
            { KeyboardKeys.NUM_ENTER, WootingKey.Keys.NumEnter },

            //Row5
            { KeyboardKeys.LEFT_CONTROL, WootingKey.Keys.LeftCtrl },
            { KeyboardKeys.LEFT_WINDOWS, WootingKey.Keys.LeftWin },
            { KeyboardKeys.LEFT_ALT, WootingKey.Keys.LeftAlt },



            { KeyboardKeys.SPACE, WootingKey.Keys.Space },



            { KeyboardKeys.RIGHT_ALT, WootingKey.Keys.RightAlt },
            { KeyboardKeys.RIGHT_WINDOWS, WootingKey.Keys.RightWin },
            { KeyboardKeys.FN_Key, WootingKey.Keys.Function },
            { KeyboardKeys.RIGHT_CONTROL, WootingKey.Keys.RightControl },
            { KeyboardKeys.ARROW_LEFT, WootingKey.Keys.Left },
            { KeyboardKeys.ARROW_DOWN, WootingKey.Keys.Down },
            { KeyboardKeys.ARROW_RIGHT, WootingKey.Keys.Right },

            { KeyboardKeys.NUM_ZERO, WootingKey.Keys.Num0 },
            { KeyboardKeys.NUM_PERIOD, WootingKey.Keys.NumPeriod },
        };

        public static WootingKey.Keys DeviceKeyToWootingKey(KeyboardKeys key)
        {
            if (KeyMap.TryGetValue(key, out WootingKey.Keys w_key))
                return w_key;

            return WootingKey.Keys.None;
        }
    }
}
