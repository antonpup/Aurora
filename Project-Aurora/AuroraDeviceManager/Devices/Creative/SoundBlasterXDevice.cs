using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using Common.Devices;
using SBAuroraReactive;

namespace AuroraDeviceManager.Devices.Creative;

public class SoundBlasterXDevice : DefaultDevice
{
    private static readonly KeyValuePair<Keyboard_LEDIndex, DeviceKeys>[] KeyboardMappingUs;
    private static readonly KeyValuePair<Keyboard_LEDIndex, DeviceKeys>[] KeyboardMappingEuropean;

    public override string DeviceName => "SoundBlasterX";

    private LEDManager? _sbScanner;

    private LEDManager? _sbKeyboard;
    private EnumeratedDevice? _sbKeyboardInfo;
    private LedSettings? _sbKeyboardSettings;

    private LEDManager? _sbMouse;
    private EnumeratedDevice? _sbMouseInfo;
    private LedSettings? _sbMouseSettings;
    
    public override string DeviceDetails
    {
        get
        {
            if (!IsInitialized && _sbKeyboard == null && _sbMouse == null)
            {
                return "";
            }

            var outDetails = "";
            if (_sbKeyboard != null)
                outDetails += _sbKeyboardInfo?.friendlyName;
            if (_sbMouse == null) return outDetails + ": Initialized";
            if (outDetails.Length > 0)
                outDetails += " and ";

            outDetails += _sbMouseInfo?.friendlyName;
            return outDetails + ": Initialized";
        }
    }
    
