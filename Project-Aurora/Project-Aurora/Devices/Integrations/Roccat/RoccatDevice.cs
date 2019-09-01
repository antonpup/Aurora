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
using Aurora.Devices.Layout;
using Aurora.Devices.Layout.Layouts;
using LEDINT = System.Int16;

namespace Aurora.Devices.Roccat
{
    
    public class RoccatSettings : FirstTimeDeviceSettings
    {
        //default_registry.Register($"{devicename}_enable_generic", true, "Enable Generic support");
        //default_registry.Register($"{devicename}_enable_ryos", true, "Enable Ryos support");
        //default_registry.Register($"{devicename}_restore_fallback", new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(255, 0, 0, 255)), "Color", new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(255, 255, 255, 255)), new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(0, 0, 0, 0)), "Set restore color for your generic roccat devices");
        private bool enableGeneric = true;
        public bool EnableGeneric { get { return enableGeneric; } set { UpdateVar(ref enableGeneric, value); } }

        private bool enableRyos = true;
        public bool EnableRyos { get { return enableRyos; } set { UpdateVar(ref enableRyos, value); } }

        private System.Drawing.Color restoreFallback = System.Drawing.Color.FromArgb(255,0,0,255);
        public System.Drawing.Color RestoreFallback { get { return restoreFallback; } set { UpdateVar(ref restoreFallback, value); } }
    }
    class RoccatDevice : Device<RoccatSettings>
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private String devicename = "Roccat";

        private TalkFxConnection talkFX = null;
        private RyosTalkFXConnection RyosTalkFX = null;
        private bool RyosInitialized = false;
        private bool generic_deactivated_first_time = true;
        private bool generic_activated_first_time = true;

