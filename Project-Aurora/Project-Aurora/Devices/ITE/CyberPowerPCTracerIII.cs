using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using Aurora.Settings;
using ITE.LightingModel;

namespace Aurora.Devices.ITE
{
    // ReSharper disable once InconsistentNaming
    public class CyberPowerPCTracerIII : Device
    {
        private const string DeviceName = "CyberpowerPC Tracer III keyboard";

        private readonly object _actionLock = new object();
        private bool _initialized = false;
        private KeyboardDriver driver = new KeyboardDriver();
        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;
        private RGBKB_Color _layoutColor = new RGBKB_Color(false, 6 * 21);

        public VariableRegistry GetRegisteredVariables()
        {
            return new VariableRegistry();
        }

        public string GetDeviceName()
        {
            return DeviceName;
        }

        public string GetDeviceDetails()
        {
            return _initialized ? $"{DeviceName}: Connected" : $"{DeviceName}: Not initialized";
        }

        public string GetDeviceUpdatePerformance()
        {
            return (_initialized ? lastUpdateTime + " ms" : "");
        }

        public bool Initialize()
        {
            lock (_actionLock)
            {
                if (!_initialized)
                {
                    _initialized = driver.Init();
                }

                return _initialized;
            }
        }

        public void Shutdown()
        {
            Reset();
            lock (_actionLock)
            {
                _initialized = false;
                driver.SetColor(_layoutColor.ColorBuffer, 50, 1, true);
            }
        }

        public void Reset()
        {
            _layoutColor = new RGBKB_Color(false, 6 * 21);
        }

        public bool Reconnect()
        {
            Shutdown();
            return Initialize();
        }

        public bool IsInitialized()
        {
            return _initialized;
        }

        public bool IsConnected()
        {
            return _initialized;
        }

        public bool IsKeyboardConnected()
        {
            return _initialized;
        }

        public bool IsPeripheralConnected()
        {
            return _initialized;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            try
            {
                lock (_actionLock)
                {
                    if (!_initialized)
                        return false;

                    foreach (var kv in keyColors)
                    {
                        var clr = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(kv.Value, kv.Value.A / 255.0D));

                        var keys = TranslateKey(kv.Key);

                        if (keys[0] == Key.Ignore)
                            continue;

                        foreach (var k in keys)
                        {
                            _layoutColor.ColorBuffer[(int)k].R = clr.R;
                            _layoutColor.ColorBuffer[(int)k].G = clr.G;
                            _layoutColor.ColorBuffer[(int)k].B = clr.B;
                        }
                    }

                    //if (e.Cancel) return false;

                    return driver.SetColor(_layoutColor.ColorBuffer, 50, 0, true, true);
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error("Failed to Update Device: " + exc.ToString());
                return false;
            }
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            var updateResult = UpdateDevice(colorComposition.keyColors, e, forced);

            watch.Stop();
            
            lastUpdateTime = watch.ElapsedMilliseconds;

            return updateResult;
        }

