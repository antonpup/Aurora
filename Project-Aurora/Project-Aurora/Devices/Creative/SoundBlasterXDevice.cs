using Aurora.Settings;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SBAuroraReactive;

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

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            uint maxKbLength = 0;
            Dictionary<Color, List<Keyboard_LEDIndex>> kbIndices = null;
            if (sbKeyboard != null)
                kbIndices = new Dictionary<Color, List<Keyboard_LEDIndex>>();

            LedColour[] mouseColors = null;
            foreach (KeyValuePair<DeviceKeys, Color> kv in keyColors)
            {
                if (e.Cancel) return false;

                if (kbIndices != null)
                {
                    var kbLedIdx = GetKeyboardMappingLedIndex(kv.Key);
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
                if (sbMouse != null)
                {
                    int moosIdx = GetMouseMappingIndex(kv.Key);
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

            uint numKbGroups = 0;
            uint[] kbGroupsArr = null;
            LedPattern[] kbPatterns = null;
            LedColour[] kbColors = null;
            if (kbIndices != null)
            {
                numKbGroups = (uint)kbIndices.Count;
                kbGroupsArr = new uint[numKbGroups*(maxKbLength+1)];
                kbPatterns = new LedPattern[numKbGroups];
                kbColors = new LedColour[numKbGroups];
                uint currGroup=0;
                foreach (var kv in kbIndices)
                {
                    if (e.Cancel) return false;

                    kbPatterns[currGroup] = LedPattern.Static;
                    kbColors[currGroup].a = kv.Key.A;
                    kbColors[currGroup].r = kv.Key.R;
                    kbColors[currGroup].g = kv.Key.G;
                    kbColors[currGroup].b = kv.Key.B;
                    uint i = currGroup*(maxKbLength + 1);
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
                        sbKeyboardSettings.payloadData = sbKeyboard.LedPayloadFillupAll(sbKeyboardSettings.payloadData.Value, numKbGroups, kbPatterns, maxKbLength+1, kbGroupsArr, 1, 1, kbColors);
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
                        var mouseGroups = new uint[MouseMapping.Length*2];
                        for (int i = 0; i < MouseMapping.Length; i++)
                        {
                            mouseGroups[(i*2)+0] = 1;                           //1 LED in group
                            mouseGroups[(i*2)+1] = (uint)MouseMapping[i].Key;   //Which LED it is
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
                            for (int i=0; i<mouseColors.Length; i++)
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

        static KeyValuePair<Keyboard_LEDIndex, DeviceKeys>[] KeyboardMapping_All = {
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Esc, DeviceKeys.ESC),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.F1, DeviceKeys.F1),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.F2, DeviceKeys.F2),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.F3, DeviceKeys.F3),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.F4, DeviceKeys.F4),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.F5, DeviceKeys.F5),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.F6, DeviceKeys.F6),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.F7, DeviceKeys.F7),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.F8, DeviceKeys.F8),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.F9, DeviceKeys.F9),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.F10, DeviceKeys.F10),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.F11, DeviceKeys.F11),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.F12, DeviceKeys.F12),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.M1, DeviceKeys.G1),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.BackQuote, DeviceKeys.TILDE),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Digit1, DeviceKeys.ONE),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Digit2, DeviceKeys.TWO),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Digit3, DeviceKeys.THREE),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Digit4, DeviceKeys.FOUR),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Digit5, DeviceKeys.FIVE),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Digit6, DeviceKeys.SIX),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Digit7, DeviceKeys.SEVEN),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Digit8, DeviceKeys.EIGHT),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Digit9, DeviceKeys.NINE),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Digit0, DeviceKeys.ZERO),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Minus, DeviceKeys.MINUS),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Equal, DeviceKeys.EQUALS),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Backspace, DeviceKeys.BACKSPACE),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.M2, DeviceKeys.G2),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Tab, DeviceKeys.TAB),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Q, DeviceKeys.Q),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.W, DeviceKeys.W),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.E, DeviceKeys.E),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.R, DeviceKeys.R),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.T, DeviceKeys.T),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Y, DeviceKeys.Y),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.U, DeviceKeys.U),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.I, DeviceKeys.I),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.O, DeviceKeys.O),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.P, DeviceKeys.P),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.OpenBracket, DeviceKeys.OPEN_BRACKET),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.ClosedBracket, DeviceKeys.CLOSE_BRACKET),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.BackSlash, DeviceKeys.BACKSLASH),         //Only on US
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.M3, DeviceKeys.G3),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.CapsLock, DeviceKeys.CAPS_LOCK),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.A, DeviceKeys.A),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.S, DeviceKeys.S),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.D, DeviceKeys.D),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.F, DeviceKeys.F),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.G, DeviceKeys.G),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.H, DeviceKeys.H),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.J, DeviceKeys.J),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.K, DeviceKeys.K),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.L, DeviceKeys.L),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Semicolon, DeviceKeys.SEMICOLON),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Apostrophe, DeviceKeys.APOSTROPHE),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.NonUS57, DeviceKeys.HASHTAG),             //Only on European
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Enter, DeviceKeys.ENTER),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.M4, DeviceKeys.G4),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.LeftShift, DeviceKeys.LEFT_SHIFT),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.NonUS61, DeviceKeys.BACKSLASH_UK),        //Only on European
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Z, DeviceKeys.Z),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.X, DeviceKeys.X),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.C, DeviceKeys.C),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.V, DeviceKeys.V),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.B, DeviceKeys.B),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.N, DeviceKeys.N),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.M, DeviceKeys.M),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Comma, DeviceKeys.COMMA),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Fullstop, DeviceKeys.PERIOD),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.ForwardSlash, DeviceKeys.FORWARD_SLASH),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.RightShift, DeviceKeys.RIGHT_SHIFT),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.M5, DeviceKeys.G5),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.LeftCtrl, DeviceKeys.LEFT_CONTROL),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.LeftWindows, DeviceKeys.LEFT_WINDOWS),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.LeftAlt, DeviceKeys.LEFT_ALT),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Space, DeviceKeys.SPACE),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.RightAlt, DeviceKeys.RIGHT_ALT),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Fn, DeviceKeys.FN_Key),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Menu, DeviceKeys.APPLICATION_SELECT),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.RightCtrl, DeviceKeys.RIGHT_CONTROL),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.PadMinus, DeviceKeys.NUM_MINUS),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.PadAsterisk, DeviceKeys.NUM_ASTERISK),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.PadForwardSlash, DeviceKeys.NUM_SLASH),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.PadNumLock, DeviceKeys.NUM_LOCK),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.PageUp, DeviceKeys.PAGE_UP),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Home, DeviceKeys.HOME),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Insert, DeviceKeys.INSERT),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.PadPlus, DeviceKeys.NUM_PLUS),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Pad9, DeviceKeys.NUM_NINE),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Pad8, DeviceKeys.NUM_EIGHT),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Pad7, DeviceKeys.NUM_SEVEN),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.PageDown, DeviceKeys.PAGE_DOWN),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.End, DeviceKeys.END),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Delete, DeviceKeys.DELETE),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.PrintScreen, DeviceKeys.PRINT_SCREEN),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Pad6, DeviceKeys.NUM_SIX),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Pad5, DeviceKeys.NUM_FIVE),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Pad4, DeviceKeys.NUM_FOUR),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Pad1, DeviceKeys.NUM_ONE),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.UpArrow, DeviceKeys.ARROW_UP),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.LeftArrow, DeviceKeys.ARROW_LEFT),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.ScrollLock, DeviceKeys.SCROLL_LOCK),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.PadEnter, DeviceKeys.NUM_ENTER),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Pad3, DeviceKeys.NUM_THREE),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Pad2, DeviceKeys.NUM_TWO),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.PadFullstop, DeviceKeys.NUM_PERIOD),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Pad0, DeviceKeys.NUM_ZERO),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.RightArrow, DeviceKeys.ARROW_RIGHT),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.DownArrow, DeviceKeys.ARROW_DOWN),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Pause, DeviceKeys.PAUSE_BREAK),
            new KeyValuePair<Keyboard_LEDIndex, DeviceKeys>(Keyboard_LEDIndex.Logo, DeviceKeys.LOGO)
        };

        public static readonly KeyValuePair<Keyboard_LEDIndex, DeviceKeys>[] KeyboardMapping_US;
        public static readonly KeyValuePair<Keyboard_LEDIndex, DeviceKeys>[] KeyboardMapping_European;

        static SoundBlasterXDevice()
        {
            KeyboardMapping_US = KeyboardMapping_All.Where(x => (x.Key != Keyboard_LEDIndex.NonUS57 && x.Key != Keyboard_LEDIndex.NonUS61)).ToArray();
            KeyboardMapping_European = KeyboardMapping_All.Where(x => (x.Key != Keyboard_LEDIndex.BackSlash)).ToArray();
        }

        public Keyboard_LEDIndex GetKeyboardMappingLedIndex(DeviceKeys devKey)
        {
            var mapping = sbKeyboardInfo.deviceId.Equals(EnumeratedDevice.SoundBlasterXVanguardK08_USEnglish) ? KeyboardMapping_US : KeyboardMapping_European;
            for (int i=0; i<mapping.Length; i++)
            {
                if (mapping[i].Value.Equals(devKey))
                    return mapping[i].Key;
            }

            return Keyboard_LEDIndex.NotApplicable;
        }

        public static readonly KeyValuePair<Mouse_LEDIndex, DeviceKeys>[] MouseMapping = {
            new KeyValuePair<Mouse_LEDIndex, DeviceKeys>(Mouse_LEDIndex.LED0, DeviceKeys.MOUSEPADLIGHT1),
            new KeyValuePair<Mouse_LEDIndex, DeviceKeys>(Mouse_LEDIndex.LED1, DeviceKeys.MOUSEPADLIGHT2),
            new KeyValuePair<Mouse_LEDIndex, DeviceKeys>(Mouse_LEDIndex.LED2, DeviceKeys.MOUSEPADLIGHT3),
            new KeyValuePair<Mouse_LEDIndex, DeviceKeys>(Mouse_LEDIndex.LED3, DeviceKeys.MOUSEPADLIGHT4),
            new KeyValuePair<Mouse_LEDIndex, DeviceKeys>(Mouse_LEDIndex.LED4, DeviceKeys.MOUSEPADLIGHT5),
            new KeyValuePair<Mouse_LEDIndex, DeviceKeys>(Mouse_LEDIndex.LED5, DeviceKeys.MOUSEPADLIGHT6),
            new KeyValuePair<Mouse_LEDIndex, DeviceKeys>(Mouse_LEDIndex.LED6, DeviceKeys.MOUSEPADLIGHT7),
            new KeyValuePair<Mouse_LEDIndex, DeviceKeys>(Mouse_LEDIndex.LED7, DeviceKeys.MOUSEPADLIGHT8),
            new KeyValuePair<Mouse_LEDIndex, DeviceKeys>(Mouse_LEDIndex.LED8, DeviceKeys.MOUSEPADLIGHT9),
            new KeyValuePair<Mouse_LEDIndex, DeviceKeys>(Mouse_LEDIndex.LED9, DeviceKeys.MOUSEPADLIGHT10),
            new KeyValuePair<Mouse_LEDIndex, DeviceKeys>(Mouse_LEDIndex.LED10, DeviceKeys.MOUSEPADLIGHT11),
            new KeyValuePair<Mouse_LEDIndex, DeviceKeys>(Mouse_LEDIndex.Logo, DeviceKeys.Peripheral_Logo),
            new KeyValuePair<Mouse_LEDIndex, DeviceKeys>(Mouse_LEDIndex.Wheel, DeviceKeys.Peripheral_ScrollWheel)
        };

        public static int GetMouseMappingIndex(DeviceKeys devKey)
        {
            int i;
            for (i=0; i<MouseMapping.Length; i++)
                if (MouseMapping[i].Value.Equals(devKey))
                    break;

            return i;
        }
    }
}
