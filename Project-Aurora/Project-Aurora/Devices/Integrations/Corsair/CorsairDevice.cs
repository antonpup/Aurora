using CUE.NET;
using CUE.NET.Devices.Generic.Enums;
using CUE.NET.Devices.Headset;
using CUE.NET.Devices.Headset.Enums;
using CUE.NET.Devices.Keyboard;
using CUE.NET.Devices.Keyboard.Enums;
using CUE.NET.Devices.Mouse;
using CUE.NET.Devices.Mouse.Enums;
using CUE.NET.Devices.Mousemat;
using CUE.NET.Devices.HeadsetStand;
using CUE.NET.Exceptions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.TaskScheduler;
using System.ComponentModel;
using Aurora.Settings;
using Aurora.Devices.Layout;
using Aurora.Devices.Layout.Layouts;
using LEDINT = System.Int16;

namespace Aurora.Devices.Corsair
{
    class CorsairDevice : Device
    {
        private String devicename = "Corsair";
        private bool isInitialized = false;
        private bool wasInitializedOnce = false;

        private bool keyboard_updated = false;
        private bool peripheral_updated = false;

        CorsairKeyboard keyboard;
        CorsairMouse mouse;
        CorsairHeadset headset;
        CorsairMousemat mousemat;
        CorsairHeadsetStand headsetstand;

        private readonly object action_lock = new object();

        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

        //Previous data
        private Color previous_peripheral_Color = Color.Black;