        private static readonly Dictionary<DeviceKeys, Key[]> KeyMap = new Dictionary<DeviceKeys, Key[]>
        {
            {DeviceKeys.ESC, new Key[]{Key.ESC}},
            {DeviceKeys.F1, new Key[]{Key.F1}},
            {DeviceKeys.F2, new Key[]{Key.F2}},
            {DeviceKeys.F3, new Key[]{Key.F3}},
            {DeviceKeys.F4, new Key[]{Key.F4}},
            {DeviceKeys.F5, new Key[]{Key.F5}},
            {DeviceKeys.F6, new Key[]{Key.F6}},
            {DeviceKeys.F7, new Key[]{Key.F7}},
            {DeviceKeys.F8, new Key[]{Key.F8}},
            {DeviceKeys.F9, new Key[]{Key.F9}},
            {DeviceKeys.F10, new Key[]{Key.F10}},
            {DeviceKeys.F11, new Key[]{Key.F11}},
            {DeviceKeys.F12, new Key[]{Key.F12}},
            {DeviceKeys.INSERT, new Key[]{Key.Insert}},
            {DeviceKeys.DELETE, new Key[]{Key.Del}},
            {DeviceKeys.HOME, new Key[]{Key.Home}},
            {DeviceKeys.END, new Key[]{Key.End}},
            {DeviceKeys.PAGE_UP, new Key[]{Key.PgUp}},
            {DeviceKeys.PAGE_DOWN, new Key[]{Key.PgDn}},
            {DeviceKeys.TILDE, new Key[]{Key.Tilda}},
            {DeviceKeys.ONE, new Key[]{Key._1}},
            {DeviceKeys.TWO, new Key[]{Key._2}},
            {DeviceKeys.THREE, new Key[]{Key._3}},
            {DeviceKeys.FOUR, new Key[]{Key._4}},
            {DeviceKeys.FIVE, new Key[]{Key._5}},
            {DeviceKeys.SIX, new Key[]{Key._6}},
            {DeviceKeys.SEVEN, new Key[]{Key._7}},
            {DeviceKeys.EIGHT, new Key[]{Key._8}},
            {DeviceKeys.NINE, new Key[]{Key._9}},
            {DeviceKeys.ZERO, new Key[]{Key._0}},
            {DeviceKeys.MINUS, new Key[]{Key.Minus}},
            {DeviceKeys.EQUALS, new Key[]{Key.Equals}},
            {DeviceKeys.BACKSPACE, new Key[]{Key.Backspace}},
            {DeviceKeys.NUM_LOCK, new Key[]{Key.NumLock}},
            {DeviceKeys.NUM_SLASH, new Key[]{Key.NumSlash}},
            {DeviceKeys.NUM_ASTERISK, new Key[]{Key.NumStar, Key.NumMinus}},
            {DeviceKeys.NUM_MINUS, new Key[]{Key.Ignore}},
            {DeviceKeys.TAB, new Key[]{Key.Tab}},
            {DeviceKeys.Q, new Key[]{Key.Q}},
            {DeviceKeys.W, new Key[]{Key.W}},
            {DeviceKeys.E, new Key[]{Key.E}},
            {DeviceKeys.R, new Key[]{Key.R}},
            {DeviceKeys.T, new Key[]{Key.T}},
            {DeviceKeys.Y, new Key[]{Key.Y}},
            {DeviceKeys.U, new Key[]{Key.U}},
            {DeviceKeys.I, new Key[]{Key.I}},
            {DeviceKeys.O, new Key[]{Key.O}},
            {DeviceKeys.P, new Key[]{Key.P}},
            {DeviceKeys.OPEN_BRACKET, new Key[]{Key.OpenSquareBracket}},
            {DeviceKeys.CLOSE_BRACKET, new Key[]{Key.ClosweSquareBracket}},
            {DeviceKeys.BACKSLASH, new Key[]{Key.Backslash}},
            {DeviceKeys.NUM_SEVEN, new Key[]{Key.Num7}},
            {DeviceKeys.NUM_EIGHT, new Key[]{Key.Num8}},
            {DeviceKeys.NUM_NINE, new Key[]{Key.Num9}},
            {DeviceKeys.NUM_PLUS, new Key[]{Key.NumPlus}},
            {DeviceKeys.CAPS_LOCK, new Key[]{Key.Caps}},
            {DeviceKeys.A, new Key[]{Key.A}},
            {DeviceKeys.S, new Key[]{Key.S}},
            {DeviceKeys.D, new Key[]{Key.D}},
            {DeviceKeys.F, new Key[]{Key.F}},
            {DeviceKeys.G, new Key[]{Key.G}},
            {DeviceKeys.H, new Key[]{Key.H}},
            {DeviceKeys.J, new Key[]{Key.J}},
            {DeviceKeys.K, new Key[]{Key.K}},
            {DeviceKeys.L, new Key[]{Key.L}},
            {DeviceKeys.SEMICOLON, new Key[]{Key.Semicolon}},
            {DeviceKeys.APOSTROPHE, new Key[]{Key.Quote}},
            {DeviceKeys.ENTER, new Key[]{Key.Enter0, Key.Enter}},
            {DeviceKeys.NUM_FOUR, new Key[]{Key.Num4}},
            {DeviceKeys.NUM_FIVE, new Key[]{Key.Num5}},
            {DeviceKeys.NUM_SIX, new Key[]{Key.Num6}},
            {DeviceKeys.LEFT_SHIFT, new Key[]{Key.LShift1, Key.LShift2, Key.LShift3}},
            {DeviceKeys.Z, new Key[]{Key.Z}},
            {DeviceKeys.X, new Key[]{Key.X}},
            {DeviceKeys.C, new Key[]{Key.C}},
            {DeviceKeys.V, new Key[]{Key.V}},
            {DeviceKeys.B, new Key[]{Key.B}},
            {DeviceKeys.N, new Key[]{Key.N}},
            {DeviceKeys.M, new Key[]{Key.M}},
            {DeviceKeys.COMMA, new Key[]{Key.Comma}},
            {DeviceKeys.PERIOD, new Key[]{Key.Dot}},
            {DeviceKeys.FORWARD_SLASH, new Key[]{Key.Slash}},
            {DeviceKeys.RIGHT_SHIFT, new Key[]{Key.RShift}},
            {DeviceKeys.ARROW_UP, new Key[]{Key.Up}},
            {DeviceKeys.NUM_ONE, new Key[]{Key.Num1}},
            {DeviceKeys.NUM_TWO, new Key[]{Key.Num2}},
            {DeviceKeys.NUM_THREE, new Key[]{Key.Num3}},
            {DeviceKeys.NUM_ENTER, new Key[]{Key.NumEnter}},
            {DeviceKeys.LEFT_CONTROL, new Key[]{Key.LCtrl}},
            {DeviceKeys.LEFT_FN, new Key[]{Key.Fn}},
            {DeviceKeys.LEFT_WINDOWS, new Key[]{Key.Win}},
            {DeviceKeys.LEFT_ALT, new Key[]{Key.LAlt}},
            {DeviceKeys.SPACE, new Key[]{Key.Space}},
            {DeviceKeys.RIGHT_ALT, new Key[]{Key.RAlt}},
            {DeviceKeys.FN_Key, new Key[]{Key.ContextMenu}},
            {DeviceKeys.RIGHT_CONTROL, new Key[]{Key.RCtrl}},
            {DeviceKeys.ARROW_LEFT, new Key[]{Key.Left}},
            {DeviceKeys.ARROW_DOWN, new Key[]{Key.Down}},
            {DeviceKeys.ARROW_RIGHT, new Key[]{Key.Right}},
            {DeviceKeys.NUM_ZERO, new Key[]{Key.Num0}},
            {DeviceKeys.NUM_PERIOD, new Key[]{Key.NumDot}},
        };

        private static Key[] TranslateKey(DeviceKeys key)
        {
            return KeyMap.TryGetValue(key, out var r) ? r : new []{ Key.Ignore };
        }
    }
}
