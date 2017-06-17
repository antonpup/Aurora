using Aurora.Settings;
using CoolerMaster;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Aurora.Devices.CoolerMaster
{
    static class CoolerMasterKeys
    {
        public static readonly Dictionary<DeviceKeys, int[]> KeyCoords = new Dictionary<DeviceKeys, int[]>
        {
            {DeviceKeys.ESC, new int [] {0,0} },
            {DeviceKeys.F1, new int [] {0, 1} },
            {DeviceKeys.F2, new int [] {0, 2} },
            {DeviceKeys.F3, new int [] {0, 3} },
            {DeviceKeys.F4, new int [] {0, 4} },
            {DeviceKeys.F5, new int [] {0, 6} },
            {DeviceKeys.F6, new int [] {0, 7} },
            {DeviceKeys.F7, new int [] {0, 8} },
            {DeviceKeys.F8, new int [] {0, 9} },
            {DeviceKeys.F9, new int [] {0, 11} },
            {DeviceKeys.F10, new int [] {0, 12} },
            {DeviceKeys.F11, new int [] {0, 13} },
            {DeviceKeys.F12, new int [] {0, 14} },
            {DeviceKeys.PRINT_SCREEN, new int [] {0, 15} },
            {DeviceKeys.SCROLL_LOCK, new int [] {0, 16} },
            {DeviceKeys.PAUSE_BREAK, new int [] {0, 17} },
            {DeviceKeys.Profile_Key1, new int [] {0, 18} },
            {DeviceKeys.Profile_Key2, new int [] {0, 19} },
            {DeviceKeys.Profile_Key3, new int [] {0, 20} },
            {DeviceKeys.Profile_Key4, new int [] {0, 21} },
            {DeviceKeys.TILDE, new int [] {1, 0} },
            {DeviceKeys.ONE, new int [] {1, 1} },
            {DeviceKeys.TWO, new int [] {1, 2} },
            {DeviceKeys.THREE, new int [] {1, 3} },
            {DeviceKeys.FOUR, new int [] {1, 4} },
            {DeviceKeys.FIVE, new int [] {1, 5} },
            {DeviceKeys.SIX, new int [] {1, 6} },
            {DeviceKeys.SEVEN, new int [] {1, 7} },
            {DeviceKeys.EIGHT, new int [] {1, 8} },
            {DeviceKeys.NINE, new int [] {1, 9} },
            {DeviceKeys.ZERO, new int [] {1, 10} },
            {DeviceKeys.MINUS, new int [] {1, 11} },
            {DeviceKeys.EQUALS, new int [] {1, 12} },
            {DeviceKeys.BACKSPACE, new int [] {1, 14} },
            {DeviceKeys.INSERT, new int [] {1, 15} },
            {DeviceKeys.HOME, new int [] {1, 16} },
            {DeviceKeys.PAGE_UP, new int [] {1, 17} },
            {DeviceKeys.NUM_LOCK, new int [] {1, 18} },
            {DeviceKeys.NUM_SLASH, new int [] {1, 19} },
            {DeviceKeys.NUM_ASTERISK, new int [] {1, 20} },
            {DeviceKeys.NUM_MINUS, new int [] {1, 21} },
            {DeviceKeys.TAB, new int [] {2, 0} },
            {DeviceKeys.Q, new int [] {2, 1} },
            {DeviceKeys.W, new int [] {2, 2} },
            {DeviceKeys.E, new int [] {2, 3} },
            {DeviceKeys.R, new int [] {2, 4} },
            {DeviceKeys.T, new int [] {2, 5} },
            {DeviceKeys.Y, new int [] {2, 6} },
            {DeviceKeys.U, new int [] {2, 7} },
            {DeviceKeys.I, new int [] {2, 8} },
            {DeviceKeys.O, new int [] {2, 9} },
            {DeviceKeys.P, new int [] {2, 10} },
            {DeviceKeys.OPEN_BRACKET, new int [] {2, 11} },
            {DeviceKeys.CLOSE_BRACKET, new int [] {2,12} },
            {DeviceKeys.BACKSLASH, new int [] {2, 14} },
            {DeviceKeys.DELETE, new int [] {2, 15} },
            {DeviceKeys.END, new int [] {2, 16} },
            {DeviceKeys.PAGE_DOWN, new int [] {2, 17} },
            {DeviceKeys.NUM_SEVEN, new int [] {2, 18} },
            {DeviceKeys.NUM_EIGHT, new int [] {2, 19} },
            {DeviceKeys.NUM_NINE, new int [] {2, 20} },
            {DeviceKeys.NUM_PLUS, new int [] {2, 21} },
            {DeviceKeys.CAPS_LOCK, new int [] {3, 0} },
            {DeviceKeys.A, new int [] {3, 1} },
            {DeviceKeys.S, new int [] {3, 2} },
            {DeviceKeys.D, new int [] {3, 3} },
            {DeviceKeys.F, new int [] {3, 4} },
            {DeviceKeys.G, new int [] {3, 5} },
            {DeviceKeys.H, new int [] {3, 6} },
            {DeviceKeys.J, new int [] {3, 7} },
            {DeviceKeys.K, new int [] {3, 8} },
            {DeviceKeys.L, new int [] {3, 9} },
            {DeviceKeys.SEMICOLON, new int [] {3, 10} },
            {DeviceKeys.APOSTROPHE, new int [] {3, 11} },
            {DeviceKeys.HASHTAG, new int [] {3, 12} },
            {DeviceKeys.ENTER, new int [] {3, 14} },
            {DeviceKeys.NUM_FOUR, new int [] {3, 18} },
            {DeviceKeys.NUM_FIVE, new int [] {3, 19} },
            {DeviceKeys.NUM_SIX, new int [] {3, 20} },
            {DeviceKeys.LEFT_SHIFT, new int [] {4, 0} },
            {DeviceKeys.BACKSLASH_UK, new int [] {4, 1} },
            {DeviceKeys.Z, new int [] {4, 2} },
            {DeviceKeys.X, new int [] {4, 3} },
            {DeviceKeys.C, new int [] {4, 4} },
            {DeviceKeys.V, new int [] {4, 5} },
            {DeviceKeys.B, new int [] {4, 6} },
            {DeviceKeys.N, new int [] {4, 7} },
            {DeviceKeys.M, new int [] {4, 8} },
            {DeviceKeys.COMMA, new int [] {4, 9} },
            {DeviceKeys.PERIOD, new int [] {4, 10} },
            {DeviceKeys.FORWARD_SLASH, new int [] {4, 11} },
            {DeviceKeys.RIGHT_SHIFT, new int [] {4, 14} },
            {DeviceKeys.ARROW_UP, new int [] {4, 16} },
            {DeviceKeys.NUM_ONE, new int [] {4, 18} },
            {DeviceKeys.NUM_TWO, new int [] {4, 19} },
            {DeviceKeys.NUM_THREE, new int [] {4, 20} },
            {DeviceKeys.NUM_ENTER, new int [] {4, 21} },
            {DeviceKeys.LEFT_CONTROL, new int [] {5, 0} },
            {DeviceKeys.LEFT_WINDOWS, new int [] {5, 1} },
            {DeviceKeys.LEFT_ALT, new int [] {5, 2} },
            {DeviceKeys.SPACE, new int [] {5, 6} },
            {DeviceKeys.RIGHT_ALT, new int [] {5, 10} },
            {DeviceKeys.RIGHT_WINDOWS, new int [] {5, 11} },
            {DeviceKeys.APPLICATION_SELECT, new int [] {5, 12} },
            {DeviceKeys.RIGHT_CONTROL, new int [] {5, 14} },
            {DeviceKeys.ARROW_LEFT, new int [] {5, 15} },
            {DeviceKeys.ARROW_DOWN, new int [] {5, 16} },
            {DeviceKeys.ARROW_RIGHT, new int [] {5, 17} },
            {DeviceKeys.NUM_ZERO, new int [] {5, 18} },
            {DeviceKeys.NUM_PERIOD, new int [] {5, 20} }
        };
    }

    class CoolerMasterDevice : Device
    {
        private String devicename = "Cooler Master";
        private bool isInitialized = false;

        private bool keyboard_updated = false;
        private bool peripheral_updated = false;

        private readonly object action_lock = new object();

        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

        //Keyboard stuff
        private CoolerMasterSDK.COLOR_MATRIX color_matrix = new CoolerMasterSDK.COLOR_MATRIX();
        private CoolerMasterSDK.KEY_COLOR[,] key_colors = new CoolerMasterSDK.KEY_COLOR[CoolerMasterSDK.MAX_LED_ROW, CoolerMasterSDK.MAX_LED_COLUMN];
        //private Color peripheral_Color = Color.Black;

        //Previous data
        //private CoolerMasterSDK.KEY_COLOR[,] previous_key_colors = new CoolerMasterSDK.KEY_COLOR[CoolerMasterSDK.MAX_LED_ROW, CoolerMasterSDK.MAX_LED_COLUMN];
        //private Color previous_peripheral_Color = Color.Black;


        public bool Initialize()
        {
            lock (action_lock)
            {
                if (!isInitialized)
                {
                    try
                    {
                        CoolerMasterSDK.SetControlDevice(CoolerMasterSDK.DEVICE_INDEX.DEV_MKeys_L);
                        if (CoolerMasterSDK.IsDevicePlug() && CoolerMasterSDK.EnableLedControl(true))
                        {
                            isInitialized = true;
                            return true;
                        }
                        else
                        {
                            CoolerMasterSDK.SetControlDevice(CoolerMasterSDK.DEVICE_INDEX.DEV_MKeys_S);
                            if (CoolerMasterSDK.IsDevicePlug() && CoolerMasterSDK.EnableLedControl(true))
                            {
                                isInitialized = true;
                                return true;
                            }
                            else
                            {
                                CoolerMasterSDK.SetControlDevice(CoolerMasterSDK.DEVICE_INDEX.DEV_MKeys_L_White);
                                if (CoolerMasterSDK.IsDevicePlug() && CoolerMasterSDK.EnableLedControl(true))
                                {
                                    isInitialized = true;
                                    return true;
                                }
                                else
                                {
                                    CoolerMasterSDK.SetControlDevice(CoolerMasterSDK.DEVICE_INDEX.DEV_MKeys_M_White);
                                    if (CoolerMasterSDK.IsDevicePlug() && CoolerMasterSDK.EnableLedControl(true))
                                    {
                                        isInitialized = true;
                                        return true;
                                    }
                                    else
                                    {
                                        CoolerMasterSDK.SetControlDevice(CoolerMasterSDK.DEVICE_INDEX.DEV_MKeys_M);
                                        if (CoolerMasterSDK.IsDevicePlug() && CoolerMasterSDK.EnableLedControl(true))
                                        {
                                            isInitialized = true;
                                            return true;
                                        }
                                        else
                                        {
                                            CoolerMasterSDK.SetControlDevice(CoolerMasterSDK.DEVICE_INDEX.DEV_MKeys_S_White);
                                            if (CoolerMasterSDK.IsDevicePlug() && CoolerMasterSDK.EnableLedControl(true))
                                            {
                                                isInitialized = true;
                                                return true;
                                            }
                                            else
                                            {
                                                Global.logger.LogLine("Cooler Master device control could not be initialized", Logging_Level.Error);

                                                isInitialized = false;
                                                return false;
                                            }
                                        }
                                    }
                                }
                            }
                        }


                    }
                    catch (Exception exc)
                    {
                        Global.logger.LogLine("There was an error initializing Cooler Master SDK.\r\n" + exc.Message, Logging_Level.Error);

                        return false;
                    }
                }

                return isInitialized;
            }
        }

        public void Shutdown()
        {
            lock (action_lock)
            {
                if (isInitialized)
                {
                    CoolerMasterSDK.EnableLedControl(false);
                    isInitialized = false;
                }
            }
        }

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                return devicename + ": Connected";
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

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        private void SetOneKey(int[] key, Color color)
        {
            CoolerMasterSDK.KEY_COLOR key_color = new CoolerMasterSDK.KEY_COLOR(color.R, color.G, color.B);
            key_colors[key[0], key[1]] = key_color;
        }

        private void SendColorsToKeyboard(bool forced = false)
        {
            if (Global.Configuration.devices_disable_keyboard)
                return;

            color_matrix.KeyColor = key_colors;
            CoolerMasterSDK.SetAllLedColor(color_matrix);
            //previous_key_colors = key_colors;
            keyboard_updated = true;
        }

        private void SendColorToPeripheral(Color color, bool forced = false)
        {
            peripheral_updated = false;
        }

        public bool IsInitialized()
        {
            return this.isInitialized;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced = false)
        {
            try
            {
                foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                {
                    int[] coordinates = new int[2];

                    DeviceKeys dev_key = key.Key;

                    if (dev_key == DeviceKeys.ENTER && (
                            Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.uk ||
                            Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.ru
                            )
                        )
                        dev_key = DeviceKeys.BACKSLASH;


                    if (CoolerMasterKeys.KeyCoords.TryGetValue(dev_key, out coordinates))
                    {
                        SetOneKey(coordinates, (Color)key.Value);
                    }
                }
                SendColorsToKeyboard(forced || !keyboard_updated);
                return true;
            }
            catch (Exception e)
            {
                Global.logger.LogLine("Failed to Update Device" + e.ToString(), Logging_Level.Error);
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
            return new VariableRegistry();
        }
    }
}
