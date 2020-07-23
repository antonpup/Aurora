using Aurora.Settings;
using OpenRGB.NET;
using OpenRGB.NET.Enums;
using Roccat_Talk.RyosTalkFX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DK = Aurora.Devices.DeviceKeys;

namespace Aurora.Devices.OpenRGB
{
    class OpenRGBAuroraDevice : Device
    {
        private string devicename = "OpenRGB";
        VariableRegistry varReg;
        bool isInitialized = false;
        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

        OpenRGBClient client;
        List<OpenRGBDevice> controllers;
        List<OpenRGBColor[]> colors;

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                string devString = devicename + ": ";
                devString += "Connected ";
                var names = controllers.Select(c => c.Name);
                devString += string.Join(",", names);
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

        public string GetDeviceUpdatePerformance()
        {
            return (isInitialized ? lastUpdateTime + " ms" : "");
        }

        public VariableRegistry GetRegisteredVariables()
        {
            if (varReg == null)
            {
                varReg = new VariableRegistry();
                varReg.Register($"{devicename}_sleep", 25, "Sleep for", 1000, 0);
                varReg.Register($"{devicename}_generic", false, "Set colors on generic devices");
            }
            return varReg;
        }

        public bool Initialize()
        {
            if (isInitialized)
                return true;

            try
            {
                client = new OpenRGBClient(name: "Aurora");
                client.Connect();

                var controllerCount = client.GetControllerCount();
                controllers = new List<OpenRGBDevice>();
                colors = new List<OpenRGBColor[]>();

                for (var i = 0; i < controllerCount; i++)
                {
                    var dev = client.GetControllerData(i);
                    controllers.Add(dev);
                    var array = new OpenRGBColor[dev.Colors.Length];
                    for (var j = 0; j < dev.Colors.Length; j++)
                        array[j] = new OpenRGBColor();
                    colors.Add(array);
                }
            }
            catch (Exception e)
            {
                Global.logger.Error("error in OpenRGB device: " + e);
                isInitialized = false;
                return false;
            }

            isInitialized = true;
            return isInitialized;
        }

        public bool IsConnected()
        {
            return isInitialized;
        }

        public bool IsInitialized()
        {
            return isInitialized;
        }

        public bool IsKeyboardConnected()
        {
            return isInitialized;
        }

        public bool IsPeripheralConnected()
        {
            return isInitialized;
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            Shutdown();
            Initialize();
        }

        public void Shutdown()
        {
            if (!isInitialized)
                return;

            for (var i = 0; i < controllers.Count; i++)
            {
                client.UpdateLeds(i, controllers[i].Colors);
            }

            client.Disconnect();
            client = null;
            isInitialized = false;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!isInitialized)
                return false;

            for (var i = 0; i < controllers.Count; i++)
            {
                switch(controllers[i].Type)
                {
                    case OpenRGBDeviceType.Keyboard:
                        for (var j = 0; j < controllers[i].Leds.Count(); j++)
                        {
                            foreach (var kn in OpenRGBKeyNames)
                            {
                                if(kn.Value == controllers[i].Leds[j].Name)
                                {
                                    keyColors.TryGetValue(kn.Key, out var keycolor);
                                    colors[i][j] = new OpenRGBColor(keycolor.R, keycolor.G, keycolor.B);
                                }
                            }
                        }
                        break;

                    case OpenRGBDeviceType.Mouse:
                        break;

                    default:
                        if (!Global.Configuration.VarRegistry.GetVariable<bool>($"{devicename}_generic"))
                            continue;
                        if (keyColors.TryGetValue(DK.Peripheral_Logo, out var color))
                        {
                            for (int j = 0; j < colors[i].Length; j++)
                            {
                                colors[i][j] = new OpenRGBColor(color.R, color.G, color.B);
                            }
                        }
                        break;
                }

                client.UpdateLeds(i, colors[i]);
            }
            var sleep = Global.Configuration.VarRegistry.GetVariable<int>($"{devicename}_sleep");
            if (sleep > 0)
                Thread.Sleep(sleep);
            return true;
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            bool update_result = UpdateDevice(colorComposition.keyColors, e, forced);

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return update_result;
        }

