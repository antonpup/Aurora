using Aurora.Settings;
using Aurora.Utils;
using Aurora.Devices.Layout;
using Aurora.Devices.Layout.Layouts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SBAuroraReactive;
using LEDINT = System.Int16;

namespace Aurora.Devices.Creative
{
    class SoundBlasterXDevice : Device
    {
        private readonly object action_lock = new object();

        private Stopwatch watch = new Stopwatch();
        private long lastUpdateTime = 0;

        private LEDManager sbScanner;

        private LEDManager sbKeyboard;
        private EnumeratedDevice sbKeyboardInfo;
        private LedSettings sbKeyboardSettings;

        private LEDManager sbMouse;
        private EnumeratedDevice sbMouseInfo;
        private LedSettings sbMouseSettings;

        public bool Initialize()
        {
            lock (action_lock)
            {
                EnumeratedDevice[] devicesArr = null;
                try
                {
                    if (sbScanner == null)
                        sbScanner = new LEDManager();

                    devicesArr = sbScanner.EnumConnectedDevices();
                }
                catch (Exception exc)
                {
                    Global.logger.Error("There was an error scanning for SoundBlasterX devices.\r\n" + exc.Message);
                    return false;
                }
                
                if (sbKeyboard == null)
                {
                    int kbdIdx;
                    for (kbdIdx=0; kbdIdx<devicesArr.Length; kbdIdx++)
                    {
                        if (devicesArr[kbdIdx].deviceId.Equals(EnumeratedDevice.SoundBlasterXVanguardK08_USEnglish) ||
                            devicesArr[kbdIdx].deviceId.Equals(EnumeratedDevice.SoundBlasterXVanguardK08_German) ||
                            devicesArr[kbdIdx].deviceId.Equals(EnumeratedDevice.SoundBlasterXVanguardK08_Nordic))
                        {
                            break;
                        }
                    }

                    if (kbdIdx < devicesArr.Length)
                    {
                        LEDManager newKeyboard = null;
                        try
                        {
                            newKeyboard = new LEDManager();
                            newKeyboard.OpenDevice(devicesArr[kbdIdx], false);
                            sbKeyboardInfo = devicesArr[kbdIdx];
                            sbKeyboard = newKeyboard;
                            newKeyboard = null;
                        }
                        catch (Exception exc)
                        {
                            Global.logger.Error("There was an error opening " + devicesArr[kbdIdx].friendlyName + ".\r\n" + exc.Message);
                        }
                        finally
                        {
                            if (newKeyboard != null)
                            {
                                newKeyboard.Dispose();
                                newKeyboard = null;
                            }
                        }
                    }                        
                }

                if (sbMouse == null)
                {
                    int moosIdx;
                    for (moosIdx=0; moosIdx<devicesArr.Length; moosIdx++)
                    {
                        if (devicesArr[moosIdx].deviceId.Equals(EnumeratedDevice.SoundBlasterXSiegeM04))
                        {
                            break;
                        }
                    }

                    if (moosIdx < devicesArr.Length)
                    {
                        LEDManager newMouse = null;
                        try
                        {
                            newMouse = new LEDManager();
                            newMouse.OpenDevice(devicesArr[moosIdx], false);
                            sbMouseInfo = devicesArr[moosIdx];
                            sbMouse = newMouse;
                            newMouse = null;
                        }
                        catch (Exception exc)
                        {
                            Global.logger.Error("There was an error opening " + devicesArr[moosIdx].friendlyName + ".\r\n" + exc.Message);
                        }
                        finally
                        {
                            if (newMouse != null)
                            {
                                newMouse.Dispose();
                                newMouse = null;
                            }
                        }
                    }
                }

                return (sbKeyboard != null) || (sbMouse != null);
            }
        }

        ~SoundBlasterXDevice()
        {
            this.Shutdown();
        }