        public string GetDeviceName()
        {
            return devicename;
        }

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                return devicename + ": " + (keyboard != null ? keyboard.DeviceInfo.Model + " " : "") + (mouse != null ? mouse.DeviceInfo.Model + " " : "") + (headset != null ? headset.DeviceInfo.Model + " " : "") + (mousemat != null ? mousemat.DeviceInfo.Model + " " : "");
            }
            else
            {
                return devicename + ": Not initialized";
            }
        }

        public bool Initialize()
        {
            lock (action_lock)
            {
                if (!isInitialized)
                {
                    try
                    {
                        if (wasInitializedOnce)
                            CueSDK.Reinitialize(true);
                        else
                            CueSDK.Initialize(true);

                        Global.logger.Info("Corsair device, Initialized with " + CueSDK.LoadedArchitecture + "-SDK");

                        keyboard = CueSDK.KeyboardSDK;
                        mouse = CueSDK.MouseSDK;
                        headset = CueSDK.HeadsetSDK;
                        mousemat = CueSDK.MousematSDK;
                        headsetstand = CueSDK.HeadsetStandSDK;
                        if (keyboard != null)
                            keyboard.Brush = (CUE.NET.Brushes.SolidColorBrush)Color.Transparent;
                        if (mouse != null)
                            mouse.Brush = (CUE.NET.Brushes.SolidColorBrush)Color.Transparent;
                        if (headset != null)
                            headset.Brush = (CUE.NET.Brushes.SolidColorBrush)Color.Transparent;
                        if (mousemat != null)
                            mousemat.Brush = (CUE.NET.Brushes.SolidColorBrush)Color.Transparent;
                        if (headsetstand != null)
                            headsetstand.Brush = (CUE.NET.Brushes.SolidColorBrush)Color.Transparent;


                        if (keyboard == null && mouse == null && headset == null && mousemat == null && headsetstand == null)
                            throw new WrapperException("No devices found");
                        else
                        {
                            if (Global.Configuration.corsair_first_time)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    CorsairInstallInstructions instructions = new CorsairInstallInstructions();
                                    instructions.ShowDialog();
                                });
                                Global.Configuration.corsair_first_time = false;
                                Settings.ConfigManager.Save(Global.Configuration);
                            }

                            //SaveLeds();
                            isInitialized = true;
                            wasInitializedOnce = true;
                            return true;
                        }
                    }
                    catch (CUEException ex)
                    {
                        Global.logger.Error("Corsair device, CUE Exception! ErrorCode: " + Enum.GetName(typeof(CorsairError), ex.Error));
                    }
                    catch (WrapperException ex)
                    {
                        Global.logger.Error("Corsair device, Wrapper Exception! Message: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Global.logger.Error("Corsair device, Exception! Message: " + ex);
                    }

                    isInitialized = false;
                    return false;
                }

                return isInitialized;
            }
        }

        public void Shutdown()
        {
            lock (action_lock)
            {
                try
                {
                    if (isInitialized)
                    {
                        this.Reset();
                        CueSDK.Reinitialize(false);
                        isInitialized = false;
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.Error("Corsair device, Exception during Shutdown. Message: " + exc);
                    isInitialized = false;
                }
            }
        }

        public void Reset()
        {
            if (this.IsInitialized() && (keyboard_updated || peripheral_updated))
            {
                //RestoreLeds();

                keyboard_updated = false;
                peripheral_updated = false;
            }
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
            return keyboard != null;
        }

        public bool IsPeripheralConnected()
        {
            return mouse != null;
        }

        public string GetDeviceUpdatePerformance()
        {
            return (isInitialized ? lastUpdateTime + " ms" : "");
        }

        public VariableRegistry GetRegisteredVariables()
        {
            return new VariableRegistry();
        }

        public bool UpdateDevice(Color GlobalColor, List<DeviceLayout> devices, DoWorkEventArgs e, bool forced = false)
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

                        // TODO: Mousepad
                    }
                }
            }
            catch (Exception ex)
            {
                Global.logger.Error("Corsair device, error when updating device: " + ex);
                return false;
            }

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return updateResult;
        }

        private bool UpdateDevice(KeyboardDeviceLayout keyboardDevice, DoWorkEventArgs e, bool forced)
        {
            if (e.Cancel) return false;
            //CorsairLedId keyindex = CorsairLedId.Invalid;

            try
            {
                if(keyboard != null)
                {
                    if (e.Cancel) return false;
                    foreach (KeyValuePair<LEDINT, Color> key in keyboardDevice.DeviceColours.deviceColours)
                    {
                        if (e.Cancel) return false;
                        CorsairLedId localKey = ToCorsair((KeyboardKeys)key.Key);

                        if (localKey != CorsairLedId.Invalid && keyboard[localKey] != null)
                            keyboard[localKey].Color = key.Value;

                        // pointless? nothing ever happens to this but assignment
                        //keyindex = localKey; 
                    }

                    if (e.Cancel) return false;
                    keyboard.Update(true);
                    keyboard_updated = true;
                    return true;
                }
                return false;
            }
            catch (Exception exc)
            {
                Global.logger.Error("Corsair device, error when updating device. Error: " + exc);
                Console.WriteLine(exc);
                return false;
            }
        }

        private bool UpdateDevice(MouseDeviceLayout mouseDevice, DoWorkEventArgs e, bool forced)
        {
            if (e.Cancel) return false;

            try
            {
                if (e.Cancel) return false;
                foreach (KeyValuePair<LEDINT, Color> key in mouseDevice.DeviceColours.deviceColours)
                {
                    if (e.Cancel) return false;

                    if ((MouseLights)key.Key == MouseLights.Peripheral_Logo)
                    {
                        SendColorToMouse(CorsairLedId.B1, (Color)(key.Value));
                        //SendColorToPeripheral((Color)(key.Value));
                    }
                    else if ((MouseLights)key.Key == MouseLights.Peripheral_FrontLight)
                    {
                        SendColorToMouse(CorsairLedId.B2, (Color)(key.Value));
                    }
                    else if ((MouseLights)key.Key == MouseLights.Peripheral_ScrollWheel)
                    {
                        SendColorToMouse(CorsairLedId.B3, (Color)(key.Value));
                        SendColorToMouse(CorsairLedId.B5, (Color)(key.Value));
                        SendColorToMouse(CorsairLedId.B6, (Color)(key.Value));
                    }
                }
                if (e.Cancel) return false;
                return true;
            }
            catch (Exception exc)
            {
                Global.logger.Error("Corsair device, error when updating device. Error: " + exc);
                Console.WriteLine(exc);
                return false;
            }
        }

        // TODO: Mousepad
