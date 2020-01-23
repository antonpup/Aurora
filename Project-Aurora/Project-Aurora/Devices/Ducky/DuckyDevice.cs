using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using HidSharp;
using Aurora.Utils;

namespace Aurora.Devices.Ducky
{
    class DuckyDevice : Device
    {
        private static readonly int PACKET_NUM = 0;
        private static readonly int OFFSET_NUM = 1;//These are helper numbers to make the code more readable.

        private Dictionary<DeviceKeys, int[]> packetOffsetMap = new Dictionary<DeviceKeys, int[]>
        {
            {DeviceKeys.ESC, new int[] {1, 24}},
            {DeviceKeys.TILDE, new int[] {1, 27}}, // also the JPN_HALFFULLWIDTH in the JIS layout.
            {DeviceKeys.TAB, new int[] {1, 30}},
            {DeviceKeys.CAPS_LOCK, new int[] {1, 33}},
            {DeviceKeys.LEFT_SHIFT, new int[] {1, 36}},
            {DeviceKeys.LEFT_CONTROL, new int[] {1, 39}},
          //{DeviceKeys., new int[] {1, 42}}, Probably nothing.
            {DeviceKeys.ONE, new int[] {1, 45}},
            {DeviceKeys.Q, new int[] {1, 48}},
            {DeviceKeys.A, new int[] {1, 51}},
          //{DeviceKeys.BACKSLASH_UK, new int[] {1, 54}}, This is almost certain to be the backslash/'<' key for ISO layouts.
            {DeviceKeys.LEFT_WINDOWS, new int[] {1,57}},
            {DeviceKeys.F1, new int[] {1,60}},
            {DeviceKeys.TWO, new int[] {1, 63}},

            {DeviceKeys.W, new int[] {2, 6}},
            {DeviceKeys.S, new int[] {2, 9}},
            {DeviceKeys.Z, new int[] {2, 12}},
            {DeviceKeys.LEFT_ALT, new int[] {2, 15}},
            {DeviceKeys.F2, new int[] {2, 18}},
            {DeviceKeys.THREE, new int[] {2, 21}},
            {DeviceKeys.E, new int[] {2, 24}},
            {DeviceKeys.D, new int[] {2, 27}},
            {DeviceKeys.X, new int[] {2, 30}},
          //{DeviceKeys.????, new int[] {2, 33}}, Possibly the JPN_MUHENKAN key for JIS keyboard layout.
            {DeviceKeys.F3, new int[] {2, 36}},
            {DeviceKeys.FOUR, new int[] {2, 39}},
            {DeviceKeys.R, new int[] {2, 42}},
            {DeviceKeys.F, new int[] {2, 45}},
            {DeviceKeys.C, new int[] {2, 48}},
          //{DeviceKeys.????, new int[] {2, 51}}, Don't have a clue.
            {DeviceKeys.F4, new int[] {2, 54}},
            {DeviceKeys.FIVE, new int[] {2, 57}},
            {DeviceKeys.T, new int[] {2, 60}},
            {DeviceKeys.G, new int[] {2, 63}},

            {DeviceKeys.V, new int[] {3, 6}},
          //{DeviceKeys., new int[] {3, 9}},
          //{DeviceKeys., new int[] {3, 12}}, These two are probably nothing.
            {DeviceKeys.SIX, new int[] {3, 15}},
            {DeviceKeys.Y, new int[] {3, 18}},
            {DeviceKeys.H, new int[] {3, 21}},
            {DeviceKeys.B, new int[] {3, 24}},
            {DeviceKeys.SPACE, new int[] {3, 27}},
            {DeviceKeys.F5, new int[] {3, 30}},
            {DeviceKeys.SEVEN, new int[] {3, 33}},
            {DeviceKeys.U, new int[] {3, 36}},
            {DeviceKeys.J, new int[] {3, 39}},
            {DeviceKeys.N, new int[] {3, 42}},
          //{DeviceKeys.????, new int[] {3, 45}}, Probably nothing. could very unlikely be JPN_HENKAN.
            {DeviceKeys.F6, new int[] {3, 48}},
            {DeviceKeys.EIGHT, new int[] {3, 51}},
            {DeviceKeys.I, new int[] {3, 54}},
            {DeviceKeys.K, new int[] {3, 57}},
            {DeviceKeys.M, new int[] {3, 60}},
          //{DeviceKeys.????, new int[] {3, 63}}, Could be the JPN_HENKAN key. (more likely than the one at {3,45})

            {DeviceKeys.F7, new int[] {4, 6}},
            {DeviceKeys.NINE, new int[] {4, 9}},
            {DeviceKeys.O, new int[] {4, 12}},
            {DeviceKeys.L, new int[] {4, 15}},
            {DeviceKeys.COMMA, new int[] {4, 18}},
          //{DeviceKeys.????, new int[] {4, 21}}, Could be the JPN_HIRAGANA_KATAKANA key.
            {DeviceKeys.F8, new int[] {4, 24}}, 
            {DeviceKeys.ZERO, new int[] {4, 27}},
            {DeviceKeys.P, new int[] {4, 30}},
            {DeviceKeys.SEMICOLON, new int[] {4, 33}}, // Could be different depending on what ISO layout you have (scandanavians have UmlautO here, UK stays the same)
            {DeviceKeys.PERIOD, new int[] {4, 36}},
            {DeviceKeys.RIGHT_ALT, new int[] {4, 39}},
            {DeviceKeys.F9, new int[] {4, 42}},
            {DeviceKeys.MINUS, new int[] {4, 45}}, //some ISO layouts might have minus and equals swapped... Why tho.
            {DeviceKeys.OPEN_BRACKET, new int[] {4, 48}}, // Could be different depending on what ISO layout you have (scandanavians have TittleA here, UK stays the same)
            {DeviceKeys.APOSTROPHE, new int[] {4, 51}}, // Could be different depending on what ISO layout you have (scandanavians have UmlautA here, UK has other stuff)
            {DeviceKeys.FORWARD_SLASH, new int[] {4, 54}}, //some ISO layouts have minus here.
          //{DeviceKeys.????, new int[] {4, 57}}, Don't know.
            {DeviceKeys.F10, new int[] {4, 60}},
            {DeviceKeys.EQUALS, new int[] {4, 63}}, //some ISO layouts have Accute Accent or minus here.

            {DeviceKeys.CLOSE_BRACKET, new int[] {5, 6}}, // Some ISO layouts have this as another Umlaut key
          //{DeviceKeys., new int[] {5, 9}}, Could be the " ' " (apostrphe) key in ISO.
          //{DeviceKeys., new int[] {5, 12}}, Probably nothing
            {DeviceKeys.RIGHT_WINDOWS, new int[] {5, 15}},
            {DeviceKeys.F11, new int[] {5, 18}},
          //{DeviceKeys., new int[] {5, 21}},
          //{DeviceKeys., new int[] {5, 24}},
          //{DeviceKeys., new int[] {5, 27}}, These three are probably nothing.
            {DeviceKeys.RIGHT_SHIFT, new int[] {5, 30}},
            {DeviceKeys.FN_Key, new int[] {5, 33}}, //The problem with this keyboard is there's dip switches on the back to move where the FN key is... This assumes default position.
            {DeviceKeys.F12, new int[] {5, 36}},
            {DeviceKeys.BACKSPACE, new int[] {5, 39}},
            {DeviceKeys.BACKSLASH, new int[] {5, 42}}, // ISO and JIS layouts don't have this key.
            {DeviceKeys.ENTER, new int[] {5, 45}},
          //{DeviceKeys., new int[] {5, 48}}, Very likely to be nothing.
            {DeviceKeys.RIGHT_CONTROL, new int[] {5, 51}},
            {DeviceKeys.PRINT_SCREEN, new int[] {5, 54}},
            {DeviceKeys.INSERT, new int[] {5, 57}},
            {DeviceKeys.DELETE, new int[] {5, 60}},
          //{DeviceKeys., new int[] {5, 63}}, Nothing.

          //{DeviceKeys., new int[] {6, 6}}, Also nothing.
            {DeviceKeys.ARROW_LEFT, new int[] {6, 9}},
            {DeviceKeys.SCROLL_LOCK, new int[] {6, 12}},
            {DeviceKeys.HOME, new int[] {6, 15}},
            {DeviceKeys.END, new int[] {6, 18}},
          //{DeviceKeys., new int[] {6, 21}}, Also nothing.
            {DeviceKeys.ARROW_UP, new int[] {6, 24}},
            {DeviceKeys.ARROW_DOWN, new int[] {6, 27}},
            {DeviceKeys.PAUSE_BREAK, new int[] {6, 30}},
            {DeviceKeys.PAGE_UP, new int[] {6, 33}},
            {DeviceKeys.PAGE_DOWN, new int[] {6, 36}},
          //{DeviceKeys., new int[] {6, 39}},
          //{DeviceKeys., new int[] {6, 42}}, Both are nothing.
            {DeviceKeys.ARROW_RIGHT, new int[] {6, 45}},
            {DeviceKeys.CALC, new int[] {6, 48}},
            {DeviceKeys.NUM_LOCK, new int[] {6, 51}},
            {DeviceKeys.NUM_SEVEN, new int[] {6, 54}},
            {DeviceKeys.NUM_FOUR, new int[] {6, 57}},
            {DeviceKeys.NUM_ONE, new int[] {6, 60}},
            {DeviceKeys.NUM_ZERO, new int[] {6, 63}},

            {DeviceKeys.VOLUME_MUTE, new int[] {7, 6}},
            {DeviceKeys.NUM_SLASH, new int[] {7, 9}},
            {DeviceKeys.NUM_EIGHT, new int[] {7, 12}},
            {DeviceKeys.NUM_FIVE, new int[] {7, 15}},
            {DeviceKeys.NUM_TWO, new int[] {7, 18}},
          //{DeviceKeys., new int[] {7, 21}}, Nothing
            {DeviceKeys.VOLUME_DOWN, new int[] {7, 24}},
            {DeviceKeys.NUM_ASTERISK, new int[] {7, 27}},
            {DeviceKeys.NUM_NINE, new int[] {7, 30}},
            {DeviceKeys.NUM_SIX, new int[] {7, 33}},
            {DeviceKeys.NUM_THREE, new int[] {7, 36}},
            {DeviceKeys.NUM_PERIOD, new int[] {7, 39}},
            {DeviceKeys.VOLUME_UP, new int[] {7, 42}},
            {DeviceKeys.NUM_MINUS, new int[] {7, 45}},
            {DeviceKeys.NUM_PLUS, new int[] {7, 48}},
            //{DeviceKeys., new int[] {7, 51}},
            //{DeviceKeys., new int[] {7, 54}}, Nothing for both.
            {DeviceKeys.NUM_ENTER, new int[] {7, 57}}
        };