        public void Shutdown()
        {
            lock (action_lock)
            {
                if (sbMouse != null)
                {
                    if (sbMouseSettings != null && sbMouseSettings.payloadData.HasValue && sbMouseSettings.payloadData.Value.opaqueSize > 0)
                    {
                        try
                        {
                            sbMouseSettings.payloadData = sbMouse.LedPayloadCleanup(sbMouseSettings.payloadData.Value, sbMouseInfo.totalNumLeds);
                        }
                        catch (Exception exc)
                        {
                            Global.logger.Error("There was an error freeing " + sbMouseInfo.friendlyName + ".\r\n" + exc.Message);
                        }
                    }
                    sbMouseSettings = null;
                    try
                    {
                        sbMouse.CloseDevice();
                        sbMouse.Dispose();
                        sbMouse = null;
                    }
                    catch (Exception exc)
                    {
                        Global.logger.Error("There was an error closing " + sbMouseInfo.friendlyName + ".\r\n" + exc.Message);
                    }
                    finally
                    {
                        if (sbMouse != null)
                        {
                            sbMouse.Dispose();
                            sbMouse = null;
                        }
                    }
                }
                if (sbKeyboard != null)
                {
                    if (sbKeyboardSettings != null && sbKeyboardSettings.payloadData.HasValue && sbKeyboardSettings.payloadData.Value.opaqueSize > 0)
                    {
                        try
                        {
                            sbKeyboardSettings.payloadData = sbKeyboard.LedPayloadCleanup(sbKeyboardSettings.payloadData.Value, sbKeyboardSettings.payloadData.Value.opaqueSize);
                        }
                        catch (Exception exc)
                        {
                            Global.logger.Error("There was an error freeing " + sbKeyboardInfo.friendlyName + ".\r\n" + exc.Message);
                        }
                    }
                    sbKeyboardSettings = null;
                    try
                    {
                        sbKeyboard.CloseDevice();
                        sbKeyboard.Dispose();
                        sbKeyboard = null;
                    }
                    catch (Exception exc)
                    {
                        Global.logger.Error("There was an error closing " + sbKeyboardInfo.friendlyName + ".\r\n" + exc.Message);
                    }
                    finally
                    {
                        if (sbKeyboard != null)
                        {
                            sbKeyboard.Dispose();
                            sbKeyboard = null;
                        }
                    }
                }
                if (sbScanner != null)
                {
                    try
                    {
                        sbScanner.Dispose();
                        sbScanner = null;
                    }
                    catch (Exception exc)
                    {
                        Global.logger.Error("There was an error closing SoundBlasterX scanner.\r\n" + exc.Message);
                    }
                    finally
                    {
                        sbScanner = null;
                    }
                }
            }
        }

        public string GetDeviceDetails()
        {
            if (sbKeyboard == null && sbMouse == null)
            {
                return "SoundBlasterX: Not initialized";
            }

            string outDetails = "";
            if (sbKeyboard != null)
                outDetails += sbKeyboardInfo.friendlyName;
            if (sbMouse != null)
            {
                if (outDetails.Length > 0)
                    outDetails += " and ";

                outDetails += sbMouseInfo.friendlyName;
            }
            return outDetails + ":Connected";
        }

        public string GetDeviceName()
        {
            if (sbKeyboard != null && sbMouse == null)
                return sbKeyboardInfo.friendlyName;
            else if (sbKeyboard == null && sbMouse != null)
                return sbMouseInfo.friendlyName;
            else
                return "SoundBlasterX";
        }

