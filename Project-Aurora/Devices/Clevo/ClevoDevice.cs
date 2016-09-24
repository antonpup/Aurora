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
        private bool useGlobalPeriphericColors;
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
            // TODO: Implement reverse KB Colors to previous state
        }

        public void Reset()
        {
            if (this.IsInitialized())
            {
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

                    // Right Side (From F11 to NUMPAD ENTER)
                    BitmapRectangle keymap_num_enter = Effects.GetBitmappingFromDeviceKey(DeviceKeys.NUM_ENTER);
                    BitmapRectangle region_right = new BitmapRectangle(keymap_f11.Left, keymap_f11.Bottom, keymap_num_enter.Left, keymap_num_enter.Bottom);
                    Color RegionRightColor = Utils.BitmapUtils.GetRegionColor(colorComposition.keyBitmap, region_right);
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