        private static string deviceName = "Ducky";
        private bool isInitialized = false;
        private long lastUpdateTime = 0;
        private readonly Stopwatch watch = new Stopwatch();
        private Color processedColor;
        private int[] currentOffset;
        private bool writeSuccess;

        private DuckyRGBAPI duckyAPI = new DuckyRGBAPI();
        HidDevice Shine7Keyboard;
        HidStream packetStream;
        byte[] colourMessage = new byte[640];
        static byte[] startingPacket = { 0x56, 0x81, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0xAA, 0xAA, 0xAA, 0xAA };
        byte[] colourHeader = { 0x56, 0x83, 0x00 };

        public VariableRegistry GetRegisteredVariables()
        {
            return new VariableRegistry();
        }

        public string GetDeviceName()
        {
            return deviceName;
        }

        public string GetDeviceDetails()
        {
            return deviceName + ": " + (isInitialized ? "Connected" : "Not initialized");
        }

        public string GetDeviceUpdatePerformance()
        {
            return (isInitialized ? lastUpdateTime + " ms" : "");
        }

        public bool Initialize()
        {
            //Sets the initialize colour change packet
            startingPacket.CopyTo(colourMessage, Packet(0) + 1);
            //Headers for each colour packet
            for (byte i = 0; i < 8; i++)
            {
                colourHeader[2] = i;
                colourHeader.CopyTo(colourMessage, Packet(i + 1) + 1);
            }
            //First colour packet has extra data
            colourMessage[Packet(1) + 5] = 0x01;
            colourMessage[Packet(1) + 9] = 0x80;
            colourMessage[Packet(1) + 10] = 0x01;
            colourMessage[Packet(1) + 12] = 0xC1;
            colourMessage[Packet(1) + 17] = 0xFF;
            colourMessage[Packet(1) + 18] = 0xFF;
            colourMessage[Packet(1) + 19] = 0xFF;
            colourMessage[Packet(1) + 20] = 0xFF;
            //Sets terminate colour packet
            colourMessage[Packet(9) + 1] = 0x51;
            colourMessage[Packet(9) + 2] = 0x28;
            colourMessage[Packet(9) + 5] = 0xFF;

            Shine7Keyboard = DeviceList.Local.GetHidDevices(0x04D9, 0x0348).SingleOrDefault(HidDevice => HidDevice.GetMaxInputReportLength() == 65);
            try
            {
                isInitialized = Shine7Keyboard.TryOpen(out packetStream);
                //This uses a monstrous 501 packets to initialize the keyboard in to letting the LEDs be controlled over USB HID.
                foreach (byte[] controlPacket in duckyAPI.getControlCommand("Shine_7_Takeover"))
                {
                    packetStream.Write(controlPacket);
                }
            }
            catch
            {
                isInitialized = false;
            }
            
            return isInitialized;
        }

