using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Settings;
using HidLibrary;
using System.ComponentModel;
using System.Runtime.InteropServices;
using MiscUtil.Linq.Extensions;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Net.Sockets;

namespace Aurora.Devices.LightFX
{
    class LightFxDevice : Device
    {
        private String devicename = "LightFX";
        private bool isInitialized = false;

        private bool keyboard_updated = false;
        private bool peripheral_updated = false;
        uint DevNUM = 0;

        private int leftZone = 0x8;
        private int leftMiddleZone = 0x4;
        private int rightZone = 0x1;
        private int rightMiddleZone = 0x2;
        private int AlienFrontLogo = 0x40;
        private int AlienBackLogo = 0x20;
        private int LeftPanelTop = 0x1000;
        private int LeftPanelBottom = 0x400;
        private int RightPanelTop = 0x2000;
        private int RightPanelBottom = 0x800;
        private int TouchPad = 0x80;

        [StructLayout(LayoutKind.Sequential)]
        public struct LFX_COLOR
        {
            public byte red;
            public byte green;
            public byte blue;

            public byte brightness;
        };

        LFX_COLOR color, color1, color2, color3, color4, color5 = new LFX_COLOR();

        [DllImport("LightFX.dll")]
        public static extern uint LFX_Initialize();

        [DllImport("LightFX.dll")]
        public static extern uint LFX_Release();

        [DllImport("LightFX.dll")]
        public static extern uint LFX_Reset();

        [DllImport("LightFX.dll")]
        public static extern uint LFX_Update();

