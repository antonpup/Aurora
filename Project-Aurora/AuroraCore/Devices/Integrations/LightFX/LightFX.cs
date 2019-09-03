using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Settings;
using System.ComponentModel;
using System.Runtime.InteropServices;
using MiscUtil.Linq.Extensions;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Net.Sockets;
using LightFXAPI;
using Aurora.Devices.Layout;
using Aurora.Devices.Layout.Layouts;
using LEDINT = System.Int16;

namespace Aurora.Devices.LightFX
{
    class LightFxDevice : Device
    {
        private String devicename = "LightFX";
        private bool isInitialized = false;

        private bool keyboard_updated = false;
        private bool peripheral_updated = false;
        uint DevNUM = 0;
        private VariableRegistry default_registry = null;

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

        public void Reset(int status)
        {
            if (usingHID) {
                byte[] Buffer = new byte[byteDataLength];
                Buffer[0] = 0x02;
                Buffer[1] = 0x07;
                Buffer[2] = (byte)status;
                bool read = LightFXSDK.HIDWrite(Buffer, Buffer.Length);
            }
        }

        int byteDataLength = 9;
        bool usingHID;
        public bool Initialize()
        {
            lock (action_lock) {
                if (!isInitialized) {
                    try {
                        int result = LightFXSDK.LightFXInitialize(0x187c);
                        if (result != -1) {
                            byteDataLength = result;
                            usingHID = true;
                        } else {
                            //Placeholder if in future, I need to use SDK instead of HID
                            /*
                            LFXInit();
                            */
                        }
                        /*
                        if (Global.Configuration.VarRegistry.GetVariable<bool>($"{devicename}_custom_pid")) {
                            int pid = Global.Configuration.VarRegistry.GetVariable<int>($"{devicename}_pid");

                            if (Global.Configuration.VarRegistry.GetVariable<bool>($"{devicename}_length"))
                                length = 12;
                            if (LightFXSDK.HIDInitialize(0x187c, pid)) {
                                usingHID = true;
                            }
                            Global.logger.Debug("PID: " + pid + " |Len: " + length);
                        } else if (LightFXSDK.HIDInitialize(0x187c, 0x511)) {
                            //AREA_51_M15X
                            usingHID = true;
                        } else if (LightFXSDK.HIDInitialize(0x187c, 0x512)) {
                            //ALL_POWERFULL_M15X/ALL_POWERFULL_M17X
                            usingHID = true;
                        } else if (LightFXSDK.HIDInitialize(0x187c, 0x514)) {
                            //ALL_POWERFULL_M11X
                            usingHID = true;
                        } else if (LightFXSDK.HIDInitialize(0x187c, 0x515)) {
                            //M15X
                            usingHID = true;
                        } else if (LightFXSDK.HIDInitialize(0x187c, 0x524)) {
                            //M17X
                            usingHID = true;
                        } else if (LightFXSDK.HIDInitialize(0x187c, 0x528)) {
                            //AW15R1/17R2
                            usingHID = true;
                        } else if (LightFXSDK.HIDInitialize(0x187c, 0x529)) {
                            //AW13R3
                            length = 12;
                            usingHID = true;
                        } else if (LightFXSDK.HIDInitialize(0x187c, 0x530)) {
                            //AW15R3/17R4
                            length = 12;
                            usingHID = true;
                        }else {
                            //Placeholder if in future, I need to use SDK instead of HID
                            //LFXInit();    
                        }*/
                        if (usingHID) {
                            AlienfxWaitForBusy();
                            Reset(0x03);
                            AlienfxWaitForReady();
                            setColor(1, (int)BITMASK.leftZone, 255, 255, 255);
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
                        if (usingHID) {
                            usingHID = false;
                            LightFXSDK.HIDClose();
                        }
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
                default_registry.Register($"{devicename}_custom_pid", false, "Use Custom PID");
                default_registry.Register($"{devicename}_pid", 0, "Device PID: 0x", flags: VariableFlags.UseHEX);
                default_registry.Register($"{devicename}_length", true, "Use 12 byte data");
            }
            return default_registry;
        }
        int ALIENFX_BUSY = 17;
        int ALIENFX_READY = 16;
        int ALIENFX_DEVICE_RESET = 6;

        public void LFXInit()
        {
            uint val = LightFXSDK.LFX_Initialize();
            Global.logger.Debug("LFX: " + val);
            if (val == LightFXSDK.LFX_SUCCESS)
            {
                LightFXSDK.LFX_Reset();
                LightFXSDK.LFX_SetTiming(40);
                uint numDevice = 0;
                isInitialized = true;
                usingHID = false;
                LightFXSDK.LFX_GetNumDevices(ref numDevice);
                /*
                for (uint devIndex = 0; devIndex < numDevices; devIndex++) {
                    uint descSize = 255;
                    char* description = new char[descSize];
                    byte devType = 0;
                    LightFXSDK.LFX_GetDeviceDescription(devIndex, description, descSize, &devType);
                } */
            }
        }

        public int deviceStatus()
        {
            if (usingHID)
            {

                byte[] Buffer = new byte[byteDataLength];

                Buffer[0] = 0x02;
                Buffer[1] = 0x06;


                bool write = LightFXSDK.HIDWrite(Buffer, Buffer.Length);
                Buffer[0] = 0x01;
                int status = LightFXSDK.HIDRead(Buffer, Buffer.Length);
                bool read = LightFXSDK.getReadStatus();
                if (!read)
                {
                    Global.logger.Info("Error Code: " + Marshal.GetLastWin32Error());
                    return -1;
                }

                // Global.logger.Info("Read Status: " + write + ": " + read);

                return status;
            }
            return -1;
            //Marshal.FreeHGlobal(unmanagedPointer);
        }

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

        public void setColor(byte index, int bitmask, byte r, byte g, byte b)
        {
            if (usingHID)
            {
                byte[] Buffer = new byte[byteDataLength];
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
                bool result = LightFXSDK.HIDWrite(Buffer, Buffer.Length);
                Loop();
            }
            //bool result2 = ExecuteColors();
            // Global.logger.Info("Color Status: " + result + ": " + result2);
            //ExecuteColors();
        }

        public void Loop()
        {
            if (usingHID)
            {
                byte[] Buffer = new byte[byteDataLength];
                Buffer[0] = 0x02;
                Buffer[1] = 0x04;
                bool result = LightFXSDK.HIDWrite(Buffer, Buffer.Length);
            }
        }

        public bool ExecuteColors()
        {
            if (usingHID)
            {

                byte[] Buffer = new byte[byteDataLength];
                Buffer[0] = 0x02;
                Buffer[1] = 0x05;

                bool result = LightFXSDK.HIDWrite(Buffer, Buffer.Length);
                return result;
            }
            return false;
        }

        KeyboardKeys[] leftZoneKeys = {KeyboardKeys.ESC , KeyboardKeys.TAB , KeyboardKeys.LEFT_FN, KeyboardKeys.CAPS_LOCK, KeyboardKeys.LEFT_SHIFT,
                            KeyboardKeys.LEFT_CONTROL, KeyboardKeys.LEFT_FN , KeyboardKeys.F1 , KeyboardKeys.ONE , KeyboardKeys.TWO  ,KeyboardKeys.Q,
                            KeyboardKeys.A , KeyboardKeys.Z,  KeyboardKeys.LEFT_WINDOWS,  KeyboardKeys.F2 , KeyboardKeys.TILDE,
                            KeyboardKeys.W , KeyboardKeys.E , KeyboardKeys.D  ,KeyboardKeys.S , KeyboardKeys.X, KeyboardKeys.LEFT_ALT,
                            KeyboardKeys.F3 ,KeyboardKeys.THREE };

        KeyboardKeys[] midLeftZoneKeys = {  KeyboardKeys.F4 ,  KeyboardKeys.FOUR ,  KeyboardKeys.C ,  KeyboardKeys.SPACE
                            ,  KeyboardKeys.F5 ,  KeyboardKeys.FIVE ,  KeyboardKeys.R ,  KeyboardKeys.F ,  KeyboardKeys.V
                            ,  KeyboardKeys.F6 ,  KeyboardKeys.SIX ,  KeyboardKeys.T ,  KeyboardKeys.G ,  KeyboardKeys.B
                           ,  KeyboardKeys.F7 ,  KeyboardKeys.SEVEN ,  KeyboardKeys.Y, KeyboardKeys.H ,  KeyboardKeys.N};

        KeyboardKeys[] midRightZoneKeys = { KeyboardKeys.F8 ,  KeyboardKeys.F9 ,  KeyboardKeys.F10 ,  KeyboardKeys.F11 ,  KeyboardKeys.F12 ,  KeyboardKeys.HOME
                         ,  KeyboardKeys.EIGHT ,  KeyboardKeys.NINE ,  KeyboardKeys.ZERO ,  KeyboardKeys.MINUS
                             ,  KeyboardKeys.U ,  KeyboardKeys.I ,  KeyboardKeys.O ,  KeyboardKeys.P ,  KeyboardKeys.OPEN_BRACKET
                           ,  KeyboardKeys.J ,  KeyboardKeys.K ,  KeyboardKeys.L ,  KeyboardKeys.SEMICOLON ,  KeyboardKeys.APOSTROPHE
                         ,  KeyboardKeys.M ,  KeyboardKeys.COMMA ,  KeyboardKeys.PERIOD ,  KeyboardKeys.FORWARD_SLASH
                             ,  KeyboardKeys.RIGHT_CONTROL ,  KeyboardKeys.RIGHT_ALT };

        KeyboardKeys[] rightZoneKeys = { KeyboardKeys.END ,  KeyboardKeys.DELETE ,  KeyboardKeys.BACKSPACE ,  KeyboardKeys.BACKSLASH
                                      ,  KeyboardKeys.RIGHT_SHIFT ,  KeyboardKeys.ARROW_UP ,  KeyboardKeys.ARROW_DOWN
                                     ,  KeyboardKeys.ARROW_RIGHT ,  KeyboardKeys.ARROW_LEFT ,  KeyboardKeys.ENTER ,  KeyboardKeys.PAGE_DOWN
                                     ,  KeyboardKeys.PAGE_UP ,  KeyboardKeys.PAGE_DOWN ,  KeyboardKeys.CLOSE_BRACKET };

        KeyboardKeys[] numpadZone = { KeyboardKeys.NUM_ONE ,  KeyboardKeys.NUM_TWO ,  KeyboardKeys.NUM_THREE ,  KeyboardKeys.NUM_FOUR
                                      ,  KeyboardKeys.NUM_FIVE ,  KeyboardKeys.NUM_SIX ,  KeyboardKeys.NUM_SEVEN
                                     ,  KeyboardKeys.NUM_EIGHT ,  KeyboardKeys.NUM_NINE ,  KeyboardKeys.NUM_ZERO ,  KeyboardKeys.NUM_PERIOD
                                     ,  KeyboardKeys.NUM_LOCK ,  KeyboardKeys.NUM_ENTER ,  KeyboardKeys.NUM_ASTERISK, KeyboardKeys.NUM_SLASH};


        bool NumLock = (((ushort)LightFXSDK.GetKeyState(0x90)) & 0xffff) != 0;

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
                Global.logger.Error("LightFX device, error when updating device: " + ex);
                return false;
            }

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return updateResult;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel) return false;
            List<Color> leftColor = new List<Color>();

            List<Color> midleftColor = new List<Color>();


            List<Color> midRightColor = new List<Color>();

            List<Color> rightColor = new List<Color>();

            List<Color> numpadColor = new List<Color>();


            try {
                if (e.Cancel) return false;

                LightFXSDK.ResetColors();

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
                            return false;
                        }
                    }

                } else {
                    LightFXSDK.LFX_Reset();
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

                        if (Array.Exists(numpadZone, s => s == key.Key) && (key.Value.R > 0 || key.Value.G > 0 || key.Value.B > 0)) {

                            numpadColor.Add(key.Value);

                        }

                        if (key.Key == DeviceKeys.Peripheral_Logo) {

                            setColor(1, (int)BITMASK.AlienFrontLogo, key.Value.R, key.Value.G, key.Value.B);
                            if (!usingHID) {
                                LightFXSDK.color.SetRGB(key.Value.R, key.Value.G, key.Value.B);

                                LightFXSDK.LFX_SetLightColor(1, 5, ref LightFXSDK.color);
                            }
                        }
                    }

                }

                if (NumLock) {
                    midRightColor.AddRange(rightColor);
                    rightColor.Clear();
                    rightColor = numpadColor;
                }
                if (leftColor.Any()) {
                    var mostUsed = leftColor.GroupBy(item => item)
                                     .OrderByDescending(item => item.Count())
                                     .Select(item => new { Color = item.Key, Count = item.Count() })
                                     .First();
                    LightFXSDK.color4.SetRGB(mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    setColor(3, (int)BITMASK.LeftPanelTop, mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    setColor(4, (int)BITMASK.leftZone, mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    if (!usingHID) {

                        LightFXSDK.LFX_SetLightColor(1, 0, ref LightFXSDK.color4);
                    }

                } else {
                    setColor(3, (int)BITMASK.LeftPanelTop, 0, 0, 0);
                    setColor(4, (int)BITMASK.leftZone, 0, 0, 0);
                    if (!usingHID) {
                        LightFXSDK.color1.brightness = 0;
                        LightFXSDK.LFX_SetLightColor(1, 0, ref LightFXSDK.color1);
                    }
                }

                if (midleftColor.Any()) {
                    // Global.logger.Debug("Updating midleftColor");
                    var mostUsed = midleftColor.GroupBy(item => item).OrderByDescending(item => item.Count())
                                    .Select(item => new { Color = item.Key, Count = item.Count() })
                                    .First();
                    LightFXSDK.color3.SetRGB(mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    setColor(5, (int)BITMASK.LeftPanelBottom, mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    setColor(6, (int)BITMASK.leftMiddleZone, mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    if (!usingHID) {
                        LightFXSDK.LFX_SetLightColor(1, 2, ref LightFXSDK.color3);
                    }
                    //Global.logger.Info("Mid Left Codes: " + color3.red + " : " + color3.green + " : " + color3.blue);
                } else {
                    setColor(5, (int)BITMASK.LeftPanelBottom, 0, 0, 0);
                    setColor(6, (int)BITMASK.leftMiddleZone, 0, 0, 0);
                    if (!usingHID) {
                        LightFXSDK.color3.brightness = 0;
                        LightFXSDK.LFX_SetLightColor(1, 2, ref LightFXSDK.color3);
                    }
                }


                if (rightColor.Any()) {
                    var mostUsed = rightColor.GroupBy(item => item)
                                       .OrderByDescending(item => item.Count())
                                       .Select(item => new { Color = item.Key, Count = item.Count() })
                                       .First();
                    LightFXSDK.color1.SetRGB(mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    setColor(7, (int)BITMASK.RightPanelTop, mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    setColor(8, (int)BITMASK.rightZone, mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    if (!usingHID) {
                        LightFXSDK.LFX_SetLightColor(1, 3, ref LightFXSDK.color1);
                    }
                } else {
                    if (!usingHID) {
                        LightFXSDK.color1.brightness = 0;
                        LightFXSDK.LFX_SetLightColor(1, 3, ref LightFXSDK.color1);
                    }
                    setColor(7, (int)BITMASK.RightPanelTop, 0, 0, 0);
                    setColor(8, (int)BITMASK.rightZone, 0, 0, 0);
                }

                if (midRightColor.Any()) {
                    var mostUsed = midRightColor.GroupBy(item => item)
                                    .OrderByDescending(item => item.Count())
                                    .Select(item => new { Color = item.Key, Count = item.Count() })
                                    .First();

                    LightFXSDK.color2.SetRGB(mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    setColor(9, (int)BITMASK.RightPanelBottom, mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    setColor(10, (int)BITMASK.rightMiddleZone, mostUsed.Color.R, mostUsed.Color.G, mostUsed.Color.B);
                    if (!usingHID) {
                        LightFXSDK.LFX_SetLightColor(1, 4, ref LightFXSDK.color2);
                    }
                    //Global.logger.Info("Mid Right Codes: " + color2.red + " : " + color2.green + " : " + color2.blue);
                } else {
                    if (!usingHID) {
                        LightFXSDK.color1.brightness = 0;
                        LightFXSDK.LFX_SetLightColor(1, 4, ref LightFXSDK.color1);
                    }
                    setColor(9, (int)BITMASK.RightPanelBottom, 0, 0, 0);
                    setColor(10, (int)BITMASK.rightMiddleZone, 0, 0, 0);
                }
                // setColor(1, TouchPad, color5.red, color5.green, color5.blue);

                ExecuteColors();
                if (!usingHID)
                    LightFXSDK.LFX_Update();
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


    }
}