        public void Shutdown()
        {
            //This one is a little smaller, 81 packets. This tells the keyboard to no longer allow USB HID control of the LEDs.
            //You can tell both the takeover and release work because the keyboard will flash the same as switching to profile 1. (The same lights when you push FN + 1)
            foreach (byte[] controlPacket in duckyAPI.getControlCommand("Shine_7_Release"))
            {
                try
                {
                    packetStream.Write(controlPacket);
                }
                catch { }
            }
            packetStream.Dispose();
            packetStream.Close();
            isInitialized = false;
        }

        public void Reset()
        {
            this.Shutdown();
            this.Initialize();
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

        public bool IsKeyboardConnected()
        {
            return isInitialized;
        }

        public bool IsPeripheralConnected()
        {
            return isInitialized;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            foreach (KeyValuePair<DeviceKeys, Color> kc in keyColors)
            {
                //This keyboard doesn't take alpha (transparency) values, so we do this:
                processedColor = ColorUtils.CorrectWithAlpha(kc.Value);

                //This if statement grabs the packet offset from the key that Aurora wants to set, using packetOffsetMap.
                //It also checks whether the key exists in the Dictionary, and if not, doesn't try and set the key colour.
                if(!packetOffsetMap.TryGetValue(kc.Key, out currentOffset)){
                    continue;
                }
                //The colours are encoded using RGB bytes consecutively throughout the 10 packets, which are offset with packetOffsetMap.
                colourMessage[Packet(currentOffset[PACKET_NUM]) + currentOffset[OFFSET_NUM] + 1] = processedColor.R;
                //To account for the headers in the next packet, the offset is pushed a further four bytes (only required if the R byte starts on the last byte of a packet).
                if (currentOffset[OFFSET_NUM] == 63)
                {
                    colourMessage[Packet(currentOffset[PACKET_NUM]) + currentOffset[OFFSET_NUM] + 6] = processedColor.G;
                    colourMessage[Packet(currentOffset[PACKET_NUM]) + currentOffset[OFFSET_NUM] + 7] = processedColor.B;
                }
                else
                {
                    colourMessage[Packet(currentOffset[PACKET_NUM]) + currentOffset[OFFSET_NUM] + 2] = processedColor.G;
                    colourMessage[Packet(currentOffset[PACKET_NUM]) + currentOffset[OFFSET_NUM] + 3] = processedColor.B;
                }
            }

            //Everything previous to setting the colours actually just write the colour data to the ColourMessage byte array.
            /*
             The keyboard is only set up to change all key colours at once, using 10 USB HID packets. They consist of:
             One initializing packet
             Eight colour packets (although the eighth one isn't used at all)
             and one terminate packet
             
             These packets are 64 bytes each (technically 65 but the first byte is just padding, which is why there's the .Take(65) there)
             Each key has its own three bytes for r,g,b somewhere in the 8 colour packets. These positions are defined in the packetOffsetMap
             The colour packets also have a header. (You might be able to send these packets out of order, and the headers will tell the keyboard where it should be, but IDK)*/
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    packetStream.Write(colourMessage.Skip(Packet(i)).Take(65).ToArray());
                }
                catch
                {
                    Shutdown();
                    return false;
                }
            }
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

        private int Packet(int packetNum) => packetNum * 64;
    }
}
