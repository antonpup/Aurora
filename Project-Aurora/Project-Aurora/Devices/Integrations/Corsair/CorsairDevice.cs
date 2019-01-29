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
using Aurora.Settings;
using Microsoft.Win32.TaskScheduler;
using System.ComponentModel;

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

        private void SendColorToMousepad(CorsairLedId zoneKey, Color color)
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
        }

        private void SendColorToPeripheral(Color color, bool forced = false)
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

                   /* if (mousemat != null && !Global.Configuration.devices_disable_mouse)
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
                    }*/

                    if (headsetstand != null && !Global.Configuration.devices_disable_headset)
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
        }

        private void SendColorToMouse(CorsairLedId ledid, Color color, bool forced = false)
        {
            if (Global.Configuration.devices_disable_mouse)
                return;

            if (Global.Configuration.allow_peripheral_devices)
            {
                //Apply and strip Alpha
                color = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));

                if (mouse != null)
                {
                    if (mouse[ledid] != null)
                        mouse[ledid].Color = color;

                    mouse.Update(true);
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
        }

        private CorsairLedId ToCorsair(DeviceKeys key)
        {
            switch (key)
            {
                case (DeviceKeys.LOGO):
                    return CorsairLedId.Logo;
                case (DeviceKeys.BRIGHTNESS_SWITCH):
                    return CorsairLedId.Brightness;
                case (DeviceKeys.LOCK_SWITCH):
                    return CorsairLedId.WinLock;

                case (DeviceKeys.VOLUME_MUTE):
                    return CorsairLedId.Mute;
                case (DeviceKeys.VOLUME_UP):
                    return CorsairLedId.VolumeUp;
                case (DeviceKeys.VOLUME_DOWN):
                    return CorsairLedId.VolumeDown;
                case (DeviceKeys.MEDIA_STOP):
                    return CorsairLedId.Stop;
                case (DeviceKeys.MEDIA_PLAY_PAUSE):
                    return CorsairLedId.PlayPause;
                case (DeviceKeys.MEDIA_PREVIOUS):
                    return CorsairLedId.ScanPreviousTrack;
                case (DeviceKeys.MEDIA_NEXT):
                    return CorsairLedId.ScanNextTrack;
                case (DeviceKeys.ESC):
                    return CorsairLedId.Escape;
                case (DeviceKeys.F1):
                    return CorsairLedId.F1;
                case (DeviceKeys.F2):
                    return CorsairLedId.F2;
                case (DeviceKeys.F3):
                    return CorsairLedId.F3;
                case (DeviceKeys.F4):
                    return CorsairLedId.F4;
                case (DeviceKeys.F5):
                    return CorsairLedId.F5;
                case (DeviceKeys.F6):
                    return CorsairLedId.F6;
                case (DeviceKeys.F7):
                    return CorsairLedId.F7;
                case (DeviceKeys.F8):
                    return CorsairLedId.F8;
                case (DeviceKeys.F9):
                    return CorsairLedId.F9;
                case (DeviceKeys.F10):
                    return CorsairLedId.F10;
                case (DeviceKeys.F11):
                    return CorsairLedId.F11;
                case (DeviceKeys.F12):
                    return CorsairLedId.F12;
                case (DeviceKeys.PRINT_SCREEN):
                    return CorsairLedId.PrintScreen;
                case (DeviceKeys.SCROLL_LOCK):
                    return CorsairLedId.ScrollLock;
                case (DeviceKeys.PAUSE_BREAK):
                    return CorsairLedId.PauseBreak;
                case (DeviceKeys.TILDE):
                    return CorsairLedId.GraveAccentAndTilde;
                case (DeviceKeys.ONE):
                    return CorsairLedId.D1;
                case (DeviceKeys.TWO):
                    return CorsairLedId.D2;
                case (DeviceKeys.THREE):
                    return CorsairLedId.D3;
                case (DeviceKeys.FOUR):
                    return CorsairLedId.D4;
                case (DeviceKeys.FIVE):
                    return CorsairLedId.D5;
                case (DeviceKeys.SIX):
                    return CorsairLedId.D6;
                case (DeviceKeys.SEVEN):
                    return CorsairLedId.D7;
                case (DeviceKeys.EIGHT):
                    return CorsairLedId.D8;
                case (DeviceKeys.NINE):
                    return CorsairLedId.D9;
                case (DeviceKeys.ZERO):
                    return CorsairLedId.D0;
                case (DeviceKeys.MINUS):
                    return CorsairLedId.MinusAndUnderscore;
                case (DeviceKeys.EQUALS):
                    return CorsairLedId.EqualsAndPlus;
                case (DeviceKeys.BACKSPACE):
                    return CorsairLedId.Backspace;
                case (DeviceKeys.INSERT):
                    return CorsairLedId.Insert;
                case (DeviceKeys.HOME):
                    return CorsairLedId.Home;
                case (DeviceKeys.PAGE_UP):
                    return CorsairLedId.PageUp;
                case (DeviceKeys.NUM_LOCK):
                    return CorsairLedId.NumLock;
                case (DeviceKeys.NUM_SLASH):
                    return CorsairLedId.KeypadSlash;
                case (DeviceKeys.NUM_ASTERISK):
                    return CorsairLedId.KeypadAsterisk;
                case (DeviceKeys.NUM_MINUS):
                    return CorsairLedId.KeypadMinus;
                case (DeviceKeys.TAB):
                    return CorsairLedId.Tab;
                case (DeviceKeys.Q):
                    return CorsairLedId.Q;
                case (DeviceKeys.W):
                    return CorsairLedId.W;
                case (DeviceKeys.E):
                    return CorsairLedId.E;
                case (DeviceKeys.R):
                    return CorsairLedId.R;
                case (DeviceKeys.T):
                    return CorsairLedId.T;
                case (DeviceKeys.Y):
                    return CorsairLedId.Y;
                case (DeviceKeys.U):
                    return CorsairLedId.U;
                case (DeviceKeys.I):
                    return CorsairLedId.I;
                case (DeviceKeys.O):
                    return CorsairLedId.O;
                case (DeviceKeys.P):
                    return CorsairLedId.P;
                case (DeviceKeys.OPEN_BRACKET):
                    return CorsairLedId.BracketLeft;
                case (DeviceKeys.CLOSE_BRACKET):
                    return CorsairLedId.BracketRight;
                case (DeviceKeys.BACKSLASH):
                    return CorsairLedId.Backslash;
                case (DeviceKeys.DELETE):
                    return CorsairLedId.Delete;
                case (DeviceKeys.END):
                    return CorsairLedId.End;
                case (DeviceKeys.PAGE_DOWN):
                    return CorsairLedId.PageDown;
                case (DeviceKeys.NUM_SEVEN):
                    return CorsairLedId.Keypad7;
                case (DeviceKeys.NUM_EIGHT):
                    return CorsairLedId.Keypad8;
                case (DeviceKeys.NUM_NINE):
                    return CorsairLedId.Keypad9;
                case (DeviceKeys.NUM_PLUS):
                    return CorsairLedId.KeypadPlus;
                case (DeviceKeys.CAPS_LOCK):
                    return CorsairLedId.CapsLock;
                case (DeviceKeys.A):
                    return CorsairLedId.A;
                case (DeviceKeys.S):
                    return CorsairLedId.S;
                case (DeviceKeys.D):
                    return CorsairLedId.D;
                case (DeviceKeys.F):
                    return CorsairLedId.F;
                case (DeviceKeys.G):
                    return CorsairLedId.G;
                case (DeviceKeys.H):
                    return CorsairLedId.H;
                case (DeviceKeys.J):
                    return CorsairLedId.J;
                case (DeviceKeys.K):
                    return CorsairLedId.K;
                case (DeviceKeys.L):
                    return CorsairLedId.L;
                case (DeviceKeys.SEMICOLON):
                    return CorsairLedId.SemicolonAndColon;
                case (DeviceKeys.APOSTROPHE):
                    return CorsairLedId.ApostropheAndDoubleQuote;
                case (DeviceKeys.HASHTAG):
                    return CorsairLedId.NonUsTilde;
                case (DeviceKeys.ENTER):
                    return CorsairLedId.Enter;
                case (DeviceKeys.NUM_FOUR):
                    return CorsairLedId.Keypad4;
                case (DeviceKeys.NUM_FIVE):
                    return CorsairLedId.Keypad5;
                case (DeviceKeys.NUM_SIX):
                    return CorsairLedId.Keypad6;
                case (DeviceKeys.LEFT_SHIFT):
                    return CorsairLedId.LeftShift;
                case (DeviceKeys.BACKSLASH_UK):
                    return CorsairLedId.NonUsBackslash;
                case (DeviceKeys.Z):
                    return CorsairLedId.Z;
                case (DeviceKeys.X):
                    return CorsairLedId.X;
                case (DeviceKeys.C):
                    return CorsairLedId.C;
                case (DeviceKeys.V):
                    return CorsairLedId.V;
                case (DeviceKeys.B):
                    return CorsairLedId.B;
                case (DeviceKeys.N):
                    return CorsairLedId.N;
                case (DeviceKeys.M):
                    return CorsairLedId.M;
                case (DeviceKeys.COMMA):
                    return CorsairLedId.CommaAndLessThan;
                case (DeviceKeys.PERIOD):
                    return CorsairLedId.PeriodAndBiggerThan;
                case (DeviceKeys.FORWARD_SLASH):
                    return CorsairLedId.SlashAndQuestionMark;
                case (DeviceKeys.OEM8):
                    return CorsairLedId.SlashAndQuestionMark;
                case (DeviceKeys.OEM102):
                    return CorsairLedId.International1;
                case (DeviceKeys.RIGHT_SHIFT):
                    return CorsairLedId.RightShift;
                case (DeviceKeys.ARROW_UP):
                    return CorsairLedId.UpArrow;
                case (DeviceKeys.NUM_ONE):
                    return CorsairLedId.Keypad1;
                case (DeviceKeys.NUM_TWO):
                    return CorsairLedId.Keypad2;
                case (DeviceKeys.NUM_THREE):
                    return CorsairLedId.Keypad3;
                case (DeviceKeys.NUM_ENTER):
                    return CorsairLedId.KeypadEnter;
                case (DeviceKeys.LEFT_CONTROL):
                    return CorsairLedId.LeftCtrl;
                case (DeviceKeys.LEFT_WINDOWS):
                    return CorsairLedId.LeftGui;
                case (DeviceKeys.LEFT_ALT):
                    return CorsairLedId.LeftAlt;
                case (DeviceKeys.SPACE):
                    return CorsairLedId.Space;
                case (DeviceKeys.RIGHT_ALT):
                    return CorsairLedId.RightAlt;
                case (DeviceKeys.RIGHT_WINDOWS):
                    return CorsairLedId.RightGui;
                case (DeviceKeys.APPLICATION_SELECT):
                    return CorsairLedId.Application;
                case (DeviceKeys.RIGHT_CONTROL):
                    return CorsairLedId.RightCtrl;
                case (DeviceKeys.ARROW_LEFT):
                    return CorsairLedId.LeftArrow;
                case (DeviceKeys.ARROW_DOWN):
                    return CorsairLedId.DownArrow;
                case (DeviceKeys.ARROW_RIGHT):
                    return CorsairLedId.RightArrow;
                case (DeviceKeys.NUM_ZERO):
                    return CorsairLedId.Keypad0;
                case (DeviceKeys.NUM_PERIOD):
                    return CorsairLedId.KeypadPeriodAndDelete;
                case (DeviceKeys.FN_Key):
                    return CorsairLedId.Fn;
                case (DeviceKeys.G1):
                    return CorsairLedId.G1;
                case (DeviceKeys.G2):
                    return CorsairLedId.G2;
                case (DeviceKeys.G3):
                    return CorsairLedId.G3;
                case (DeviceKeys.G4):
                    return CorsairLedId.G4;
                case (DeviceKeys.G5):
                    return CorsairLedId.G5;
                case (DeviceKeys.G6):
                    return CorsairLedId.G6;
                case (DeviceKeys.G7):
                    return CorsairLedId.G7;
                case (DeviceKeys.G8):
                    return CorsairLedId.G8;
                case (DeviceKeys.G9):
                    return CorsairLedId.G9;
                case (DeviceKeys.G10):
                    return CorsairLedId.G10;
                case (DeviceKeys.G11):
                    return CorsairLedId.G11;
                case (DeviceKeys.G12):
                    return CorsairLedId.G12;
                case (DeviceKeys.G13):
                    return CorsairLedId.G13;
                case (DeviceKeys.G14):
                    return CorsairLedId.G14;
                case (DeviceKeys.G15):
                    return CorsairLedId.G15;
                case (DeviceKeys.G16):
                    return CorsairLedId.G16;
                case (DeviceKeys.G17):
                    return CorsairLedId.G17;
                case (DeviceKeys.G18):
                    return CorsairLedId.G18;
                case (DeviceKeys.ADDITIONALLIGHT1):
                    return CorsairLedId.Lightbar1;
                case (DeviceKeys.ADDITIONALLIGHT2):
                    return CorsairLedId.Lightbar2;
                case (DeviceKeys.ADDITIONALLIGHT3):
                    return CorsairLedId.Lightbar3;
                case (DeviceKeys.ADDITIONALLIGHT4):
                    return CorsairLedId.Lightbar4;
                case (DeviceKeys.ADDITIONALLIGHT5):
                    return CorsairLedId.Lightbar5;
                case (DeviceKeys.ADDITIONALLIGHT6):
                    return CorsairLedId.Lightbar6;
                case (DeviceKeys.ADDITIONALLIGHT7):
                    return CorsairLedId.Lightbar7;
                case (DeviceKeys.ADDITIONALLIGHT8):
                    return CorsairLedId.Lightbar8;
                case (DeviceKeys.ADDITIONALLIGHT9):
                    return CorsairLedId.Lightbar9;
                case (DeviceKeys.ADDITIONALLIGHT10):
                    return CorsairLedId.Lightbar10;
                case (DeviceKeys.ADDITIONALLIGHT11):
                    return CorsairLedId.Lightbar11;
                case (DeviceKeys.ADDITIONALLIGHT12):
                    return CorsairLedId.Lightbar12;
                case (DeviceKeys.ADDITIONALLIGHT13):
                    return CorsairLedId.Lightbar13;
                case (DeviceKeys.ADDITIONALLIGHT14):
                    return CorsairLedId.Lightbar14;
                case (DeviceKeys.ADDITIONALLIGHT15):
                    return CorsairLedId.Lightbar15;
                case (DeviceKeys.ADDITIONALLIGHT16):
                    return CorsairLedId.Lightbar16;
                case (DeviceKeys.ADDITIONALLIGHT17):
                    return CorsairLedId.Lightbar17;
                case (DeviceKeys.ADDITIONALLIGHT18):
                    return CorsairLedId.Lightbar18;
                case (DeviceKeys.ADDITIONALLIGHT19):
                    return CorsairLedId.Lightbar19;
                default:
                    return CorsairLedId.Invalid;
            }
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
    }
}