        public void Reset()
        {
            if (sbKeyboard != null)
            {
                if (sbKeyboardSettings != null && sbKeyboardSettings.payloadData.HasValue && sbKeyboardSettings.payloadData.Value.opaqueSize > 0)
                {
                    try
                    {
                        sbKeyboardSettings.payloadData = sbKeyboard.LedPayloadCleanup(sbKeyboardSettings.payloadData.Value, sbKeyboardSettings.payloadData.Value.opaqueSize);
                    }
                    catch (Exception exc)
                    {
                        Global.logger.Error("There was an error freeing " + sbKeyboardInfo.friendlyName + ".\r\n" + exc.Message);
                    }
                }
                try
                {
                    sbKeyboard.SetLedSettings(null);
                }
                catch (Exception exc)
                {
                    Global.logger.Error("There was an error resetting " + sbKeyboardInfo.friendlyName + ".\r\n" + exc.Message);
                }
            }
            if (sbMouse != null)
            {
                if (sbMouseSettings != null && sbMouseSettings.payloadData.HasValue && sbMouseSettings.payloadData.Value.opaqueSize > 0)
                {
                    try
                    {
                        sbMouseSettings.payloadData = sbMouse.LedPayloadCleanup(sbMouseSettings.payloadData.Value, sbMouseInfo.totalNumLeds);
                    }
                    catch (Exception exc)
                    {
                        Global.logger.Error("There was an error freeing " + sbMouseInfo.friendlyName + ".\r\n" + exc.Message);
                    }
                }
                try
                {
                    sbMouse.SetLedSettings(null);
                }
                catch (Exception exc)
                {
                    Global.logger.Error("There was an error resetting " + sbMouseInfo.friendlyName + ".\r\n" + exc.Message);
                }
            }
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool IsInitialized()
        {
            return (sbKeyboard != null || sbMouse != null);
        }

        public bool IsKeyboardConnected()
        {
            return (sbKeyboard != null);
        }

        public bool IsPeripheralConnected()
        {
            return (sbMouse != null);
        }

        public string GetDeviceUpdatePerformance()
        {
            return (IsInitialized() ? lastUpdateTime + " ms" : "");
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
                    }
                }
            }
            catch (Exception ex)
            {
                Global.logger.Error("SoundBlasterX device, error when updating device: " + ex);
                return false;
            }

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return updateResult;
        }

        public bool UpdateDevice(KeyboardDeviceLayout keyboard, DoWorkEventArgs e, bool forced = false)
        {
            uint maxKbLength = 0;
            Dictionary<Color, List<Keyboard_LEDIndex>> kbIndices = null;
            if (sbKeyboard != null)
                kbIndices = new Dictionary<Color, List<Keyboard_LEDIndex>>();

            foreach (KeyValuePair<LEDINT, Color> kv in keyboard.DeviceColours.deviceColours)
            {
                if (e.Cancel) return false;

                if (kbIndices != null)
                {
                    var kbLedIdx = GetKeyboardMappingLedIndex((KeyboardKeys)kv.Key);
                    if (kbLedIdx != Keyboard_LEDIndex.NotApplicable)
                    {
                        if (!kbIndices.ContainsKey(kv.Value))
                            kbIndices[kv.Value] = new List<Keyboard_LEDIndex>(1);

                        var list = kbIndices[kv.Value];
                        list.Add(kbLedIdx);
                        if (list.Count > maxKbLength)
                            maxKbLength = (uint)list.Count;
                    }
                }
            }

            uint numKbGroups = 0;
            uint[] kbGroupsArr = null;
            LedPattern[] kbPatterns = null;
            LedColour[] kbColors = null;
            if (kbIndices != null)
            {
                numKbGroups = (uint)kbIndices.Count;
                kbGroupsArr = new uint[numKbGroups * (maxKbLength + 1)];
                kbPatterns = new LedPattern[numKbGroups];
                kbColors = new LedColour[numKbGroups];
                uint currGroup = 0;
                foreach (var kv in kbIndices)
                {
                    if (e.Cancel) return false;

                    kbPatterns[currGroup] = LedPattern.Static;
                    kbColors[currGroup].a = kv.Key.A;
                    kbColors[currGroup].r = kv.Key.R;
                    kbColors[currGroup].g = kv.Key.G;
                    kbColors[currGroup].b = kv.Key.B;
                    uint i = currGroup * (maxKbLength + 1);
                    kbGroupsArr[i++] = (uint)kv.Value.Count;
                    foreach (Keyboard_LEDIndex idx in kv.Value)
                        kbGroupsArr[i++] = (uint)idx;

                    currGroup++;
                }
                kbIndices = null;
            }

            lock (action_lock)
            {
                if (e.Cancel) return false;
                if (sbKeyboard != null && numKbGroups > 0)
                {
                    try
                    {
                        if (sbKeyboardSettings == null)
                        {
                            sbKeyboardSettings = new LedSettings();
                            sbKeyboardSettings.persistentInDevice = false;
                            sbKeyboardSettings.globalPatternMode = false;
                            sbKeyboardSettings.pattern = LedPattern.Static;
                            sbKeyboardSettings.payloadData = new LedPayloadData();
                        }

                        sbKeyboardSettings.payloadData = sbKeyboard.LedPayloadInitialize(sbKeyboardSettings.payloadData.Value, numKbGroups, maxKbLength, 1);
                        sbKeyboardSettings.payloadData = sbKeyboard.LedPayloadFillupAll(sbKeyboardSettings.payloadData.Value, numKbGroups, kbPatterns, maxKbLength + 1, kbGroupsArr, 1, 1, kbColors);
                        sbKeyboard.SetLedSettings(sbKeyboardSettings);
                    }
                    catch (Exception exc)
                    {
                        Global.logger.Error("Failed to Update Device " + sbKeyboardInfo.friendlyName + ": " + exc.ToString());
                        return false;
                    }
                    finally
                    {
                        if (sbKeyboardSettings != null && sbKeyboardSettings.payloadData.HasValue && sbKeyboardSettings.payloadData.Value.opaqueSize > 0)
                            sbKeyboardSettings.payloadData = sbKeyboard.LedPayloadCleanup(sbKeyboardSettings.payloadData.Value, numKbGroups);
                    }
                }
            }
            return true;
        }

        static KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>[] KeyboardMapping_All = {
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Esc, KeyboardKeys.ESC),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.F1, KeyboardKeys.F1),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.F2, KeyboardKeys.F2),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.F3, KeyboardKeys.F3),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.F4, KeyboardKeys.F4),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.F5, KeyboardKeys.F5),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.F6, KeyboardKeys.F6),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.F7, KeyboardKeys.F7),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.F8, KeyboardKeys.F8),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.F9, KeyboardKeys.F9),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.F10, KeyboardKeys.F10),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.F11, KeyboardKeys.F11),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.F12, KeyboardKeys.F12),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.M1, KeyboardKeys.G1),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.BackQuote, KeyboardKeys.TILDE),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Digit1, KeyboardKeys.ONE),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Digit2, KeyboardKeys.TWO),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Digit3, KeyboardKeys.THREE),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Digit4, KeyboardKeys.FOUR),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Digit5, KeyboardKeys.FIVE),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Digit6, KeyboardKeys.SIX),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Digit7, KeyboardKeys.SEVEN),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Digit8, KeyboardKeys.EIGHT),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Digit9, KeyboardKeys.NINE),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Digit0, KeyboardKeys.ZERO),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Minus, KeyboardKeys.MINUS),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Equal, KeyboardKeys.EQUALS),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Backspace, KeyboardKeys.BACKSPACE),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.M2, KeyboardKeys.G2),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Tab, KeyboardKeys.TAB),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Q, KeyboardKeys.Q),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.W, KeyboardKeys.W),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.E, KeyboardKeys.E),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.R, KeyboardKeys.R),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.T, KeyboardKeys.T),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Y, KeyboardKeys.Y),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.U, KeyboardKeys.U),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.I, KeyboardKeys.I),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.O, KeyboardKeys.O),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.P, KeyboardKeys.P),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.OpenBracket, KeyboardKeys.OPEN_BRACKET),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.ClosedBracket, KeyboardKeys.CLOSE_BRACKET),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.BackSlash, KeyboardKeys.BACKSLASH),         //Only on US
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.M3, KeyboardKeys.G3),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.CapsLock, KeyboardKeys.CAPS_LOCK),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.A, KeyboardKeys.A),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.S, KeyboardKeys.S),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.D, KeyboardKeys.D),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.F, KeyboardKeys.F),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.G, KeyboardKeys.G),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.H, KeyboardKeys.H),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.J, KeyboardKeys.J),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.K, KeyboardKeys.K),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.L, KeyboardKeys.L),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Semicolon, KeyboardKeys.SEMICOLON),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Apostrophe, KeyboardKeys.APOSTROPHE),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.NonUS57, KeyboardKeys.HASH),             //Only on European
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Enter, KeyboardKeys.ENTER),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.M4, KeyboardKeys.G4),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.LeftShift, KeyboardKeys.LEFT_SHIFT),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.NonUS61, KeyboardKeys.BACKSLASH_UK),        //Only on European
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Z, KeyboardKeys.Z),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.X, KeyboardKeys.X),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.C, KeyboardKeys.C),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.V, KeyboardKeys.V),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.B, KeyboardKeys.B),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.N, KeyboardKeys.N),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.M, KeyboardKeys.M),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Comma, KeyboardKeys.COMMA),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Fullstop, KeyboardKeys.PERIOD),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.ForwardSlash, KeyboardKeys.FORWARD_SLASH),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.RightShift, KeyboardKeys.RIGHT_SHIFT),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.M5, KeyboardKeys.G5),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.LeftCtrl, KeyboardKeys.LEFT_CONTROL),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.LeftWindows, KeyboardKeys.LEFT_WINDOWS),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.LeftAlt, KeyboardKeys.LEFT_ALT),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Space, KeyboardKeys.SPACE),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.RightAlt, KeyboardKeys.RIGHT_ALT),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Fn, KeyboardKeys.FN_Key),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Menu, KeyboardKeys.APPLICATION_SELECT),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.RightCtrl, KeyboardKeys.RIGHT_CONTROL),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.PadMinus, KeyboardKeys.NUM_MINUS),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.PadAsterisk, KeyboardKeys.NUM_ASTERISK),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.PadForwardSlash, KeyboardKeys.NUM_SLASH),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.PadNumLock, KeyboardKeys.NUM_LOCK),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.PageUp, KeyboardKeys.PAGE_UP),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Home, KeyboardKeys.HOME),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Insert, KeyboardKeys.INSERT),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.PadPlus, KeyboardKeys.NUM_PLUS),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Pad9, KeyboardKeys.NUM_NINE),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Pad8, KeyboardKeys.NUM_EIGHT),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Pad7, KeyboardKeys.NUM_SEVEN),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.PageDown, KeyboardKeys.PAGE_DOWN),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.End, KeyboardKeys.END),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Delete, KeyboardKeys.DELETE),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.PrintScreen, KeyboardKeys.PRINT_SCREEN),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Pad6, KeyboardKeys.NUM_SIX),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Pad5, KeyboardKeys.NUM_FIVE),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Pad4, KeyboardKeys.NUM_FOUR),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Pad1, KeyboardKeys.NUM_ONE),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.UpArrow, KeyboardKeys.ARROW_UP),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.LeftArrow, KeyboardKeys.ARROW_LEFT),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.ScrollLock, KeyboardKeys.SCROLL_LOCK),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.PadEnter, KeyboardKeys.NUM_ENTER),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Pad3, KeyboardKeys.NUM_THREE),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Pad2, KeyboardKeys.NUM_TWO),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.PadFullstop, KeyboardKeys.NUM_PERIOD),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Pad0, KeyboardKeys.NUM_ZERO),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.RightArrow, KeyboardKeys.ARROW_RIGHT),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.DownArrow, KeyboardKeys.ARROW_DOWN),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Pause, KeyboardKeys.PAUSE_BREAK),
            new KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>(Keyboard_LEDIndex.Logo, KeyboardKeys.LOGO)
        };

        public static readonly KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>[] KeyboardMapping_US;
        public static readonly KeyValuePair<Keyboard_LEDIndex, KeyboardKeys>[] KeyboardMapping_European;

        static SoundBlasterXDevice()
        {
            KeyboardMapping_US = KeyboardMapping_All.Where(x => (x.Key != Keyboard_LEDIndex.NonUS57 && x.Key != Keyboard_LEDIndex.NonUS61)).ToArray();
            KeyboardMapping_European = KeyboardMapping_All.Where(x => (x.Key != Keyboard_LEDIndex.BackSlash)).ToArray();
        }

        public Keyboard_LEDIndex GetKeyboardMappingLedIndex(KeyboardKeys devKey)
        {
            var mapping = sbKeyboardInfo.deviceId.Equals(EnumeratedDevice.SoundBlasterXVanguardK08_USEnglish) ? KeyboardMapping_US : KeyboardMapping_European;
            for (int i = 0; i < mapping.Length; i++)
            {
                if (mapping[i].Value.Equals(devKey))
                    return mapping[i].Key;
            }

            return Keyboard_LEDIndex.NotApplicable;
        }

        public bool UpdateDevice(MouseDeviceLayout mouse, DoWorkEventArgs e, bool forced = false)
        {
            LedColour[] mouseColors = null;
            foreach (KeyValuePair<LEDINT, Color> kv in mouse.DeviceColours.deviceColours)
            {
                if (e.Cancel) return false;
                if (sbMouse != null)
                {
                    int moosIdx = GetMouseMappingIndex((MouseLights)kv.Key);
                    if (moosIdx >= 0 && moosIdx <= MouseMapping.Length)
                    {
                        if (mouseColors == null)
                            mouseColors = new LedColour[MouseMapping.Length];

                        mouseColors[moosIdx].a = kv.Value.A;
                        mouseColors[moosIdx].r = kv.Value.R;
                        mouseColors[moosIdx].g = kv.Value.G;
                        mouseColors[moosIdx].b = kv.Value.B;
                    }
                }
            }

            lock (action_lock)
            {
                if (e.Cancel) return false;
                if (sbMouse != null && mouseColors != null)
                {
                    if (sbMouseSettings == null)
                    {
                        sbMouseSettings = new LedSettings();
                        sbMouseSettings.persistentInDevice = false;
                        sbMouseSettings.globalPatternMode = false;
                        sbMouseSettings.pattern = LedPattern.Static;
                        sbMouseSettings.payloadData = new LedPayloadData();
                    }

                    if (sbMouseSettings.payloadData.Value.opaqueSize == 0)
                    {
                        var mousePatterns = new LedPattern[mouseColors.Length];
                        var mouseGroups = new uint[MouseMapping.Length * 2];
                        for (int i = 0; i < MouseMapping.Length; i++)
                        {
                            mouseGroups[(i * 2) + 0] = 1;                           //1 LED in group
                            mouseGroups[(i * 2) + 1] = (uint)MouseMapping[i].Key;   //Which LED it is
                            mousePatterns[i] = LedPattern.Static;               //LED has a host-controlled static color
                        }

                        try
                        {
                            sbMouseSettings.payloadData = sbMouse.LedPayloadInitialize(sbMouseSettings.payloadData.Value, sbMouseInfo.totalNumLeds, 1, 1);
                            sbMouseSettings.payloadData = sbMouse.LedPayloadFillupAll(sbMouseSettings.payloadData.Value, (uint)mouseColors.Length, mousePatterns, 2, mouseGroups, 1, 1, mouseColors);
                        }
                        catch (Exception exc)
                        {
                            Global.logger.Error("Failed to setup data for " + sbMouseInfo.friendlyName + ": " + exc.ToString());
                            if (sbMouseSettings.payloadData.Value.opaqueSize > 0)
                                sbMouseSettings.payloadData = sbMouse.LedPayloadCleanup(sbMouseSettings.payloadData.Value, sbMouseInfo.totalNumLeds);

                            return false;
                        }
                    }
                    else
                    {
                        try
                        {
                            for (int i = 0; i < mouseColors.Length; i++)
                                sbMouseSettings.payloadData = sbMouse.LedPayloadFillupLedColour(sbMouseSettings.payloadData.Value, (uint)i, 1, mouseColors[i], false);
                        }
                        catch (Exception exc)
                        {
                            Global.logger.Error("Failed to fill color data for " + sbMouseInfo.friendlyName + ": " + exc.ToString());
                            return false;
                        }
                    }

                    try
                    {
                        sbMouse.SetLedSettings(sbMouseSettings);
                    }
                    catch (Exception exc)
                    {
                        Global.logger.Error("Failed to Update Device " + sbMouseInfo.friendlyName + ": " + exc.ToString());
                        return false;
                    }
                }
            }

            return true;
        }

        public static readonly KeyValuePair<Mouse_LEDIndex, MouseLights>[] MouseMapping = {

            new KeyValuePair<Mouse_LEDIndex, MouseLights>(Mouse_LEDIndex.LED0, MouseLights.Peripheral_ExtraLightIndex),
            new KeyValuePair<Mouse_LEDIndex, MouseLights>(Mouse_LEDIndex.LED1, MouseLights.Peripheral_ExtraLightIndex+1),
            new KeyValuePair<Mouse_LEDIndex, MouseLights>(Mouse_LEDIndex.LED2, MouseLights.Peripheral_ExtraLightIndex+2),
            new KeyValuePair<Mouse_LEDIndex, MouseLights>(Mouse_LEDIndex.LED3, MouseLights.Peripheral_ExtraLightIndex+3),
            new KeyValuePair<Mouse_LEDIndex, MouseLights>(Mouse_LEDIndex.LED4, MouseLights.Peripheral_ExtraLightIndex+4),
            new KeyValuePair<Mouse_LEDIndex, MouseLights>(Mouse_LEDIndex.LED5, MouseLights.Peripheral_ExtraLightIndex+5),
            new KeyValuePair<Mouse_LEDIndex, MouseLights>(Mouse_LEDIndex.LED6, MouseLights.Peripheral_ExtraLightIndex+6),
            new KeyValuePair<Mouse_LEDIndex, MouseLights>(Mouse_LEDIndex.LED7, MouseLights.Peripheral_ExtraLightIndex+7),
            new KeyValuePair<Mouse_LEDIndex, MouseLights>(Mouse_LEDIndex.LED8, MouseLights.Peripheral_ExtraLightIndex+8),
            new KeyValuePair<Mouse_LEDIndex, MouseLights>(Mouse_LEDIndex.LED9, MouseLights.Peripheral_ExtraLightIndex+9),
            new KeyValuePair<Mouse_LEDIndex, MouseLights>(Mouse_LEDIndex.LED10, MouseLights.Peripheral_ExtraLightIndex+10),
            new KeyValuePair<Mouse_LEDIndex, MouseLights>(Mouse_LEDIndex.Logo, MouseLights.Peripheral_Logo),
            new KeyValuePair<Mouse_LEDIndex, MouseLights>(Mouse_LEDIndex.Wheel, MouseLights.Peripheral_ScrollWheel)
        };

        public static int GetMouseMappingIndex(MouseLights devKey)
        {
            int i;
            for (i = 0; i < MouseMapping.Length; i++)
                if (MouseMapping[i].Value.Equals(devKey))
                    break;

            return i;
        }
    }
}