        private System.Drawing.Color previous_peripheral_Color = System.Drawing.Color.Black;
        public static Dictionary<KeyboardKeys, byte> KeyboardKeysMap = new Dictionary<KeyboardKeys, byte>
        {
            {KeyboardKeys.ESC, 0 },
            {KeyboardKeys.F1, 1 },
            {KeyboardKeys.F2, 2 },
            {KeyboardKeys.F3, 3 },
            {KeyboardKeys.F4, 4 },
            {KeyboardKeys.F5, 5 },
            {KeyboardKeys.F6, 6 },
            {KeyboardKeys.F7, 7 },
            {KeyboardKeys.F8, 8 },
            {KeyboardKeys.F9, 9 },
            {KeyboardKeys.F10, 10 },
            {KeyboardKeys.F11, 11 },
            {KeyboardKeys.F12, 12 },
            {KeyboardKeys.PRINT_SCREEN, 13 },
            {KeyboardKeys.SCROLL_LOCK, 14 },
            {KeyboardKeys.PAUSE_BREAK, 15 },
            {KeyboardKeys.G1, 16 },
            {KeyboardKeys.TILDE, 17 },
            {KeyboardKeys.ONE, 18 },
            {KeyboardKeys.TWO, 19 },
            {KeyboardKeys.THREE, 20 },
            {KeyboardKeys.FOUR, 21 },
            {KeyboardKeys.FIVE, 22 },
            {KeyboardKeys.SIX, 23 },
            {KeyboardKeys.SEVEN, 24 },
            {KeyboardKeys.EIGHT, 25 },
            {KeyboardKeys.NINE, 26 },
            {KeyboardKeys.ZERO, 27 },
            {KeyboardKeys.MINUS, 28 },
            {KeyboardKeys.EQUALS, 29 },
            {KeyboardKeys.BACKSPACE, 30 },
            {KeyboardKeys.INSERT, 31 },
            {KeyboardKeys.HOME, 32 },
            {KeyboardKeys.PAGE_UP, 33 },
            {KeyboardKeys.NUM_LOCK, 34 },
            {KeyboardKeys.NUM_SLASH, 35 },
            {KeyboardKeys.NUM_ASTERISK, 36 },
            {KeyboardKeys.NUM_MINUS, 37 },
            {KeyboardKeys.G2, 38 },
            {KeyboardKeys.TAB, 39 },
            {KeyboardKeys.Q, 40 },
            {KeyboardKeys.W, 41 },
            {KeyboardKeys.E, 42 },
            {KeyboardKeys.R, 43 },
            {KeyboardKeys.T, 44 },
            {KeyboardKeys.Y, 45 },
            {KeyboardKeys.U, 46 },
            {KeyboardKeys.I, 47 },
            {KeyboardKeys.O, 48 },
            {KeyboardKeys.P, 49 },
            {KeyboardKeys.OPEN_BRACKET, 50 },
            {KeyboardKeys.CLOSE_BRACKET, 51 },
            {KeyboardKeys.BACKSLASH, 52 },
            {KeyboardKeys.DELETE, 53 },
            {KeyboardKeys.END, 54 },
            {KeyboardKeys.PAGE_DOWN, 55 },
            {KeyboardKeys.NUM_SEVEN, 56 },
            {KeyboardKeys.NUM_EIGHT, 57 },
            {KeyboardKeys.NUM_NINE, 58 },
            {KeyboardKeys.NUM_PLUS, 59 },
            {KeyboardKeys.G3, 60 },
            {KeyboardKeys.CAPS_LOCK, 61 },
            {KeyboardKeys.A, 62 },
            {KeyboardKeys.S, 63 },
            {KeyboardKeys.D, 64 },
            {KeyboardKeys.F, 65 },
            {KeyboardKeys.G, 66 },
            {KeyboardKeys.H, 67 },
            {KeyboardKeys.J, 68 },
            {KeyboardKeys.K, 69 },
            {KeyboardKeys.L, 70 },
            {KeyboardKeys.SEMICOLON, 71 },
            {KeyboardKeys.APOSTROPHE, 72 },
            {KeyboardKeys.ENTER, 73 },
            {KeyboardKeys.NUM_FOUR, 74 },
            {KeyboardKeys.NUM_FIVE, 75 },
            {KeyboardKeys.NUM_SIX, 76 },
            {KeyboardKeys.G4, 77 },
            {KeyboardKeys.LEFT_SHIFT, 78 },
            {KeyboardKeys.BACKSLASH_UK, 79 },
            {KeyboardKeys.Z, 80 },
            {KeyboardKeys.X, 81 },
            {KeyboardKeys.C, 82 },
            {KeyboardKeys.V, 83 },
            {KeyboardKeys.B, 84 },
            {KeyboardKeys.N, 85 },
            {KeyboardKeys.M, 86 },
            {KeyboardKeys.COMMA, 87 },
            {KeyboardKeys.PERIOD, 88 },
            {KeyboardKeys.FORWARD_SLASH, 89 },
            {KeyboardKeys.RIGHT_SHIFT, 90 },
            {KeyboardKeys.ARROW_UP, 91 },
            {KeyboardKeys.NUM_ONE, 92 },
            {KeyboardKeys.NUM_TWO, 93 },
            {KeyboardKeys.NUM_THREE, 94 },
            {KeyboardKeys.NUM_ENTER, 95 },
            {KeyboardKeys.G5, 96 },
            {KeyboardKeys.LEFT_CONTROL, 97 },
            {KeyboardKeys.LEFT_WINDOWS, 98 },
            {KeyboardKeys.LEFT_ALT, 99 },
            {KeyboardKeys.SPACE, 100 },
            {KeyboardKeys.RIGHT_ALT, 101 },
            {KeyboardKeys.FN_Key, 102 },
            {KeyboardKeys.APPLICATION_SELECT, 103 },
            {KeyboardKeys.RIGHT_CONTROL, 104 },
            {KeyboardKeys.ARROW_LEFT, 105 },
            {KeyboardKeys.ARROW_DOWN, 106 },
            {KeyboardKeys.ARROW_RIGHT, 107 },
            {KeyboardKeys.NUM_ZERO, 108 },
            {KeyboardKeys.NUM_PERIOD, 109 }
        };
        public enum RoccatDeviceLayout : byte
        {
            ISO = 0,
            ANSI = 1,
            JP = 2
        }

        private byte layout = 0x01; //TALKFX_RYOS_LAYOUT_US

        public RoccatDevice() : base()
        {
        }

        public override string GetDeviceName()
        {
            return devicename;
        }

        public override string GetDeviceDetails()
        {
            if (Initialized)
            {
                return devicename + ": " + (talkFX != null ? "TalkFX Initialized " : "") + (RyosTalkFX != null && RyosInitialized ? "RyosTalkFX Initialized " : "");
            }
            else
            {
                return devicename + ": Not initialized";
            }
        }