/*
        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel) return false;
            CorsairLedId keyindex = CorsairLedId.Invalid;

            try
            {
                if (e.Cancel) return false;
                foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                {
                    if (e.Cancel) return false;
                    CorsairLedId localKey = ToCorsair(key.Key);

                    if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.Peripheral_Logo ||
                        localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.Peripheral)
                    {
                        SendColorToMouse(CorsairLedId.B1, (Color)(key.Value));
                        SendColorToPeripheral((Color)(key.Value));
                       
                    }
                    else if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.Peripheral_FrontLight)
                    {
                        SendColorToMouse(CorsairLedId.B2, (Color)(key.Value));
                    }
                    else if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.Peripheral_ScrollWheel)
                    {
                        SendColorToMouse(CorsairLedId.B3, (Color)(key.Value));
                        SendColorToMouse(CorsairLedId.B5, (Color)(key.Value));
                        SendColorToMouse(CorsairLedId.B6, (Color)(key.Value));
                    }
                    else if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.MOUSEPADLIGHT1)
                    {
                        SendColorToMousepad(CorsairLedId.Zone1, (Color)(key.Value));
                    }
                    else if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.MOUSEPADLIGHT2)
                    {
                        SendColorToMousepad(CorsairLedId.Zone2, (Color)(key.Value));
                    }
                    else if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.MOUSEPADLIGHT3)
                    {
                        SendColorToMousepad(CorsairLedId.Zone3, (Color)(key.Value));
                    }
                    else if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.MOUSEPADLIGHT4)
                    {
                        SendColorToMousepad(CorsairLedId.Zone4, (Color)(key.Value));
                    }
                    else if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.MOUSEPADLIGHT5)
                    {
                        SendColorToMousepad(CorsairLedId.Zone5, (Color)(key.Value));
                    }
                    else if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.MOUSEPADLIGHT6)
                    {
                        SendColorToMousepad(CorsairLedId.Zone6, (Color)(key.Value));
                    }
                    else if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.MOUSEPADLIGHT7)
                    {
                        SendColorToMousepad(CorsairLedId.Zone7, (Color)(key.Value));
                    }
                    else if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.MOUSEPADLIGHT8)
                    {
                        SendColorToMousepad(CorsairLedId.Zone8, (Color)(key.Value));
                    }
                    else if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.MOUSEPADLIGHT9)
                    {
                        SendColorToMousepad(CorsairLedId.Zone9, (Color)(key.Value));
                    }
                    else if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.MOUSEPADLIGHT10)
                    {
                        SendColorToMousepad(CorsairLedId.Zone10, (Color)(key.Value));
                    }
                    else if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.MOUSEPADLIGHT11)
                    {
                        SendColorToMousepad(CorsairLedId.Zone11, (Color)(key.Value));
                    }
                    else if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.MOUSEPADLIGHT12)
                    {
                        SendColorToMousepad(CorsairLedId.Zone12, (Color)(key.Value));
                    }
                    else if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.MOUSEPADLIGHT13)
                    {
                        SendColorToMousepad(CorsairLedId.Zone13, (Color)(key.Value));
                    }
                    else if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.MOUSEPADLIGHT14)
                    {
                        SendColorToMousepad(CorsairLedId.Zone14, (Color)(key.Value));
                    }
                    else if (localKey == CorsairLedId.Invalid && key.Key == DeviceKeys.MOUSEPADLIGHT15)
                    {
                        SendColorToMousepad(CorsairLedId.Zone15, (Color)(key.Value));
                    }
                    else if (localKey != CorsairLedId.Invalid)
                    {
                        SetOneKey(localKey, (Color)(key.Value));
                    }

                    keyindex = localKey;
                }

                if (e.Cancel) return false;
                SendColorsToKeyboard(forced);
                return true;
            }
            catch (Exception exc)
            {
                Global.logger.Error("Corsair device, error when updating device. Error: " + exc);
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
        
        private void SendColorsToKeyboard(bool forced = false)
        {
            if (keyboard != null && !Global.Configuration.devices_disable_keyboard)
            {
                keyboard.Update(true);
                keyboard_updated = true;
            }
        }

        private void SetOneKey(CorsairLedId localKey, Color color)
        {
            //Apply and strip Alpha
            color = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));
            if (keyboard != null && keyboard[localKey] != null)
                keyboard[localKey].Color = color;
        }
        */
        /*private void SendColorToMousepad(CorsairLedId zoneKey, Color color)
        {
            if (Global.Configuration.devices_disable_mouse)
                return;

            if (Global.Configuration.allow_peripheral_devices)
            {
                if (mousemat != null && !Global.Configuration.devices_disable_mouse)
            {
                if (mousemat[zoneKey] != null)
                    mousemat[zoneKey].Color = color;
                mousemat.Update(true);
            }
            peripheral_updated = true;
        }
            else
            {
                if (peripheral_updated)
                {
                    peripheral_updated = false;
                }
}
        }*/

        /*private void SendColorToPeripheral(Color color, bool forced = false)
        {
            if ((!previous_peripheral_Color.Equals(color) || forced))
            {
                if (Global.Configuration.allow_peripheral_devices)
                {
                    //Apply and strip Alpha
                    color = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));

                    if (mouse != null && !Global.Configuration.devices_disable_mouse)
                    {
                        if (mouse[CorsairLedId.B1] != null)
                            mouse[CorsairLedId.B1].Color = color;
                        if (mouse[CorsairLedId.B2] != null)
                            mouse[CorsairLedId.B2].Color = color;
                        if (mouse[CorsairLedId.B3] != null)
                            mouse[CorsairLedId.B3].Color = color;
                        if (mouse[CorsairLedId.B4] != null)
                            mouse[CorsairLedId.B4].Color = color;
                        if (mouse[CorsairLedId.B5] != null)
                            mouse[CorsairLedId.B5].Color = color;
                        if (mouse[CorsairLedId.B6] != null)
                            mouse[CorsairLedId.B6].Color = color;
                        mouse.Update(true);
                    }

                    if (headset != null && !Global.Configuration.devices_disable_headset)
                    {
                        if (headset[CorsairLedId.LeftLogo] != null)
                            headset[CorsairLedId.LeftLogo].Color = color;
                        if (headset[CorsairLedId.RightLogo] != null)
                            headset[CorsairLedId.RightLogo].Color = color;

                        headset.Update(true);
                    }

                   if (mousemat != null && !Global.Configuration.devices_disable_mouse)
                    {
                        if (mousemat[CorsairLedId.Zone1] != null)
                            mousemat[CorsairLedId.Zone1].Color = color;
                        if (mousemat[CorsairLedId.Zone2] != null)
                            mousemat[CorsairLedId.Zone2].Color = color;
                        if (mousemat[CorsairLedId.Zone3] != null)
                            mousemat[CorsairLedId.Zone3].Color = color;
                        if (mousemat[CorsairLedId.Zone4] != null)
                            mousemat[CorsairLedId.Zone4].Color = color;
                        if (mousemat[CorsairLedId.Zone5] != null)
                            mousemat[CorsairLedId.Zone5].Color = color;
                        if (mousemat[CorsairLedId.Zone6] != null)
                            mousemat[CorsairLedId.Zone6].Color = color;
                        if (mousemat[CorsairLedId.Zone7] != null)
                            mousemat[CorsairLedId.Zone7].Color = color;
                        if (mousemat[CorsairLedId.Zone8] != null)
                            mousemat[CorsairLedId.Zone8].Color = color;
                        if (mousemat[CorsairLedId.Zone9] != null)
                            mousemat[CorsairLedId.Zone9].Color = color;
                        if (mousemat[CorsairLedId.Zone10] != null)
                            mousemat[CorsairLedId.Zone10].Color = color;
                        if (mousemat[CorsairLedId.Zone11] != null)
                            mousemat[CorsairLedId.Zone11].Color = color;
                        if (mousemat[CorsairLedId.Zone12] != null)
                            mousemat[CorsairLedId.Zone12].Color = color;
                        if (mousemat[CorsairLedId.Zone13] != null)
                            mousemat[CorsairLedId.Zone13].Color = color;
                        if (mousemat[CorsairLedId.Zone14] != null)
                            mousemat[CorsairLedId.Zone14].Color = color;
                        if (mousemat[CorsairLedId.Zone15] != null)
                            mousemat[CorsairLedId.Zone15].Color = color;

                        mousemat.Update(true);
                    }

                    if (headsetstand != null)
                    {
                        if (headsetstand[CorsairLedId.HeadsetStandZone1] != null)
                            headsetstand[CorsairLedId.HeadsetStandZone1].Color = color;
                        if (headsetstand[CorsairLedId.HeadsetStandZone2] != null)
                            headsetstand[CorsairLedId.HeadsetStandZone2].Color = color;
                        if (headsetstand[CorsairLedId.HeadsetStandZone3] != null)
                            headsetstand[CorsairLedId.HeadsetStandZone3].Color = color;
                        if (headsetstand[CorsairLedId.HeadsetStandZone4] != null)
                            headsetstand[CorsairLedId.HeadsetStandZone4].Color = color;
                        if (headsetstand[CorsairLedId.HeadsetStandZone5] != null)
                            headsetstand[CorsairLedId.HeadsetStandZone5].Color = color;
                        if (headsetstand[CorsairLedId.HeadsetStandZone6] != null)
                            headsetstand[CorsairLedId.HeadsetStandZone6].Color = color;
                        if (headsetstand[CorsairLedId.HeadsetStandZone7] != null)
                            headsetstand[CorsairLedId.HeadsetStandZone7].Color = color;
                        if (headsetstand[CorsairLedId.HeadsetStandZone8] != null)
                            headsetstand[CorsairLedId.HeadsetStandZone8].Color = color;
                        if (headsetstand[CorsairLedId.HeadsetStandZone9] != null)
                            headsetstand[CorsairLedId.HeadsetStandZone9].Color = color;

                        headsetstand.Update(true);
                    }

                    previous_peripheral_Color = color;
                    peripheral_updated = true;
                }
                else
                {
                    if (peripheral_updated)
                    {
                        peripheral_updated = false;
                    }
                }
            }
        }*/

        private void SendColorToMouse(CorsairLedId ledid, Color color, bool forced = false)
        {
                //Apply and strip Alpha
                //color = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));

            if (mouse != null)
            {
                if (mouse[ledid] != null)
                    mouse[ledid].Color = color;

                mouse.Update(true);
                peripheral_updated = true;
            }
            else
            {
                if (peripheral_updated)
                {
                    peripheral_updated = false;
                }
            }
        }

        private CorsairLedId ToCorsair(KeyboardKeys key)
        {
            switch (key)
            {
                case (KeyboardKeys.LOGO):
                    return CorsairLedId.Logo;
                case (KeyboardKeys.BRIGHTNESS_SWITCH):
                    return CorsairLedId.Brightness;
                case (KeyboardKeys.LOCK_SWITCH):
                    return CorsairLedId.WinLock;

                case (KeyboardKeys.VOLUME_MUTE):
                    return CorsairLedId.Mute;
                case (KeyboardKeys.VOLUME_UP):
                    return CorsairLedId.VolumeUp;
                case (KeyboardKeys.VOLUME_DOWN):
                    return CorsairLedId.VolumeDown;
                case (KeyboardKeys.MEDIA_STOP):
                    return CorsairLedId.Stop;
                case (KeyboardKeys.MEDIA_PLAY_PAUSE):
                    return CorsairLedId.PlayPause;
                case (KeyboardKeys.MEDIA_PREVIOUS):
                    return CorsairLedId.ScanPreviousTrack;
                case (KeyboardKeys.MEDIA_NEXT):
                    return CorsairLedId.ScanNextTrack;
                case (KeyboardKeys.ESC):
                    return CorsairLedId.Escape;
                case (KeyboardKeys.F1):
                    return CorsairLedId.F1;
                case (KeyboardKeys.F2):
                    return CorsairLedId.F2;
                case (KeyboardKeys.F3):
                    return CorsairLedId.F3;
                case (KeyboardKeys.F4):
                    return CorsairLedId.F4;
                case (KeyboardKeys.F5):
                    return CorsairLedId.F5;
                case (KeyboardKeys.F6):
                    return CorsairLedId.F6;
                case (KeyboardKeys.F7):
                    return CorsairLedId.F7;
                case (KeyboardKeys.F8):
                    return CorsairLedId.F8;
                case (KeyboardKeys.F9):
                    return CorsairLedId.F9;
                case (KeyboardKeys.F10):
                    return CorsairLedId.F10;
                case (KeyboardKeys.F11):
                    return CorsairLedId.F11;
                case (KeyboardKeys.F12):
                    return CorsairLedId.F12;
                case (KeyboardKeys.PRINT_SCREEN):
                    return CorsairLedId.PrintScreen;
                case (KeyboardKeys.SCROLL_LOCK):
                    return CorsairLedId.ScrollLock;
                case (KeyboardKeys.PAUSE_BREAK):
                    return CorsairLedId.PauseBreak;
                case (KeyboardKeys.TILDE):
                    return CorsairLedId.GraveAccentAndTilde;
                case (KeyboardKeys.ONE):
                    return CorsairLedId.D1;
                case (KeyboardKeys.TWO):
                    return CorsairLedId.D2;
                case (KeyboardKeys.THREE):
                    return CorsairLedId.D3;
                case (KeyboardKeys.FOUR):
                    return CorsairLedId.D4;
                case (KeyboardKeys.FIVE):
                    return CorsairLedId.D5;
                case (KeyboardKeys.SIX):
                    return CorsairLedId.D6;
                case (KeyboardKeys.SEVEN):
                    return CorsairLedId.D7;
                case (KeyboardKeys.EIGHT):
                    return CorsairLedId.D8;
                case (KeyboardKeys.NINE):
                    return CorsairLedId.D9;
                case (KeyboardKeys.ZERO):
                    return CorsairLedId.D0;
                case (KeyboardKeys.MINUS):
                    return CorsairLedId.MinusAndUnderscore;
                case (KeyboardKeys.EQUALS):
                    return CorsairLedId.EqualsAndPlus;
                case (KeyboardKeys.BACKSPACE):
                    return CorsairLedId.Backspace;
                case (KeyboardKeys.INSERT):
                    return CorsairLedId.Insert;
                case (KeyboardKeys.HOME):
                    return CorsairLedId.Home;
                case (KeyboardKeys.PAGE_UP):
                    return CorsairLedId.PageUp;
                case (KeyboardKeys.NUM_LOCK):
                    return CorsairLedId.NumLock;
                case (KeyboardKeys.NUM_SLASH):
                    return CorsairLedId.KeypadSlash;
                case (KeyboardKeys.NUM_ASTERISK):
                    return CorsairLedId.KeypadAsterisk;
                case (KeyboardKeys.NUM_MINUS):
                    return CorsairLedId.KeypadMinus;
                case (KeyboardKeys.TAB):
                    return CorsairLedId.Tab;
                case (KeyboardKeys.Q):
                    return CorsairLedId.Q;
                case (KeyboardKeys.W):
                    return CorsairLedId.W;
                case (KeyboardKeys.E):
                    return CorsairLedId.E;
                case (KeyboardKeys.R):
                    return CorsairLedId.R;
                case (KeyboardKeys.T):
                    return CorsairLedId.T;
                case (KeyboardKeys.Y):
                    return CorsairLedId.Y;
                case (KeyboardKeys.U):
                    return CorsairLedId.U;
                case (KeyboardKeys.I):
                    return CorsairLedId.I;
                case (KeyboardKeys.O):
                    return CorsairLedId.O;
                case (KeyboardKeys.P):
                    return CorsairLedId.P;
                case (KeyboardKeys.OPEN_BRACKET):
                    return CorsairLedId.BracketLeft;
                case (KeyboardKeys.CLOSE_BRACKET):
                    return CorsairLedId.BracketRight;
                case (KeyboardKeys.BACKSLASH):
                    return CorsairLedId.Backslash;
                case (KeyboardKeys.DELETE):
                    return CorsairLedId.Delete;
                case (KeyboardKeys.END):
                    return CorsairLedId.End;
                case (KeyboardKeys.PAGE_DOWN):
                    return CorsairLedId.PageDown;
                case (KeyboardKeys.NUM_SEVEN):
                    return CorsairLedId.Keypad7;
                case (KeyboardKeys.NUM_EIGHT):
                    return CorsairLedId.Keypad8;
                case (KeyboardKeys.NUM_NINE):
                    return CorsairLedId.Keypad9;
                case (KeyboardKeys.NUM_PLUS):
                    return CorsairLedId.KeypadPlus;
                case (KeyboardKeys.CAPS_LOCK):
                    return CorsairLedId.CapsLock;
                case (KeyboardKeys.A):
                    return CorsairLedId.A;
                case (KeyboardKeys.S):
                    return CorsairLedId.S;
                case (KeyboardKeys.D):
                    return CorsairLedId.D;
                case (KeyboardKeys.F):
                    return CorsairLedId.F;
                case (KeyboardKeys.G):
                    return CorsairLedId.G;
                case (KeyboardKeys.H):
                    return CorsairLedId.H;
                case (KeyboardKeys.J):
                    return CorsairLedId.J;
                case (KeyboardKeys.K):
                    return CorsairLedId.K;
                case (KeyboardKeys.L):
                    return CorsairLedId.L;
                case (KeyboardKeys.SEMICOLON):
                    return CorsairLedId.SemicolonAndColon;
                case (KeyboardKeys.APOSTROPHE):
                    return CorsairLedId.ApostropheAndDoubleQuote;
                case (KeyboardKeys.HASH):
                    return CorsairLedId.NonUsTilde;
                case (KeyboardKeys.ENTER):
                    return CorsairLedId.Enter;
                case (KeyboardKeys.NUM_FOUR):
                    return CorsairLedId.Keypad4;
                case (KeyboardKeys.NUM_FIVE):
                    return CorsairLedId.Keypad5;
                case (KeyboardKeys.NUM_SIX):
                    return CorsairLedId.Keypad6;
                case (KeyboardKeys.LEFT_SHIFT):
                    return CorsairLedId.LeftShift;
                case (KeyboardKeys.BACKSLASH_UK):
                    return CorsairLedId.NonUsBackslash;
                case (KeyboardKeys.Z):
                    return CorsairLedId.Z;
                case (KeyboardKeys.X):
                    return CorsairLedId.X;
                case (KeyboardKeys.C):
                    return CorsairLedId.C;
                case (KeyboardKeys.V):
                    return CorsairLedId.V;
                case (KeyboardKeys.B):
                    return CorsairLedId.B;
                case (KeyboardKeys.N):
                    return CorsairLedId.N;
                case (KeyboardKeys.M):
                    return CorsairLedId.M;
                case (KeyboardKeys.COMMA):
                    return CorsairLedId.CommaAndLessThan;
                case (KeyboardKeys.PERIOD):
                    return CorsairLedId.PeriodAndBiggerThan;
                case (KeyboardKeys.FORWARD_SLASH):
                    return CorsairLedId.SlashAndQuestionMark;
                case (KeyboardKeys.OEM8):
                    return CorsairLedId.SlashAndQuestionMark;
                case (KeyboardKeys.OEM102):
                    return CorsairLedId.International1;
                case (KeyboardKeys.RIGHT_SHIFT):
                    return CorsairLedId.RightShift;
                case (KeyboardKeys.ARROW_UP):
                    return CorsairLedId.UpArrow;
                case (KeyboardKeys.NUM_ONE):
                    return CorsairLedId.Keypad1;
                case (KeyboardKeys.NUM_TWO):
                    return CorsairLedId.Keypad2;
                case (KeyboardKeys.NUM_THREE):
                    return CorsairLedId.Keypad3;
                case (KeyboardKeys.NUM_ENTER):
                    return CorsairLedId.KeypadEnter;
                case (KeyboardKeys.LEFT_CONTROL):
                    return CorsairLedId.LeftCtrl;
                case (KeyboardKeys.LEFT_WINDOWS):
                    return CorsairLedId.LeftGui;
                case (KeyboardKeys.LEFT_ALT):
                    return CorsairLedId.LeftAlt;
                case (KeyboardKeys.SPACE):
                    return CorsairLedId.Space;
                case (KeyboardKeys.RIGHT_ALT):
                    return CorsairLedId.RightAlt;
                case (KeyboardKeys.RIGHT_WINDOWS):
                    return CorsairLedId.RightGui;
                case (KeyboardKeys.APPLICATION_SELECT):
                    return CorsairLedId.Application;
                case (KeyboardKeys.RIGHT_CONTROL):
                    return CorsairLedId.RightCtrl;
                case (KeyboardKeys.ARROW_LEFT):
                    return CorsairLedId.LeftArrow;
                case (KeyboardKeys.ARROW_DOWN):
                    return CorsairLedId.DownArrow;
                case (KeyboardKeys.ARROW_RIGHT):
                    return CorsairLedId.RightArrow;
                case (KeyboardKeys.NUM_ZERO):
                    return CorsairLedId.Keypad0;
                case (KeyboardKeys.NUM_PERIOD):
                    return CorsairLedId.KeypadPeriodAndDelete;
                case (KeyboardKeys.FN_Key):
                    return CorsairLedId.Fn;
                case (KeyboardKeys.G1):
                    return CorsairLedId.G1;
                case (KeyboardKeys.G2):
                    return CorsairLedId.G2;
                case (KeyboardKeys.G3):
                    return CorsairLedId.G3;
                case (KeyboardKeys.G4):
                    return CorsairLedId.G4;
                case (KeyboardKeys.G5):
                    return CorsairLedId.G5;
                case (KeyboardKeys.G6):
                    return CorsairLedId.G6;
                case (KeyboardKeys.G7):
                    return CorsairLedId.G7;
                case (KeyboardKeys.G8):
                    return CorsairLedId.G8;
                case (KeyboardKeys.G9):
                    return CorsairLedId.G9;
                case (KeyboardKeys.G10):
                    return CorsairLedId.G10;
                case (KeyboardKeys.G11):
                    return CorsairLedId.G11;
                case (KeyboardKeys.G12):
                    return CorsairLedId.G12;
                case (KeyboardKeys.G13):
                    return CorsairLedId.G13;
                case (KeyboardKeys.G14):
                    return CorsairLedId.G14;
                case (KeyboardKeys.G15):
                    return CorsairLedId.G15;
                case (KeyboardKeys.G16):
                    return CorsairLedId.G16;
                case (KeyboardKeys.G17):
                    return CorsairLedId.G17;
                case (KeyboardKeys.G18):
                    return CorsairLedId.G18;
                case (KeyboardKeys.ADDITIONALLIGHT1):
                    return CorsairLedId.Lightbar1;
                case (KeyboardKeys.ADDITIONALLIGHT2):
                    return CorsairLedId.Lightbar2;
                case (KeyboardKeys.ADDITIONALLIGHT3):
                    return CorsairLedId.Lightbar3;
                case (KeyboardKeys.ADDITIONALLIGHT4):
                    return CorsairLedId.Lightbar4;
                case (KeyboardKeys.ADDITIONALLIGHT5):
                    return CorsairLedId.Lightbar5;
                case (KeyboardKeys.ADDITIONALLIGHT6):
                    return CorsairLedId.Lightbar6;
                case (KeyboardKeys.ADDITIONALLIGHT7):
                    return CorsairLedId.Lightbar7;
                case (KeyboardKeys.ADDITIONALLIGHT8):
                    return CorsairLedId.Lightbar8;
                case (KeyboardKeys.ADDITIONALLIGHT9):
                    return CorsairLedId.Lightbar9;
                case (KeyboardKeys.ADDITIONALLIGHT10):
                    return CorsairLedId.Lightbar10;
                case (KeyboardKeys.ADDITIONALLIGHT11):
                    return CorsairLedId.Lightbar11;
                case (KeyboardKeys.ADDITIONALLIGHT12):
                    return CorsairLedId.Lightbar12;
                case (KeyboardKeys.ADDITIONALLIGHT13):
                    return CorsairLedId.Lightbar13;
                case (KeyboardKeys.ADDITIONALLIGHT14):
                    return CorsairLedId.Lightbar14;
                case (KeyboardKeys.ADDITIONALLIGHT15):
                    return CorsairLedId.Lightbar15;
                case (KeyboardKeys.ADDITIONALLIGHT16):
                    return CorsairLedId.Lightbar16;
                case (KeyboardKeys.ADDITIONALLIGHT17):
                    return CorsairLedId.Lightbar17;
                case (KeyboardKeys.ADDITIONALLIGHT18):
                    return CorsairLedId.Lightbar18;
                case (KeyboardKeys.ADDITIONALLIGHT19):
                    return CorsairLedId.Lightbar19;
                default:
                    return CorsairLedId.Invalid;
            }
        }
    }
}
