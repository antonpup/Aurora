using Aurora.Settings;
using Aurora.Utils;
using CoolerMaster;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wooting;

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

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            try
            {
                //Do this to prevent setting lighting again after the keyboard has been shutdown and reset
                lock (action_lock)
                {
                    if (!this.isInitialized)
                        return false;

                    foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                    {
                        if (e.Cancel) return false;


                        Color clr = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(key.Value, key.Value.A / 255.0D));
                        WootingKey.Keys devKey = DeviceKeyToWootingKey(key.Key);
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

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            bool update_result = UpdateDevice(colorComposition.keyColors, e, forced);

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

        public static Dictionary<DeviceKeys, WootingKey.Keys> KeyMap = new Dictionary<DeviceKeys, WootingKey.Keys> {
            //Row 0
            { DeviceKeys.ESC, WootingKey.Keys.Esc },
            { DeviceKeys.F1, WootingKey.Keys.F1 },
            { DeviceKeys.F2, WootingKey.Keys.F2 },
            { DeviceKeys.F3, WootingKey.Keys.F3 },
            { DeviceKeys.F4, WootingKey.Keys.F4 },
            { DeviceKeys.F5, WootingKey.Keys.F5 },
            { DeviceKeys.F6, WootingKey.Keys.F6 },
            { DeviceKeys.F7, WootingKey.Keys.F7 },
            { DeviceKeys.F8, WootingKey.Keys.F8 },
            { DeviceKeys.F9, WootingKey.Keys.F9 },
            { DeviceKeys.F10, WootingKey.Keys.F10 },
            { DeviceKeys.F11, WootingKey.Keys.F11 },
            { DeviceKeys.F12, WootingKey.Keys.F12 },
            { DeviceKeys.PRINT_SCREEN, WootingKey.Keys.PrintScreen },
            { DeviceKeys.PAUSE_BREAK, WootingKey.Keys.PauseBreak },
            { DeviceKeys.SCROLL_LOCK, WootingKey.Keys.Mode_ScrollLock },
            { DeviceKeys.Profile_Key1, WootingKey.Keys.A1 },
            { DeviceKeys.Profile_Key2,  WootingKey.Keys.A2 },
            { DeviceKeys.Profile_Key3, WootingKey.Keys.A3 },
            { DeviceKeys.Profile_Key4, WootingKey.Keys.Mode },

            //Row 1
            { DeviceKeys.TILDE, WootingKey.Keys.Tilda },
            { DeviceKeys.ONE, WootingKey.Keys.N1 },
            { DeviceKeys.TWO, WootingKey.Keys.N2 },
            { DeviceKeys.THREE, WootingKey.Keys.N3 },
            { DeviceKeys.FOUR, WootingKey.Keys.N4 },
            { DeviceKeys.FIVE, WootingKey.Keys.N5 },
            { DeviceKeys.SIX, WootingKey.Keys.N6 },
            { DeviceKeys.SEVEN, WootingKey.Keys.N7 },
            { DeviceKeys.EIGHT, WootingKey.Keys.N8 },
            { DeviceKeys.NINE, WootingKey.Keys.N9 },
            { DeviceKeys.ZERO, WootingKey.Keys.N0 },
            { DeviceKeys.MINUS, WootingKey.Keys.Minus },
            { DeviceKeys.EQUALS, WootingKey.Keys.Equals },
            { DeviceKeys.BACKSPACE, WootingKey.Keys.Backspace },
            { DeviceKeys.INSERT, WootingKey.Keys.Insert },
            { DeviceKeys.HOME, WootingKey.Keys.Home },
            { DeviceKeys.PAGE_UP, WootingKey.Keys.PageUp },
            { DeviceKeys.NUM_LOCK, WootingKey.Keys.NumLock },
            { DeviceKeys.NUM_SLASH, WootingKey.Keys.NumSlash },
            { DeviceKeys.NUM_ASTERISK, WootingKey.Keys.NumMulti },
            { DeviceKeys.NUM_MINUS, WootingKey.Keys.NumMinus },

            //Row2
            { DeviceKeys.TAB, WootingKey.Keys.Tab },
            { DeviceKeys.Q, WootingKey.Keys.Q },
            { DeviceKeys.W, WootingKey.Keys.W },
            { DeviceKeys.E, WootingKey.Keys.E },
            { DeviceKeys.R, WootingKey.Keys.R},
            { DeviceKeys.T, WootingKey.Keys.T },
            { DeviceKeys.Y, WootingKey.Keys.Y },
            { DeviceKeys.U, WootingKey.Keys.U },
            { DeviceKeys.I, WootingKey.Keys.I },
            { DeviceKeys.O, WootingKey.Keys.O },
            { DeviceKeys.P, WootingKey.Keys.P },
            { DeviceKeys.OPEN_BRACKET, WootingKey.Keys.OpenBracket },
            { DeviceKeys.CLOSE_BRACKET, WootingKey.Keys.CloseBracket },
            { DeviceKeys.BACKSLASH, WootingKey.Keys.ANSI_Backslash },
            { DeviceKeys.DELETE, WootingKey.Keys.Delete },
            { DeviceKeys.END, WootingKey.Keys.End },
            { DeviceKeys.PAGE_DOWN, WootingKey.Keys.PageDown },
            { DeviceKeys.NUM_SEVEN, WootingKey.Keys.Num7 },
            { DeviceKeys.NUM_EIGHT, WootingKey.Keys.Num8 },
            { DeviceKeys.NUM_NINE, WootingKey.Keys.Num9 },
            { DeviceKeys.NUM_PLUS, WootingKey.Keys.NumPlus },

            //Row3
            { DeviceKeys.CAPS_LOCK, WootingKey.Keys.CapsLock },
            { DeviceKeys.A, WootingKey.Keys.A },
            { DeviceKeys.S, WootingKey.Keys.S },
            { DeviceKeys.D, WootingKey.Keys.D },
            { DeviceKeys.F, WootingKey.Keys.F },
            { DeviceKeys.G, WootingKey.Keys.G },
            { DeviceKeys.H, WootingKey.Keys.H },
            { DeviceKeys.J, WootingKey.Keys.J },
            { DeviceKeys.K, WootingKey.Keys.K },
            { DeviceKeys.L, WootingKey.Keys.L },
            { DeviceKeys.SEMICOLON, WootingKey.Keys.SemiColon },
            { DeviceKeys.APOSTROPHE, WootingKey.Keys.Apostophe },
            { DeviceKeys.HASHTAG, WootingKey.Keys.ISO_Hash },
            { DeviceKeys.ENTER, WootingKey.Keys.Enter },
            { DeviceKeys.NUM_FOUR, WootingKey.Keys.Num4 },
            { DeviceKeys.NUM_FIVE, WootingKey.Keys.Num5 },
            { DeviceKeys.NUM_SIX, WootingKey.Keys.Num6 },

            //Row4
            { DeviceKeys.LEFT_SHIFT, WootingKey.Keys.LeftShift },
            { DeviceKeys.BACKSLASH_UK, WootingKey.Keys.ISO_Blackslash },
            { DeviceKeys.Z, WootingKey.Keys.Z },
            { DeviceKeys.X, WootingKey.Keys.X },
            { DeviceKeys.C, WootingKey.Keys.C },
            { DeviceKeys.V, WootingKey.Keys.V },
            { DeviceKeys.B, WootingKey.Keys.B },
            { DeviceKeys.N, WootingKey.Keys.N },
            { DeviceKeys.M, WootingKey.Keys.M },
            { DeviceKeys.COMMA, WootingKey.Keys.Comma },
            { DeviceKeys.PERIOD, WootingKey.Keys.Period },
            { DeviceKeys.FORWARD_SLASH, WootingKey.Keys.Slash },

            { DeviceKeys.RIGHT_SHIFT, WootingKey.Keys.RightShift },

            { DeviceKeys.ARROW_UP, WootingKey.Keys.Up },

            { DeviceKeys.NUM_ONE, WootingKey.Keys.Num1 },
            { DeviceKeys.NUM_TWO, WootingKey.Keys.Num2 },
            { DeviceKeys.NUM_THREE, WootingKey.Keys.Num3 },
            { DeviceKeys.NUM_ENTER, WootingKey.Keys.NumEnter },

            //Row5
            { DeviceKeys.LEFT_CONTROL, WootingKey.Keys.LeftCtrl },
            { DeviceKeys.LEFT_WINDOWS, WootingKey.Keys.LeftWin },
            { DeviceKeys.LEFT_ALT, WootingKey.Keys.LeftAlt },



            { DeviceKeys.SPACE, WootingKey.Keys.Space },



            { DeviceKeys.RIGHT_ALT, WootingKey.Keys.RightAlt },
            { DeviceKeys.RIGHT_WINDOWS, WootingKey.Keys.RightWin },
            { DeviceKeys.FN_Key, WootingKey.Keys.Function },
            { DeviceKeys.RIGHT_CONTROL, WootingKey.Keys.RightControl },
            { DeviceKeys.ARROW_LEFT, WootingKey.Keys.Left },
            { DeviceKeys.ARROW_DOWN, WootingKey.Keys.Down },
            { DeviceKeys.ARROW_RIGHT, WootingKey.Keys.Right },

            { DeviceKeys.NUM_ZERO, WootingKey.Keys.Num0 },
            { DeviceKeys.NUM_PERIOD, WootingKey.Keys.NumPeriod },
        };

        public static WootingKey.Keys DeviceKeyToWootingKey(DeviceKeys key)
        {
            if (KeyMap.TryGetValue(key, out WootingKey.Keys w_key))
                return w_key;

            return WootingKey.Keys.None;
        }
    }
}
