/* Credit to https://github.com/horgeon 
* for his initial developments in Roccat support
* here: https://github.com/horgeon/Aurora/commits/dev
*/

/* Side notes about this device:
 * SDK Docs state "Due to hardware and protocol limitations, the approximate latency for on/off events is currently about 20 to 30ms." So there might be a delay for Ryos lighting.
 * The SDK also only allows for individual LEDs to be either ON or OFF, no per-key color lighting.
 */

/* 2018 Update:
 * Completed support for Ryos MK FX
 * Per-key color lighting is now supported, all leds are set in one SetMkFxKeyboardState() call
 * Only US layout is currently supported.
 *
 * REQUIREMENTS:
 * Roccat Talk FX must be installed and running (there should be an icon in system tray)
 * Download: https://www.roccat.org/en-US/Products/Gaming-Software/Talk-FX/
 *
 * 3rd party DLLs:
 * - Roccat-Talk.dll (from: https://github.com/mwasilak/roccat-talk-csharp , branch feature-ryos-mk-fx)
 * - talkfx-c.dll (from: https://github.com/mwasilak/talkfx-c-wrapper , branch feature-ryos-mk-fx)
 */



using Roccat_Talk.RyosTalkFX;
using Roccat_Talk.TalkFX;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Settings;
using System.ComponentModel;

namespace Aurora.Devices.Roccat
{
    class RoccatDevice : Device
    {
        private String devicename = "Roccat";
        private bool isInitialized = false;

        private TalkFxConnection talkFX = null;
        private RyosTalkFXConnection RyosTalkFX = null;
        private bool RyosInitialized = false;
        private bool generic_deactivated_first_time = true;
        private bool generic_activated_first_time = true;

        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;
        private VariableRegistry default_registry = null;

        private System.Drawing.Color previous_peripheral_Color = System.Drawing.Color.Black;
        public static Dictionary<DeviceKeys, byte> DeviceKeysMap = new Dictionary<DeviceKeys, byte>
        {
            {DeviceKeys.ESC, 0 },
            {DeviceKeys.F1, 1 },
            {DeviceKeys.F2, 2 },
            {DeviceKeys.F3, 3 },
            {DeviceKeys.F4, 4 },
            {DeviceKeys.F5, 5 },
            {DeviceKeys.F6, 6 },
            {DeviceKeys.F7, 7 },
            {DeviceKeys.F8, 8 },
            {DeviceKeys.F9, 9 },
            {DeviceKeys.F10, 10 },
            {DeviceKeys.F11, 11 },
            {DeviceKeys.F12, 12 },
            {DeviceKeys.PRINT_SCREEN, 13 },
            {DeviceKeys.SCROLL_LOCK, 14 },
            {DeviceKeys.PAUSE_BREAK, 15 },
            {DeviceKeys.G1, 16 },
            {DeviceKeys.TILDE, 17 },
            {DeviceKeys.ONE, 18 },
            {DeviceKeys.TWO, 19 },
            {DeviceKeys.THREE, 20 },
            {DeviceKeys.FOUR, 21 },
            {DeviceKeys.FIVE, 22 },
            {DeviceKeys.SIX, 23 },
            {DeviceKeys.SEVEN, 24 },
            {DeviceKeys.EIGHT, 25 },
            {DeviceKeys.NINE, 26 },
            {DeviceKeys.ZERO, 27 },
            {DeviceKeys.MINUS, 28 },
            {DeviceKeys.EQUALS, 29 },
            {DeviceKeys.BACKSPACE, 30 },
            {DeviceKeys.INSERT, 31 },
            {DeviceKeys.HOME, 32 },
            {DeviceKeys.PAGE_UP, 33 },
            {DeviceKeys.NUM_LOCK, 34 },
            {DeviceKeys.NUM_SLASH, 35 },
            {DeviceKeys.NUM_ASTERISK, 36 },
            {DeviceKeys.NUM_MINUS, 37 },
            {DeviceKeys.G2, 38 },
            {DeviceKeys.TAB, 39 },
            {DeviceKeys.Q, 40 },
            {DeviceKeys.W, 41 },
            {DeviceKeys.E, 42 },
            {DeviceKeys.R, 43 },
            {DeviceKeys.T, 44 },
            {DeviceKeys.Y, 45 },
            {DeviceKeys.U, 46 },
            {DeviceKeys.I, 47 },
            {DeviceKeys.O, 48 },
            {DeviceKeys.P, 49 },
            {DeviceKeys.OPEN_BRACKET, 50 },
            {DeviceKeys.CLOSE_BRACKET, 51 },
            {DeviceKeys.BACKSLASH, 52 },
            {DeviceKeys.DELETE, 53 },
            {DeviceKeys.END, 54 },
            {DeviceKeys.PAGE_DOWN, 55 },
            {DeviceKeys.NUM_SEVEN, 56 },
            {DeviceKeys.NUM_EIGHT, 57 },
            {DeviceKeys.NUM_NINE, 58 },
            {DeviceKeys.NUM_PLUS, 59 },
            {DeviceKeys.G3, 60 },
            {DeviceKeys.CAPS_LOCK, 61 },
            {DeviceKeys.A, 62 },
            {DeviceKeys.S, 63 },
            {DeviceKeys.D, 64 },
            {DeviceKeys.F, 65 },
            {DeviceKeys.G, 66 },
            {DeviceKeys.H, 67 },
            {DeviceKeys.J, 68 },
            {DeviceKeys.K, 69 },
            {DeviceKeys.L, 70 },
            {DeviceKeys.SEMICOLON, 71 },
            {DeviceKeys.APOSTROPHE, 72 },
            {DeviceKeys.ENTER, 73 },
            {DeviceKeys.NUM_FOUR, 74 },
            {DeviceKeys.NUM_FIVE, 75 },
            {DeviceKeys.NUM_SIX, 76 },
            {DeviceKeys.G4, 77 },
            {DeviceKeys.LEFT_SHIFT, 78 },
            {DeviceKeys.BACKSLASH_UK, 79 },
            {DeviceKeys.Z, 80 },
            {DeviceKeys.X, 81 },
            {DeviceKeys.C, 82 },
            {DeviceKeys.V, 83 },
            {DeviceKeys.B, 84 },
            {DeviceKeys.N, 85 },
            {DeviceKeys.M, 86 },
            {DeviceKeys.COMMA, 87 },
            {DeviceKeys.PERIOD, 88 },
            {DeviceKeys.FORWARD_SLASH, 89 },
            {DeviceKeys.RIGHT_SHIFT, 90 },
            {DeviceKeys.ARROW_UP, 91 },
            {DeviceKeys.NUM_ONE, 92 },
            {DeviceKeys.NUM_TWO, 93 },
            {DeviceKeys.NUM_THREE, 94 },
            {DeviceKeys.NUM_ENTER, 95 },
            {DeviceKeys.G5, 96 },
            {DeviceKeys.LEFT_CONTROL, 97 },
            {DeviceKeys.LEFT_WINDOWS, 98 },
            {DeviceKeys.LEFT_ALT, 99 },
            {DeviceKeys.SPACE, 100 },
            {DeviceKeys.RIGHT_ALT, 101 },
            {DeviceKeys.FN_Key, 102 },
            {DeviceKeys.APPLICATION_SELECT, 103 },
            {DeviceKeys.RIGHT_CONTROL, 104 },
            {DeviceKeys.ARROW_LEFT, 105 },
            {DeviceKeys.ARROW_DOWN, 106 },
            {DeviceKeys.ARROW_RIGHT, 107 },
            {DeviceKeys.NUM_ZERO, 108 },
            {DeviceKeys.NUM_PERIOD, 109 }
        };
        public enum DeviceLayout : byte
        {
            ISO = 0,
            ANSI = 1,
            JP = 2
        }

