using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Aurora.Devices.Clevo
{
    class ClevoDevice : Device
    {
        // Generic Variables
        private string devicename = "Clevo Keyboard";
        private bool isInitialized = false;

        // Settings
        // TODO: Theese settings could be implemented with posibility of configuration from the Aurora GUI (Or external JSON, INI, Settings, etc)
        private bool useGlobalPeriphericColors = false;
        private bool useTouchpad = true;

        // Clevo Controll Class
        private ClevoSetKBLED clevo = new ClevoSetKBLED();

        // Color Variables
        private Color ColorKBCenter = Color.Black;
        private Color ColorKBLeft = Color.Black;
        private Color ColorKBRight = Color.Black;
        private Color ColorTouchpad = Color.Black;
        private bool ColorUpdated;
        private Color LastColorKBCenter = Color.Black;
        private Color LastColorKBLeft = Color.Black;
        private Color LastColorKBRight = Color.Black;
        private Color LastColorTouchpad = Color.Black; 

        public string GetDeviceName()
        {
            return devicename;
        }

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                return devicename + ": Initialized";
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
                    // Initialize Clevo WMI Interface Connection
                    if (!clevo.Initialize())
                    {
                        Global.logger.LogLine("Could not connect to Clevo WMI Interface", Logging_Level.Info);
                        throw new Exception("Could not connect to Clevo WMI Interface");
                    }

                    // Mark Initialized = TRUE
                    isInitialized = true;
                    return true;
                }
                catch (Exception ex)
                {
                    Global.logger.LogLine("Clevo device, Exception! Message:" + ex, Logging_Level.Error);
                }

                // Mark Initialized = FALSE
                isInitialized = false;
                return false;
            }

            return isInitialized;

        }

        public void Shutdown()
        {
            if (this.IsInitialized())
            {
                clevo.ResetKBLEDColors();
                clevo.Release();
            }
        }

        public void Reset()
        {
            if (this.IsInitialized())
            {
                clevo.ResetKBLEDColors();
                clevo.Release();
                isInitialized = clevo.Initialize();
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

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced = false) // Is this necessary?
        {
            throw new NotImplementedException();
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, bool forced = false)
        {

            Dictionary<DeviceKeys, Color> keyColors = colorComposition.keyColors;
            try
            {
                foreach (KeyValuePair<DeviceKeys, Color> pair in keyColors)
                {
                    if (useGlobalPeriphericColors)
                    {
                        if (pair.Key == DeviceKeys.Peripheral) // This is not working anymore. Was working in MASTER
                        {
                            ColorKBLeft = pair.Value;
                            ColorKBCenter = pair.Value;
                            ColorKBRight = pair.Value;
                            ColorTouchpad = pair.Value;
                            ColorUpdated = true;
                        }
                    }
                    else
                    {
                        // TouchPad (It would be nice to have a Touchpad Peripheral)
                        if (pair.Key == DeviceKeys.Peripheral) {
                            ColorTouchpad = pair.Value;
                            ColorUpdated = true;
                        }
                    }
                }

                if (!useGlobalPeriphericColors) { // Clevo 3 region keyboard
                    // Left Side (From ESC to Half Spacebar)
                    BitmapRectangle keymap_esc = Effects.GetBitmappingFromDeviceKey(DeviceKeys.ESC);
                    BitmapRectangle keymap_space = Effects.GetBitmappingFromDeviceKey(DeviceKeys.SPACE);
                    PointF spacebar_center = keymap_space.Center; // Key Center
                    BitmapRectangle region_left = new BitmapRectangle(keymap_esc.Left, keymap_esc.Top, (int)spacebar_center.X - keymap_esc.Left, (int)spacebar_center.Y - keymap_esc.Top);
                    Color RegionLeftColor = Utils.BitmapUtils.GetRegionColor(colorComposition.keyBitmap, region_left);
                    if (!ColorKBLeft.Equals(RegionLeftColor)) {
                        ColorKBLeft = RegionLeftColor;
                        ColorUpdated = true;
                    }

                    // Center (Other Half of Spacebar to F11) - Clevo keyboards are very compact and the right side color bleeds over to the up/left/right/down keys)
                    BitmapRectangle keymap_f11 = Effects.GetBitmappingFromDeviceKey(DeviceKeys.F11);
                    BitmapRectangle region_center = new BitmapRectangle((int)spacebar_center.X - keymap_esc.Left, (int)spacebar_center.Y - keymap_esc.Top, keymap_f11.Left, keymap_f11.Top);
                    Color RegionCenterColor = Utils.BitmapUtils.GetRegionColor(colorComposition.keyBitmap, region_center);
                    if (!ColorKBCenter.Equals(RegionCenterColor))
                    {
                        ColorKBCenter = RegionCenterColor;
                        ColorUpdated = true;
                    }

                    // Right Side - F11 to CTRL_RIGHT (1/4)
                    BitmapRectangle keymap_ctrlright = Effects.GetBitmappingFromDeviceKey(DeviceKeys.RIGHT_CONTROL);
                    BitmapRectangle region_right = new BitmapRectangle(keymap_f11.Left, keymap_f11.Top, keymap_ctrlright.Right, keymap_ctrlright.Bottom);
                    Color RegionRightColor1 = Utils.BitmapUtils.GetRegionColor(colorComposition.keyBitmap, region_right);

                    // Right Side - NUMLOCK to NUMENTER (2/4)
                    BitmapRectangle keymap_numlock = Effects.GetBitmappingFromDeviceKey(DeviceKeys.NUM_LOCK);
                    BitmapRectangle keymap_numenter = Effects.GetBitmappingFromDeviceKey(DeviceKeys.NUM_ENTER);
                    region_right = new BitmapRectangle(keymap_numlock.Left, keymap_numlock.Top, keymap_numenter.Right, keymap_numenter.Bottom);
                    Color RegionRightColor2 = Utils.BitmapUtils.GetRegionColor(colorComposition.keyBitmap, region_right);

                    // Right Side - PRINTSCR to PAGEDOWN (3/4)
                    BitmapRectangle keymap_printscr = Effects.GetBitmappingFromDeviceKey(DeviceKeys.PRINT_SCREEN);
                    BitmapRectangle keymap_pagedown = Effects.GetBitmappingFromDeviceKey(DeviceKeys.PAGE_DOWN);
                    region_right = new BitmapRectangle(keymap_printscr.Left, keymap_printscr.Top, keymap_pagedown.Right, keymap_pagedown.Bottom);
                    Color RegionRightColor3 = Utils.BitmapUtils.GetRegionColor(colorComposition.keyBitmap, region_right);

                    // Right Side - Direction Keys (4/4)
                    // TODO: To be implemented

                    // Final Composition
                    Color RegionRightColor = ColorUtils.BlendColors(ColorUtils.BlendColors(RegionRightColor1, RegionRightColor2, 0.5), RegionRightColor3, 0.5);

                    if (!ColorKBRight.Equals(RegionRightColor))
                    {
                        ColorKBRight = RegionRightColor;
                        ColorUpdated = true;
                    }

                }

                SendColorsToKeyboard(forced);
                return true;
            }
            catch (Exception exception)
            {
                Global.logger.LogLine("Clevo device, error when updating device. Error: " + exception, Logging_Level.Error, true);
                Console.WriteLine(exception);
                return false;
            }
        }

        private void SendColorsToKeyboard(bool forced = false)
        {
            if (ColorUpdated)
            {
                if (forced || !LastColorKBLeft.Equals(ColorKBLeft))
                {
                    // MYSTERY: // Why is it B,R,G instead of R,G,B? SetKBLED uses R,G,B but only B,R,G returns the correct colors. Is bitshifting different in C# than in C++?
                    clevo.SetKBLED(ClevoSetKBLED.KBLEDAREA.ColorKBLeft, ColorKBLeft.B, ColorKBLeft.R, ColorKBLeft.G, (double)(ColorKBLeft.A / 0xff));
                    LastColorKBLeft = ColorKBLeft;
                }
                if (forced || !LastColorKBCenter.Equals(ColorKBCenter))
                {
                    clevo.SetKBLED(ClevoSetKBLED.KBLEDAREA.ColorKBCenter, ColorKBCenter.B, ColorKBCenter.R, ColorKBCenter.G, (double)(ColorKBCenter.A / 0xff));
                    LastColorKBCenter = ColorKBCenter;
                }
                if (forced || !LastColorKBRight.Equals(ColorKBRight))
                {
                    clevo.SetKBLED(ClevoSetKBLED.KBLEDAREA.ColorKBRight, ColorKBRight.B, ColorKBRight.R, ColorKBRight.G, (double)(ColorKBRight.A / 0xff));
                    LastColorKBRight = ColorKBRight;
                }
                if (forced || (useTouchpad && !LastColorTouchpad.Equals(ColorTouchpad)))
                {
                    clevo.SetKBLED(ClevoSetKBLED.KBLEDAREA.ColorTouchpad, ColorTouchpad.B, ColorTouchpad.R, ColorTouchpad.G, (double)(ColorTouchpad.A / 0xff));
                    LastColorTouchpad = ColorTouchpad;
                }
                ColorUpdated = false;
            }
        }

        // Device Status Methods
        public bool IsKeyboardConnected()
        {
            return isInitialized;
        }

        public bool IsPeripheralConnected()
        {
            return isInitialized;
        }
    }
}