        public override bool Initialize()
        {
            if (!Initialized)
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
                    if (this.Settings.FirstTime)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            RoccatInstallInstructions instructions = new RoccatInstallInstructions();
                            instructions.ShowDialog();
                        });
                        this.Settings.FirstTime = false;
                    }
                    Initialized = true;
                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error("Roccat device, Exception! Message:" + ex);
                }

                Initialized = false;
                return false;
            }

            return Initialized;
        }

        public override void Shutdown()
        {
            if (talkFX != null)
            {
                Restoregeneric();
            }

            if (RyosTalkFX != null)
            {
                RyosTalkFX.ExitSdkMode();
            }
            Initialized = false;
        }

        public override void Reset()
        {
            if (Initialized)
            {
                Restoregeneric();
            }
        }

        private void Restoregeneric()
        {
            //Workaround
            //Global.logger.LogLine("restore Roccat generic");
            System.Drawing.Color restore_fallback = this.Settings.RestoreFallback;
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
            //color = System.Drawing.Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));
            talkFX.SetLedRgb(Zone.Event, KeyEffect.On, Speed.Fast, new Color(color.R, color.G, color.B)); ;
        }

        public override bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public override bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public override bool IsKeyboardConnected()
        {
            return false;
        }

        public override bool IsPeripheralConnected()
        {
            return Initialized;
        }

        byte[] stateStruct = new byte[110];
        Roccat_Talk.TalkFX.Color[] colorStruct = new Roccat_Talk.TalkFX.Color[110];

        public override bool PerformUpdateDevice(System.Drawing.Color GlobalColor, List<DeviceLayout> devices, DoWorkEventArgs e, bool forced = false)
        {

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
                logger.Error("Roccat device, error when updating device: " + ex);
                return false;
            }

            return updateResult;
        }

        private bool UpdateDevice(KeyboardDeviceLayout kb, DoWorkEventArgs e, bool forced)
        {
            if (RyosTalkFX == null || !RyosInitialized)
                return false;

            if (e.Cancel) return false;

            try
            {
                RoccatDeviceLayout layout = RoccatDeviceLayout.ISO;
                if (kb.Language.IsANSI())
                    layout = RoccatDeviceLayout.ANSI;
                else if (kb.Language == PreferredKeyboardLocalization.jpn)
                    layout = RoccatDeviceLayout.JP;

                foreach (KeyValuePair<LEDINT, System.Drawing.Color> key in kb.DeviceColours.deviceColours)
                {
                    if (e.Cancel) return false;
                    KeyboardKeys dev_key = (KeyboardKeys)key.Key;
                    //Solution to slightly different mapping rather than giving a whole different dictionary
                    if (layout == RoccatDeviceLayout.ANSI)
                    {
                        if (dev_key == KeyboardKeys.ENTER)
                            dev_key = KeyboardKeys.BACKSLASH;
                        if (dev_key == KeyboardKeys.HASH)
                            dev_key = KeyboardKeys.ENTER;
                    }
                    if (KeyboardKeysMap.TryGetValue(dev_key, out byte i))
                    {
                        //Global.logger.LogLine("Roccat update device: " + key + " , " + key.Value);
                        Color roccatColor = ConvertToRoccatColor(key.Value);
                        stateStruct[i] = IsLedOn(roccatColor);
                        colorStruct[i] = roccatColor;
                    }
                }

                //send KeyboardState to Ryos only when enabled
                if (this.Settings.EnableRyos)
                {
                    RyosTalkFX.SetMkFxKeyboardState(stateStruct, colorStruct, (byte)layout);
                }

                return true;
            }
            catch (Exception exc)
            {
                logger.Error("Roccat device, error when updating device. Error: " + exc);
                return false;
            }
        }

        private bool UpdateDevice(MouseDeviceLayout mouse, DoWorkEventArgs e, bool forced)
        {
            if (RyosTalkFX == null || !RyosInitialized)
                return false;

            if (e.Cancel) return false;

            try
            {
                foreach (KeyValuePair<LEDINT, System.Drawing.Color> key in mouse.DeviceColours.deviceColours)
                {
                    if (e.Cancel) return false;
                    if (this.Settings.EnableGeneric)
                    {
                        generic_deactivated_first_time = true;
                        if ((MouseLights)key.Key == MouseLights.Peripheral_Logo)
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
                }
                return true;
            }
            catch (Exception exc)
            {
                logger.Error("Roccat device, error when updating device. Error: " + exc);
                return false;
            }
        }

        private byte IsLedOn(Roccat_Talk.TalkFX.Color roccatColor)
        {
            if (roccatColor.Red == 0 && roccatColor.Green == 0 && roccatColor.Blue == 0)
            {
                return 0;
            }
            return 1;
        }

        private Roccat_Talk.TalkFX.Color ConvertToRoccatColor(System.Drawing.Color color)
        {
            return new Roccat_Talk.TalkFX.Color(color.R, color.G, color.B);
        }

    }
}