        private byte layout = 0x01; //TALKFX_RYOS_LAYOUT_US

        public string GetDeviceName()
        {
            return devicename;
        }

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                return devicename + ": " + (talkFX != null ? "TalkFX Initialized " : "") + (RyosTalkFX != null && RyosInitialized ? "RyosTalkFX Initialized " : "");
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
                    talkFX = new TalkFxConnection();
                    RyosTalkFX = new RyosTalkFXConnection();

                    if (RyosTalkFX != null)
                    {
                        RyosInitialized = RyosTalkFX.Initialize();
                        RyosInitialized = RyosInitialized && RyosTalkFX.EnterSdkMode();
                    }

                    if (talkFX == null ||
                        RyosTalkFX == null ||
                        !RyosInitialized
                        )
                    {
                        throw new Exception("No devices connected");
                    }
                    if (Global.Configuration.roccat_first_time)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            RoccatInstallInstructions instructions = new RoccatInstallInstructions();
                            instructions.ShowDialog();
                        });
                        Global.Configuration.roccat_first_time = false;
                        Settings.ConfigManager.Save(Global.Configuration);
                    }
                    isInitialized = true;
                    return true;
                }
                catch (Exception ex)
                {
                    Global.logger.Error("Roccat device, Exception! Message:" + ex);
                }

                isInitialized = false;
                return false;
            }

            return isInitialized;
        }

        public void Shutdown()
        {
            if (talkFX != null)
            {
                Restoregeneric();
            }

            if (RyosTalkFX != null)
            {
                RyosTalkFX.ExitSdkMode();
            }
            isInitialized = false;
        }

        public void Reset()
        {
            if (this.IsInitialized())
            {
                Restoregeneric();
            }
        }

        private void Restoregeneric()
        {
            //Workaround
            //Global.logger.LogLine("restore Roccat generic");
            System.Drawing.Color restore_fallback = Global.Configuration.VarRegistry.GetVariable<Aurora.Utils.RealColor>($"{devicename}_restore_fallback").GetDrawingColor();
            restore_fallback = System.Drawing.Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(restore_fallback, restore_fallback.A / 255.0D));

            //Global.logger.LogLine("restore Roccat generic" + restore_fallback + restore_fallback.R + restore_fallback.G + restore_fallback.B);
            talkFX.SetLedRgb(Zone.Event, KeyEffect.On, Speed.Fast, new Color(restore_fallback.R, restore_fallback.G, restore_fallback.B));

            previous_peripheral_Color = System.Drawing.Color.FromArgb(restore_fallback.R, restore_fallback.G, restore_fallback.B);

            //.RestoreLedRgb() Does not work 
            talkFX.RestoreLedRgb();
        }

        private void send_to_roccat_generic(System.Drawing.Color color)
        {
            //Alpha necessary for Global Brightness modifier
            color = System.Drawing.Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));
            talkFX.SetLedRgb(Zone.Event, KeyEffect.On, Speed.Fast, new Color(color.R, color.G, color.B)); ;
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

        byte[] stateStruct = new byte[110];
        Roccat_Talk.TalkFX.Color[] colorStruct = new Roccat_Talk.TalkFX.Color[110];
        public bool UpdateDevice(Dictionary<DeviceKeys, System.Drawing.Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (RyosTalkFX == null || !RyosInitialized)
                return false;

            if (e.Cancel) return false;

            try
            {
                DeviceLayout layout = DeviceLayout.ISO;
                if (Global.Configuration.keyboard_localization == PreferredKeyboardLocalization.dvorak
                    || Global.Configuration.keyboard_localization == PreferredKeyboardLocalization.us
                    || Global.Configuration.keyboard_localization == PreferredKeyboardLocalization.ru)
                    layout = DeviceLayout.ANSI;
                else if (Global.Configuration.keyboard_localization == PreferredKeyboardLocalization.jpn)
                    layout = DeviceLayout.JP;

                foreach (KeyValuePair<DeviceKeys, System.Drawing.Color> key in keyColors)
                {
                    if (e.Cancel) return false;
                    DeviceKeys dev_key = key.Key;
                    //Solution to slightly different mapping rather than giving a whole different dictionary
                    if (layout == DeviceLayout.ANSI)
                    {
                        if (dev_key == DeviceKeys.ENTER)
                            dev_key = DeviceKeys.BACKSLASH;
                        if (dev_key == DeviceKeys.HASHTAG)
                            dev_key = DeviceKeys.ENTER;
                    }

                    //set peripheral color to Roccat generic peripheral if enabled
                    if (Global.Configuration.VarRegistry.GetVariable<bool>($"{devicename}_enable_generic") == true)
                    {
                        generic_deactivated_first_time = true;
                        if (key.Key == DeviceKeys.Peripheral_Logo || key.Key == DeviceKeys.Peripheral)
                        {
                            //Send to generic roccat device if color not equal or 1. time after generic got enabled
                            if (!previous_peripheral_Color.Equals(key.Value) || generic_activated_first_time == true)
                            {
                                send_to_roccat_generic(key.Value);
                                //talkFX.RestoreLedRgb(); //Does not even here work

                                previous_peripheral_Color = key.Value;
                                generic_activated_first_time = false;
                            }
                        }
                    }
                    else
                    {
                        if (generic_deactivated_first_time == true)
                        {
                            Restoregeneric();
                            generic_deactivated_first_time = false;
                            //Global.logger.LogLine("first time");
                        }
                        generic_activated_first_time = true;
                    }

                    if (DeviceKeysMap.TryGetValue(dev_key, out byte i))
                    {
                        //Global.logger.LogLine("Roccat update device: " + key + " , " + key.Value);
                        Color roccatColor = ConvertToRoccatColor(key.Value);
                        stateStruct[i] = IsLedOn(roccatColor);
                        colorStruct[i] = roccatColor;
                    }
                }

                //send KeyboardState to Ryos only when enabled
                if (Global.Configuration.VarRegistry.GetVariable<bool>($"{devicename}_enable_ryos"))
                {
                    RyosTalkFX.SetMkFxKeyboardState(stateStruct, colorStruct, (byte)layout);
                }

                return true;
            }
            catch (Exception exc)
            {
                Global.logger.Error("Roccat device, error when updating device. Error: " + exc);
                return false;
            }
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            if (e.Cancel) return false;

            bool update_result = UpdateDevice(colorComposition.keyColors, e, forced);

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return update_result;
        }

        private byte IsLedOn(Roccat_Talk.TalkFX.Color roccatColor)
        {
            if (roccatColor.Red == 0 && roccatColor.Green == 0 && roccatColor.Blue == 0)
            {
                return 0;
            }
            return 1;
        }

        public bool IsKeyboardConnected()
        {
            return false;
        }

        public bool IsPeripheralConnected()
        {
            return this.IsInitialized();
        }

        private Roccat_Talk.TalkFX.Color ConvertToRoccatColor(System.Drawing.Color color)
        {
            return new Roccat_Talk.TalkFX.Color(color.R, color.G, color.B);
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
                default_registry.Register($"{devicename}_enable_generic", true, "Enable Generic support");
                default_registry.Register($"{devicename}_enable_ryos", true, "Enable Ryos support");
                default_registry.Register($"{devicename}_restore_fallback", new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(255, 0, 0, 255)), "Color", new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(255, 255, 255, 255)), new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(0, 0, 0, 0)), "Set restore color for your generic roccat devices");
            }

            return default_registry;
        }
    }
}