    [MethodImpl(MethodImplOptions.Synchronized)]
    protected override Task<bool> DoInitialize()
    {
        EnumeratedDevice[] devicesArr;
        try
        {
            _sbScanner ??= new LEDManager();
            devicesArr = _sbScanner.EnumConnectedDevices();
        }
        catch (Exception exc)
        {
            Global.Logger.Error("There was an error scanning for SoundBlasterX devices", exc);
            IsInitialized = false;
            return Task.FromResult(false);
        }

        if (_sbKeyboard == null)
        {
            int kbdIdx;
            for (kbdIdx = 0; kbdIdx < devicesArr.Length; kbdIdx++)
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
                    _sbKeyboardInfo = devicesArr[kbdIdx];
                    _sbKeyboard = newKeyboard;
                    newKeyboard = null;
                }
                catch (Exception exc)
                {
                    Global.Logger.Error(exc, "There was an error opening {Name}", devicesArr[kbdIdx].friendlyName);
                }
                finally
                {
                    newKeyboard?.Dispose();
                }
            }
        }

        if (_sbMouse != null)
        {
            IsInitialized = _sbKeyboard != null || _sbMouse != null;
            return Task.FromResult(IsInitialized);
        }

        int moosIdx;
        for (moosIdx = 0; moosIdx < devicesArr.Length; moosIdx++)
        {
            if (devicesArr[moosIdx].deviceId.Equals(EnumeratedDevice.SoundBlasterXSiegeM04))
            {
                break;
            }
        }

        if (moosIdx >= devicesArr.Length)
        {
            IsInitialized = _sbKeyboard != null || _sbMouse != null;
            return Task.FromResult(IsInitialized);
        }

        LEDManager newMouse = null;
        try
        {
            newMouse = new LEDManager();
            newMouse.OpenDevice(devicesArr[moosIdx], false);
            _sbMouseInfo = devicesArr[moosIdx];
            _sbMouse = newMouse;
            newMouse = null;
        }
        catch (Exception exc)
        {
            Global.Logger.Error(exc, "There was an error opening {Name}", devicesArr[moosIdx].friendlyName);
        }
        finally
        {
            newMouse?.Dispose();
        }

        IsInitialized = _sbKeyboard != null || _sbMouse != null;
        return Task.FromResult(IsInitialized);
    }

    static SoundBlasterXDevice()
    {
        KeyboardMappingUs = SoundBlasterMappings.KeyboardMapping_All
            .Where(x => x.Key != Keyboard_LEDIndex.NonUS57 && x.Key != Keyboard_LEDIndex.NonUS61).ToArray();
        KeyboardMappingEuropean = SoundBlasterMappings.KeyboardMapping_All
            .Where(x => x.Key != Keyboard_LEDIndex.BackSlash).ToArray();
    }

    ~SoundBlasterXDevice()
    {
        Shutdown();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    protected override Task Shutdown()
    {
        if (_sbMouse != null)
        {
            if (_sbMouseSettings is { payloadData.opaqueSize: > 0 })
            {
                try
                {
                    _sbMouseSettings.payloadData = _sbMouse.LedPayloadCleanup(_sbMouseSettings.payloadData.Value,
                        _sbMouseInfo?.totalNumLeds ?? 0);
                }
                catch (Exception exc)
                {
                    Global.Logger.Error(exc, "There was an error freeing {Name}", _sbMouseInfo?.friendlyName);
                }
            }

            _sbMouseSettings = null;
            try
            {
                _sbMouse.CloseDevice();
                _sbMouse.Dispose();
                _sbMouse = null;
            }
            catch (Exception exc)
            {
                Global.Logger.Error(exc, "There was an error closing {Name}", _sbMouseInfo?.friendlyName);
            }
            finally
            {
                if (_sbMouse != null)
                {
                    _sbMouse.Dispose();
                    _sbMouse = null;
                }
            }
        }

        if (_sbKeyboard != null)
        {
            if (_sbKeyboardSettings is { payloadData.opaqueSize: > 0 })
            {
                try
                {
                    _sbKeyboardSettings.payloadData = _sbKeyboard.LedPayloadCleanup(
                        _sbKeyboardSettings.payloadData.Value, _sbKeyboardSettings.payloadData.Value.opaqueSize);
                }
                catch (Exception exc)
                {
                    Global.Logger.Error(exc, "There was an error freeing {Name}", _sbKeyboardInfo?.friendlyName);
                }
            }

            _sbKeyboardSettings = null;
            try
            {
                _sbKeyboard.CloseDevice();
                _sbKeyboard.Dispose();
                _sbKeyboard = null;
            }
            catch (Exception exc)
            {
                Global.Logger.Error(exc, "There was an error closing {Name}", _sbKeyboardInfo?.friendlyName);
            }
            finally
            {
                if (_sbKeyboard != null)
                {
                    _sbKeyboard.Dispose();
                    _sbKeyboard = null;
                }
            }
        }

        if (_sbScanner == null) return Task.CompletedTask;
        try
        {
            _sbScanner.Dispose();
            _sbScanner = null;
        }
        catch (Exception exc)
        {
            Global.Logger.Error(exc, "There was an error closing SoundBlasterX scanner");
        }
        finally
        {
            _sbScanner = null;
        }

        return Task.CompletedTask;
    }

    protected override Task<bool> UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e,
        bool forced = false)
    {
        uint maxKbLength = 0;
        Dictionary<Color, List<Keyboard_LEDIndex>> kbIndices = null;
        if (_sbKeyboard != null)
            kbIndices = new Dictionary<Color, List<Keyboard_LEDIndex>>();

        LedColour[] mouseColors = null;
        foreach (KeyValuePair<DeviceKeys, Color> kv in keyColors)
        {
            if (e.Cancel) return Task.FromResult(false);

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

            if (_sbMouse == null) continue;
            int moosIdx = GetMouseMappingIndex(kv.Key);
            if (moosIdx < 0 || moosIdx > SoundBlasterMappings.MouseMapping.Length) continue;
            mouseColors ??= new LedColour[SoundBlasterMappings.MouseMapping.Length];

            mouseColors[moosIdx].a = kv.Value.A;
            mouseColors[moosIdx].r = kv.Value.R;
            mouseColors[moosIdx].g = kv.Value.G;
            mouseColors[moosIdx].b = kv.Value.B;
        }

        uint numKbGroups = 0;
        if (kbIndices == null)
            return Task.FromResult(Update(e, numKbGroups, maxKbLength, null, null, null, mouseColors));
        numKbGroups = (uint)kbIndices.Count;
        var kbGroupsArr = new uint[numKbGroups * (maxKbLength + 1)];
        var kbPatterns = new LedPattern[numKbGroups];
        var kbColors = new LedColour[numKbGroups];
        uint currGroup = 0;
        foreach (var kv in kbIndices)
        {
            if (e.Cancel) return Task.FromResult(false);

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

        return Task.FromResult(Update(e, numKbGroups, maxKbLength, kbPatterns, kbGroupsArr, kbColors, mouseColors));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private bool Update(DoWorkEventArgs e, uint numKbGroups, uint maxKbLength, LedPattern[] kbPatterns, uint[] kbGroupsArr,
        LedColour[] kbColors, LedColour[]? mouseColors)
    {
        if (e.Cancel) return false;
        if (_sbKeyboard != null && numKbGroups > 0)
        {
            try
            {
                if (_sbKeyboardSettings == null)
                {
                    _sbKeyboardSettings = new LedSettings();
                    _sbKeyboardSettings.persistentInDevice = false;
                    _sbKeyboardSettings.globalPatternMode = false;
                    _sbKeyboardSettings.pattern = LedPattern.Static;
                    _sbKeyboardSettings.payloadData = new LedPayloadData();
                }

                _sbKeyboardSettings.payloadData = _sbKeyboard.LedPayloadInitialize(
                    _sbKeyboardSettings.payloadData.Value, numKbGroups, maxKbLength, 1);
                _sbKeyboardSettings.payloadData = _sbKeyboard.LedPayloadFillupAll(
                    _sbKeyboardSettings.payloadData.Value, numKbGroups, kbPatterns,
                    maxKbLength + 1, kbGroupsArr, 1, 1, kbColors);
                _sbKeyboard.SetLedSettings(_sbKeyboardSettings);
            }
            catch (Exception exc)
            {
                Global.Logger.Error(exc,"Failed to Update Device {Name}", _sbKeyboardInfo?.friendlyName);
                return false;
            }
            finally
            {
                if (_sbKeyboardSettings != null && _sbKeyboardSettings.payloadData.HasValue &&
                    _sbKeyboardSettings.payloadData.Value.opaqueSize > 0)
                    _sbKeyboardSettings.payloadData =
                        _sbKeyboard.LedPayloadCleanup(_sbKeyboardSettings.payloadData.Value, numKbGroups);
            }
        }

        if (e.Cancel) return false;
        if (_sbMouse == null || mouseColors == null) return true;
        if (_sbMouseSettings == null)
        {
            _sbMouseSettings = new LedSettings();
            _sbMouseSettings.persistentInDevice = false;
            _sbMouseSettings.globalPatternMode = false;
            _sbMouseSettings.pattern = LedPattern.Static;
            _sbMouseSettings.payloadData = new LedPayloadData();
        }

        if (_sbMouseSettings.payloadData.Value.opaqueSize == 0)
        {
            var mousePatterns = new LedPattern[mouseColors.Length];
            var mouseGroups = new uint[SoundBlasterMappings.MouseMapping.Length * 2];
            for (int i = 0; i < SoundBlasterMappings.MouseMapping.Length; i++)
            {
                mouseGroups[i * 2 + 0] = 1; //1 LED in group
                mouseGroups[i * 2 + 1] = (uint)SoundBlasterMappings.MouseMapping[i].Key; //Which LED it is
                mousePatterns[i] = LedPattern.Static; //LED has a host-controlled static color
            }

            try
            {
                _sbMouseSettings.payloadData = _sbMouse.LedPayloadInitialize(
                    _sbMouseSettings.payloadData.Value,
                    _sbMouseInfo?.totalNumLeds ?? 0,
                    1, 1);
                _sbMouseSettings.payloadData = _sbMouse.LedPayloadFillupAll(
                    _sbMouseSettings.payloadData.Value,
                    (uint)mouseColors.Length, mousePatterns,
                    2, mouseGroups, 1, 1, mouseColors);
            }
            catch (Exception exc)
            {
                Global.Logger.Error(exc, "Failed to setup data for {Name}", _sbMouseInfo?.friendlyName);
                if (_sbMouseSettings.payloadData.Value.opaqueSize > 0)
                    _sbMouseSettings.payloadData = _sbMouse.LedPayloadCleanup(_sbMouseSettings.payloadData.Value,
                        _sbMouseInfo?.totalNumLeds ?? 0);

                return false;
            }
        }
        else
        {
            try
            {
                for (int i = 0; i < mouseColors.Length; i++)
                    _sbMouseSettings.payloadData = _sbMouse.LedPayloadFillupLedColour(
                        _sbMouseSettings.payloadData.Value, (uint)i, 1, mouseColors[i], false);
            }
            catch (Exception exc)
            {
                Global.Logger.Error(exc, "Failed to fill color data for {Name}", _sbMouseInfo?.friendlyName);
                return false;
            }
        }

        try
        {
            _sbMouse.SetLedSettings(_sbMouseSettings);
        }
        catch (Exception exc)
        {
            Global.Logger.Error(exc, "Failed to Update Device {Name}", _sbMouseInfo?.friendlyName);
            return false;
        }

        return true;
    }

    private Keyboard_LEDIndex GetKeyboardMappingLedIndex(DeviceKeys devKey)
    {
        var mapping = _sbKeyboardInfo?.deviceId.Equals(EnumeratedDevice.SoundBlasterXVanguardK08_USEnglish) ?? true
            ? KeyboardMappingUs
            : KeyboardMappingEuropean;
        foreach (var t in mapping)
        {
            if (t.Value.Equals(devKey))
                return t.Key;
        }

        return Keyboard_LEDIndex.NotApplicable;
    }

    private static int GetMouseMappingIndex(DeviceKeys devKey)
    {
        int i;
        for (i = 0; i < SoundBlasterMappings.MouseMapping.Length; i++)
            if (SoundBlasterMappings.MouseMapping[i].Value.Equals(devKey))
                break;

        return i;
    }
}