        private readonly Dictionary<DK, String> OpenRGBKeyNames = new Dictionary<DK, String>()
        {
            { DK.A                 , "Key: A"                   },
            { DK.B                 , "Key: B"                   },
            { DK.C                 , "Key: C"                   },
            { DK.D                 , "Key: D"                   },
            { DK.E                 , "Key: E"                   },
            { DK.F                 , "Key: F"                   },
            { DK.G                 , "Key: G"                   },
            { DK.H                 , "Key: H"                   },
            { DK.I                 , "Key: I"                   },
            { DK.J                 , "Key: J"                   },
            { DK.K                 , "Key: K"                   },
            { DK.L                 , "Key: L"                   },
            { DK.M                 , "Key: M"                   },
            { DK.N                 , "Key: N"                   },
            { DK.O                 , "Key: O"                   },
            { DK.P                 , "Key: P"                   },
            { DK.Q                 , "Key: Q"                   },
            { DK.R                 , "Key: R"                   },
            { DK.S                 , "Key: S"                   },
            { DK.T                 , "Key: T"                   },
            { DK.U                 , "Key: U"                   },
            { DK.V                 , "Key: V"                   },
            { DK.W                 , "Key: W"                   },
            { DK.X                 , "Key: X"                   },
            { DK.Y                 , "Key: Y"                   },
            { DK.Z                 , "Key: Z"                   },
            { DK.ONE               , "Key: 1"                   },
            { DK.TWO               , "Key: 2"                   },
            { DK.THREE             , "Key: 3"                   },
            { DK.FOUR              , "Key: 4"                   },
            { DK.FIVE              , "Key: 5"                   },
            { DK.SIX               , "Key: 6"                   },
            { DK.SEVEN             , "Key: 7"                   },
            { DK.EIGHT             , "Key: 8"                   },
            { DK.NINE              , "Key: 9"                   },
            { DK.ZERO              , "Key: 0"                   },
            { DK.ENTER             , "Key: Enter"               },
            { DK.ESC               , "Key: Escape"              },
            { DK.BACKSPACE         , "Key: Backspace"           },
            { DK.TAB               , "Key: Tab"                 },
            { DK.SPACE             , "Key: Space"               },
            { DK.MINUS             , "Key: -"                   },
            { DK.EQUALS            , "Key: ="                   },
            { DK.OPEN_BRACKET      , "Key: ["                   },
            { DK.CLOSE_BRACKET     , "Key: ]"                   },
            { DK.BACKSLASH         , "Key: \\"                  },
            { DK.HASHTAG           , "Key: #"                   },
            { DK.SEMICOLON         , "Key: ;"                   },
            { DK.APOSTROPHE        , "Key: '"                   },
            { DK.TILDE             , "Key: `"                   },
            { DK.COMMA             , "Key: ,"                   },
            { DK.PERIOD            , "Key: ."                   },
            { DK.FORWARD_SLASH     , "Key: /"                   },
            { DK.CAPS_LOCK         , "Key: Caps Lock"           },
            { DK.F1                , "Key: F1"                  },
            { DK.F2                , "Key: F2"                  },
            { DK.F3                , "Key: F3"                  },
            { DK.F4                , "Key: F4"                  },
            { DK.F5                , "Key: F5"                  },
            { DK.F6                , "Key: F6"                  },
            { DK.F7                , "Key: F7"                  },
            { DK.F8                , "Key: F8"                  },
            { DK.F9                , "Key: F9"                  },
            { DK.F10               , "Key: F10"                 },
            { DK.F11               , "Key: F11"                 },
            { DK.F12               , "Key: F12"                 },
            { DK.PRINT_SCREEN      , "Key: Print Screen"        },
            { DK.SCROLL_LOCK       , "Key: Scroll Lock"         },
            { DK.PAUSE_BREAK       , "Key: Pause/Break"         },
            { DK.INSERT            , "Key: Insert"              },
            { DK.HOME              , "Key: Home"                },
            { DK.PAGE_UP           , "Key: Page Up"             },
            { DK.DELETE            , "Key: Delete"              },
            { DK.END               , "Key: End"                 },
            { DK.PAGE_DOWN         , "Key: Page Down"           },
            { DK.ARROW_RIGHT       , "Key: Right Arrow"         },
            { DK.ARROW_LEFT        , "Key: Left Arrow"          },
            { DK.ARROW_DOWN        , "Key: Down Arrow"          },
            { DK.ARROW_UP          , "Key: Up Arrow"            },
            { DK.NUM_LOCK          , "Key: Num Lock"            },
            { DK.NUM_SLASH         , "Key: Number Pad /"        },
            { DK.NUM_ASTERISK      , "Key: Number Pad *"        },
            { DK.NUM_MINUS         , "Key: Number Pad -"        },
            { DK.NUM_PLUS          , "Key: Number Pad +"        },
            { DK.NUM_ENTER         , "Key: Number Pad Enter"    },
            { DK.NUM_ONE           , "Key: Number Pad 1"        },
            { DK.NUM_TWO           , "Key: Number Pad 2"        },
            { DK.NUM_THREE         , "Key: Number Pad 3"        },
            { DK.NUM_FOUR          , "Key: Number Pad 4"        },
            { DK.NUM_FIVE          , "Key: Number Pad 5"        },
            { DK.NUM_SIX           , "Key: Number Pad 6"        },
            { DK.NUM_SEVEN         , "Key: Number Pad 7"        },
            { DK.NUM_EIGHT         , "Key: Number Pad 8"        },
            { DK.NUM_NINE          , "Key: Number Pad 9"        },
            { DK.NUM_ZERO          , "Key: Number Pad 0"        },
            { DK.NUM_PERIOD        , "Key: Number Pad ."        },
            { DK.LEFT_FN           , "Key: Left Fn"             },
            { DK.FN_Key            , "Key: Right Fn"            },
            { DK.BACKSLASH_UK      , "Key: \\ (UK)"             },
            { DK.APPLICATION_SELECT, "Key: Context"             },
            { DK.LEFT_CONTROL      , "Key: Left Control"        },
            { DK.LEFT_SHIFT        , "Key: Left Shift"          },
            { DK.LEFT_ALT          , "Key: Left Alt"            },
            { DK.LEFT_WINDOWS      , "Key: Left Windows"        },
            { DK.RIGHT_CONTROL     , "Key: Right Control"       },
            { DK.RIGHT_SHIFT       , "Key: Right Shift"         },
            { DK.RIGHT_ALT         , "Key: Right Alt"           },
            { DK.RIGHT_WINDOWS     , "Key: Right Windows"       },
            { DK.MEDIA_NEXT        , "Key: Media Next"          },
            { DK.MEDIA_PREVIOUS    , "Key: Media Previous"      },
            { DK.MEDIA_STOP        , "Key: Media Stop"          },
            { DK.MEDIA_PLAY        , "Key: Media Play"          },
            { DK.VOLUME_MUTE       , "Key: Media Mute"          },
            { DK.LOGO              , "Logo"                     },
            { DK.BRIGHTNESS_SWITCH , "Key: Brightness"          },
            { DK.G0                , "Key: G0"                  },
            { DK.G1                , "Key: G1"                  },
            { DK.G2                , "Key: G2"                  },
            { DK.G3                , "Key: G3"                  },
        };
    }
}