        [DllImport("LightFX.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_GetNumDevices(ref uint numDevices);

        [DllImport("LightFX.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_GetDeviceDescription(uint devIndex, ref char description, uint descSize, ref byte devtype);

        [DllImport("LightFX.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_SetTiming(int timing);

        [DllImport("LightFX.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_GetNumLights(uint devIndex, ref uint numLights);

        [DllImport("LightFX.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_GetLightColor(uint devIndex, uint lightIndex, ref LFX_COLOR color);

        [DllImport("LightFX.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint LFX_SetLightColor(uint devIndex, uint lightIndex, ref LFX_COLOR color);

        [DllImport("LightFX_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool HIDInitialize(int vid, int pid);

        [DllImport("LightFX_SDK.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool HIDClose();

        [DllImport("LightFX_SDK.dll")]
        public static extern int GetError();

        [DllImport("LightFX_SDK.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool getReadStatus();

        [DllImport("LightFX_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool HIDWrite(byte[] Buffer, int len);

        [DllImport("LightFX_SDK.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int HIDRead(byte[] Buffer, int len);



        public const int LFX_SUCCESS = 0;
        // Success

        public const int LFX_FAILURE = 1;
        // Generic failure

        public const int LFX_ERROR_NOINIT = 2;
        // System not initialized yet

        public const int LFX_ERROR_NODEVS = 3;
        // No devices available

        public const int LFX_ERROR_NOLIGHTS = 4;
        // No lights available

        public const int LFX_ERROR_BUFFSIZE = 5;
        // Buffer size too small

        public const int LFX_FIRSTEVENT = 0;
        // First event

        public const int LFX_NEXTEVENT = 1;
        // Next event

        public const int LFX_GAME = 0;
        // The application is a game

        public const int LFX_GENERALUSEAPP = 1;
        // It is a general use application

        private readonly object action_lock = new object();

        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

        public string GetDeviceName()
        {
            return devicename;
        }

        public string GetDeviceDetails()
        {
            if (isInitialized) {
                return devicename + ": Initialized";
            } else {
                return devicename + ": Not initialized";
            }
        }



        public int deviceStatus()
        {
            if (usingHID) {

                byte[] Buffer = new byte[length];

                Buffer[0] = 0x02;
                Buffer[1] = 0x06;


                bool write = HIDWrite(Buffer, Buffer.Length);
                Buffer[0] = 0x01;
                int status = HIDRead(Buffer, Buffer.Length);
                bool read = getReadStatus();
                if (!read) {
                    Global.logger.Info("Error Code: " + Marshal.GetLastWin32Error());
                    return -1;
                }

                // Global.logger.Info("Read Status: " + write + ": " + read);

                return status;
            }
            return -1;
            //Marshal.FreeHGlobal(unmanagedPointer);
        }

        public void setColor(byte index, int bitmask, byte r, byte g, byte b)
        {
            if (usingHID) {
                byte[] Buffer = new byte[length];
                Buffer[0] = 0x02;
                Buffer[1] = 0x03;
                Buffer[2] = index;
                Buffer[3] = (byte)((bitmask & 0xFF0000) >> 16);
                Buffer[4] = (byte)((bitmask & 0x00FF00) >> 8);
                Buffer[5] = (byte)((bitmask & 0x0000FF));
                Buffer[6] = r;
                Buffer[7] = g;
                Buffer[8] = b;
                // bool result = DeviceIoControl(devHandle, 0xb0195, Buffer, (uint)Buffer.Length, IntPtr.Zero, 0, ref writtenByteLength, IntPtr.Zero);
                bool result = HIDWrite(Buffer, Buffer.Length);
                Loop();
            }
            //bool result2 = ExecuteColors();
            // Global.logger.Info("Color Status: " + result + ": " + result2);
            //ExecuteColors();
        }

        public void Loop()
        {
            if (usingHID) {
                byte[] Buffer = new byte[length];
                Buffer[0] = 0x02;
                Buffer[1] = 0x04;
                bool result = HIDWrite(Buffer, Buffer.Length);
            }
        }

        public bool ExecuteColors()
        {
            if (usingHID) {

                byte[] Buffer = new byte[length];
                Buffer[0] = 0x02;
                Buffer[1] = 0x05;

                bool result = HIDWrite(Buffer, Buffer.Length);
                return result;
            }
            return false;
        }

        public void Reset(int status)
        {
            if (usingHID) {
                byte[] Buffer = new byte[length];
                Buffer[0] = 0x02;
                Buffer[1] = 0x07;
                Buffer[2] = (byte)status;
                bool read = HIDWrite(Buffer, Buffer.Length);
            }
        }

        int length = 9;
        bool usingHID;
        public bool Initialize()
        {
            lock (action_lock) {
                if (!isInitialized) {
                    try {
                        if (HIDInitialize(0x187c, 0x511)) {
                            //AREA_51_M15X
                            usingHID = true;
                        } else if (HIDInitialize(0x187c, 0x512)) {
                            //ALL_POWERFULL_M15X/ALL_POWERFULL_M17X
                            usingHID = true;
                        } else if (HIDInitialize(0x187c, 0x514)) {
                            //ALL_POWERFULL_M11X
                            usingHID = true;
                        } else if (HIDInitialize(0x187c, 0x528)) {
                            //AW15R1/17R2
                            usingHID = true;
                        } else if (HIDInitialize(0x187c, 0x529)) {
                            //AW13R3
                            length = 12;
                            usingHID = true;
                        } else if (HIDInitialize(0x187c, 0x530)) {
                            //AW15R3/17R4
                            length = 12;
                            usingHID = true;
                        } else {
                            //Placeholder if in future, I need to use SDK instead of HID
                            /*
                            uint val = LFX_Initialize();
                            Global.logger.Debug("LFX: " + val);
                            if (val == LFX_SUCCESS) {
                                LFX_Reset();
                                LFX_SetTiming(40);
                                uint numDevice = 0;
                                isInitialized = true;
                                usingHID = false;
                                LFX_GetNumDevices(ref numDevice);
                                for (uint devIndex = 0; devIndex < numDevices; devIndex++) {
                                    uint descSize = 255;
                                    char* description = new char[descSize];
                                    byte devType = 0;
                                    LFX_GetDeviceDescription(devIndex, description, descSize, &devType);
                                }
                              }*/
                        }
                        if (usingHID) {
                            AlienfxWaitForBusy();
                            Reset(0x03);
                            AlienfxWaitForReady();
                            setColor(1, leftZone, 255, 255, 255);
                            isInitialized = true;
                        }

                        return isInitialized;
                    } catch (Exception ex) {
                        Global.logger.Error("LIGHTFX device, Exception! Message: " + ex);
                        isInitialized = false;
                        return isInitialized;
                    }
                }
                return isInitialized;
            }
        }

        public void Shutdown()
        {
            lock (action_lock) {
                try {
                    if (isInitialized) {
                        this.Reset();
                        if (usingHID)
                            HIDClose();
                        //LFX_Release();
                        isInitialized = false;
                    }
                } catch (Exception exc) {
                    Global.logger.Error("LightFX device, Exception during Shutdown. Message: " + exc);
                    isInitialized = false;
                }
            }
        }

        public void Reset()
        {
            if (this.IsInitialized() && (keyboard_updated || peripheral_updated)) {
                keyboard_updated = false;
                peripheral_updated = false;
            }
        }

        public bool Reconnect()
        {
            Shutdown();
            Initialize();
            return isInitialized;
        }

        public bool IsInitialized()
        {
            return isInitialized;
        }

        public bool IsConnected()
        {
            return isInitialized;
        }
        int ALIENFX_BUSY = 17;
        int ALIENFX_READY = 16;
        int ALIENFX_DEVICE_RESET = 6;

        public int AlienfxWaitForBusy()
        {

            int status = deviceStatus();
            for (int i = 0; i < 5 && status != ALIENFX_BUSY; i++) {
                if (status == ALIENFX_DEVICE_RESET)
                    return status;
                Thread.Sleep(2);
                status = deviceStatus();
            }
            // Global.logger.Info("Wait Bytes: " + status);
            return status;
        }

        public int AlienfxWaitForReady()
        {

            int status = deviceStatus();
            for (int i = 0; i < 5 && status != ALIENFX_READY; i++) {
                if (status == ALIENFX_DEVICE_RESET)
                    return status;
                Thread.Sleep(2);
                status = deviceStatus();
            }
            //  Global.logger.Info("Ready Bytes: " + status);
            return status;
        }

        DeviceKeys[] leftZoneKeys = {DeviceKeys.ESC , DeviceKeys.TAB , DeviceKeys.LEFT_FN, DeviceKeys.CAPS_LOCK, DeviceKeys.LEFT_SHIFT,
                            DeviceKeys.LEFT_CONTROL, DeviceKeys.LEFT_FN , DeviceKeys.F1 , DeviceKeys.ONE , DeviceKeys.TWO  ,DeviceKeys.Q,
                            DeviceKeys.A , DeviceKeys.Z,  DeviceKeys.LEFT_WINDOWS,  DeviceKeys.F2 , DeviceKeys.TILDE,
                            DeviceKeys.W , DeviceKeys.E , DeviceKeys.D  ,DeviceKeys.S , DeviceKeys.X, DeviceKeys.LEFT_ALT,
                            DeviceKeys.F3 ,DeviceKeys.THREE };

        DeviceKeys[] midLeftZoneKeys = {  DeviceKeys.F4 ,  DeviceKeys.FOUR ,  DeviceKeys.C ,  DeviceKeys.SPACE
                            ,  DeviceKeys.F5 ,  DeviceKeys.FIVE ,  DeviceKeys.R ,  DeviceKeys.F ,  DeviceKeys.V
                            ,  DeviceKeys.F6 ,  DeviceKeys.SIX ,  DeviceKeys.T ,  DeviceKeys.G ,  DeviceKeys.B
                           ,  DeviceKeys.F7 ,  DeviceKeys.SEVEN ,  DeviceKeys.Y, DeviceKeys.H ,  DeviceKeys.N};

        DeviceKeys[] midRightZoneKeys = { DeviceKeys.F8 ,  DeviceKeys.F9 ,  DeviceKeys.F10 ,  DeviceKeys.F11 ,  DeviceKeys.F12 ,  DeviceKeys.HOME
                         ,  DeviceKeys.EIGHT ,  DeviceKeys.NINE ,  DeviceKeys.ZERO ,  DeviceKeys.MINUS
                             ,  DeviceKeys.U ,  DeviceKeys.I ,  DeviceKeys.O ,  DeviceKeys.P ,  DeviceKeys.OPEN_BRACKET
                           ,  DeviceKeys.J ,  DeviceKeys.K ,  DeviceKeys.L ,  DeviceKeys.SEMICOLON ,  DeviceKeys.APOSTROPHE
                         ,  DeviceKeys.M ,  DeviceKeys.COMMA ,  DeviceKeys.PERIOD ,  DeviceKeys.FORWARD_SLASH
                             ,  DeviceKeys.RIGHT_CONTROL ,  DeviceKeys.RIGHT_ALT };

        DeviceKeys[] rightZoneKeys = { DeviceKeys.END ,  DeviceKeys.DELETE ,  DeviceKeys.BACKSPACE ,  DeviceKeys.BACKSLASH
                                      ,  DeviceKeys.RIGHT_SHIFT ,  DeviceKeys.ARROW_UP ,  DeviceKeys.ARROW_DOWN
                                     ,  DeviceKeys.ARROW_RIGHT ,  DeviceKeys.ARROW_LEFT ,  DeviceKeys.ENTER ,  DeviceKeys.PAGE_DOWN
                                     ,  DeviceKeys.PAGE_UP ,  DeviceKeys.PAGE_DOWN ,  DeviceKeys.CLOSE_BRACKET };
        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel) return false;
            List<Color> leftColor = new List<Color>();

            List<Color> midleftColor = new List<Color>();


            List<Color> midRightColor = new List<Color>();

            List<Color> rightColor = new List<Color>();


            try {
                if (e.Cancel) return false;

                color1.green = 0;
                color1.blue = 0;
                color1.red = 0;
                color2.green = 0;
                color2.blue = 0;
                color2.red = 0;
                color3.green = 0;
                color3.blue = 0;
                color3.red = 0;
                color4.green = 0;
                color4.blue = 0;
                color4.red = 0;
                color5.green = 0;
                color5.blue = 0;
                color5.red = 0;

                if (usingHID) {
                    int status = AlienfxWaitForBusy();

                    if (status == ALIENFX_DEVICE_RESET) {
                        Thread.Sleep(1000);
                        // continue;
                        Global.logger.Debug("first reset false");
                        return false;
                        //AlienfxReinit();

                    } else if (status != ALIENFX_BUSY) {
                        Thread.Sleep(50);
                        // continue;
                        Global.logger.Debug("Not busy false");
                        //return false;
                    }
                    Reset(0x04);
                    Thread.Sleep(3);
                    status = AlienfxWaitForReady();
                    if (status == 6) {
                        Thread.Sleep(1000);
                        //AlienfxReinit();
                        // continue;
                        Global.logger.Debug("Reset false");
                        return false;
                    } else if (status != ALIENFX_READY) {
                        if (status == ALIENFX_BUSY) {
                            Reset(0x04);
                            Thread.Sleep(3);
                            status = AlienfxWaitForReady();
                            if (status == ALIENFX_DEVICE_RESET) {
                                Thread.Sleep(1000);
                                //AlienfxReinit();
                                // continue;
                                Global.logger.Debug("last Reset false");
                                return false;
                            }
                        } else {
                            Thread.Sleep(50);
                            // continue;
                            Global.logger.Debug("Else false");
                            return false;
                        }
                    }

                } else {
                    LFX_Reset();
                }
                foreach (KeyValuePair<DeviceKeys, Color> key in keyColors) {
                    if (e.Cancel) return false;
                    if (isInitialized) {
                        

                        //left
                        if (Array.Exists(leftZoneKeys, s => s == key.Key) && (key.Value.R > 0 || key.Value.G > 0 || key.Value.B > 0)) {
                           
                            leftColor.Add(key.Value);

                        } //middle left
                        if (Array.Exists(midLeftZoneKeys, s => s == key.Key) && (key.Value.R > 0 || key.Value.G > 0 || key.Value.B > 0)) {

                            midleftColor.Add(key.Value);

                        }//middle right
                        if (Array.Exists(midRightZoneKeys, s => s == key.Key) && (key.Value.R > 0 || key.Value.G > 0 || key.Value.B > 0)) {

                            midRightColor.Add(key.Value);

                        }//right */
                        if (Array.Exists(rightZoneKeys, s => s == key.Key) && (key.Value.R > 0 || key.Value.G > 0 || key.Value.B > 0)) {

                            rightColor.Add(key.Value);

                        }


                        if (key.Key == DeviceKeys.Peripheral_Logo) {

                            setColor(1, AlienFrontLogo, key.Value.R, key.Value.G, key.Value.B);
                            if (!usingHID) {
                                color.red = key.Value.R;
                                color.blue = key.Value.B;
                                color.green = key.Value.G;
                                color.brightness = 255;
                                LFX_SetLightColor(1, 5, ref color);
                            }
                        }
                    }




                }
                if (leftColor.Any()) {
                    var mostUsed = leftColor.GroupBy(item => item)
                                     .OrderByDescending(item => item.Count())
                                     .Select(item => new { Color = item.Key, Count = item.Count() })
                                     .First();
                    color4.red = mostUsed.Color.R;
                    color4.green = mostUsed.Color.G;
                    color4.blue = mostUsed.Color.B;
                    color4.brightness = 255;
                    setColor(3, LeftPanelTop, mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    setColor(4, leftZone, mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    if (!usingHID) {
                        LFX_SetLightColor(1, 0, ref color4);
                    }
                    //Global.logger.Info("Left Codes: " + color4.red + " : " + color4.green + " : " + color4.blue);
                } else {
                    setColor(3, LeftPanelTop, 0, 0, 0);
                    setColor(4, leftZone, 0, 0, 0);
                    if (!usingHID) {
                        color1.brightness = 0;
                        LFX_SetLightColor(1, 0, ref color1);
                    }
                }

                if (midleftColor.Any()) {
                    // Global.logger.Debug("Updating midleftColor");
                    var mostUsed = midleftColor.GroupBy(item => item).OrderByDescending(item => item.Count())
                                    .Select(item => new { Color = item.Key, Count = item.Count() })
                                    .First();
                    color3.red = mostUsed.Color.R;
                    color3.green = mostUsed.Color.G;
                    color3.blue = mostUsed.Color.B;
                    color3.brightness = 255;
                    setColor(5, LeftPanelBottom, mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    setColor(6, leftMiddleZone, mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    if (!usingHID) {

                        LFX_SetLightColor(1, 2, ref color3);
                    }
                    //Global.logger.Info("Mid Left Codes: " + color3.red + " : " + color3.green + " : " + color3.blue);
                } else {
                    setColor(5, LeftPanelBottom, 0, 0, 0);
                    setColor(6, leftMiddleZone, 0, 0, 0);
                    if (!usingHID) {
                        color3.brightness = 0;
                        LFX_SetLightColor(1, 2, ref color3);
                    }
                }


                if (rightColor.Any()) {
                    var mostUsed = rightColor.GroupBy(item => item)
                                       .OrderByDescending(item => item.Count())
                                       .Select(item => new { Color = item.Key, Count = item.Count() })
                                       .First();
                    color1.red = mostUsed.Color.R;
                    color1.green = mostUsed.Color.G;
                    color1.blue = mostUsed.Color.B;
                    color1.brightness = 255;
                    setColor(7, RightPanelTop, mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    setColor(8, rightZone, mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    if (!usingHID) {
                        LFX_SetLightColor(1, 3, ref color1);
                    }
                } else {
                    if (!usingHID) {
                        color1.brightness = 0;
                        LFX_SetLightColor(1, 3, ref color1);
                    }
                    setColor(7, RightPanelTop, 0, 0, 0);
                    setColor(8, rightZone, 0, 0, 0);
                }

                if (midRightColor.Any()) {
                    var mostUsed = midRightColor.GroupBy(item => item)
                                    .OrderByDescending(item => item.Count())
                                    .Select(item => new { Color = item.Key, Count = item.Count() })
                                    .First();

                    color2.red = mostUsed.Color.R;
                    color2.green = mostUsed.Color.G;
                    color2.blue = mostUsed.Color.B;
                    color2.brightness = 255;
                    setColor(9, RightPanelBottom, mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    setColor(10, rightMiddleZone, mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    if (!usingHID) {
                        LFX_SetLightColor(1, 4, ref color2);
                    }
                    //Global.logger.Info("Mid Right Codes: " + color2.red + " : " + color2.green + " : " + color2.blue);
                } else {
                    if (!usingHID) {
                        color1.brightness = 0;
                        LFX_SetLightColor(1, 4, ref color1);
                    }
                    setColor(9, RightPanelBottom, 0, 0, 0);
                    setColor(10, rightMiddleZone, 0, 0, 0);
                }
                // setColor(1, TouchPad, color5.red, color5.green, color5.blue);

                ExecuteColors();
                if (!usingHID)
                    LFX_Update();
                leftColor.Clear();
                midleftColor.Clear();
                rightColor.Clear();
                midRightColor.Clear();
                if (e.Cancel) return false;
                return true;
            } catch (Exception exc) {
                Global.logger.Error("LightFX device, error when updating device. Error: " + exc);
                Console.WriteLine(exc);
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
            return new VariableRegistry();
        }
    }
}