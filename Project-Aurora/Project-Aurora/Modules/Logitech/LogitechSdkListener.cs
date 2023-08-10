using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Aurora.Devices;
using Aurora.Modules.Logitech.Enums;
using Aurora.Modules.Logitech.Structs;
using Aurora.Modules.ProcessMonitor;
using Aurora.Utils;
using Microsoft.Win32;
using RGB.NET.Devices.Logitech;
using Color = System.Drawing.Color;

namespace Aurora.Modules.Logitech;

public enum LightsyncSdkState
{
    NotInstalled,
    Waiting,
    Connected,
    Conflicted,
}

public class LogitechSdkListener
{
    private const string RegistryPath64 = @"SOFTWARE\Classes\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary";
    private const string RegistryPath32 = @"SOFTWARE\Classes\WOW6432Node\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary";
    public event EventHandler? ColorsUpdated;
    public event EventHandler<string?>? ApplicationChanged;

    public LightsyncSdkState State { get; private set; }

    public IReadOnlyDictionary<DeviceKeys, Color> Colors => _colors;
    public Color BackgroundColor { get; private set; } = Color.Empty;

    private LogiSetTargetDeviceType DeviceType { get; set; }

    private readonly PipeListener _pipeListener = new("LGS_LED_SDK-00000001");
    private readonly ConcurrentDictionary<DeviceKeys, Color> _colors = new();
    private readonly HashSet<DeviceKeys> _excluded = new();

    private Dictionary<DeviceKeys, Color> _savedColors = new();
    private Color _savedBackground = Color.Empty;

    public async Task Initialize()
    {
        const string runReg = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        using var runKey = Registry.LocalMachine.OpenSubKey(runReg);
        var lgsLaunch = runKey?.GetValue("Launch LCore");
        var lgsInstalled = lgsLaunch != null;
        if (lgsInstalled)
        {
            const string runApprovedReg = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
            using var runApprovedKey = Registry.LocalMachine.OpenSubKey(runApprovedReg);
            var lgsLaunchApproved = runApprovedKey?.GetValue("Launch LCore");
            var runApproved = lgsLaunchApproved is byte[] startValue && startValue[0] == 2;

            if (runApproved)
            {
                State = LightsyncSdkState.Conflicted;
                return;
            }
        }

        if (RunningProcessMonitor.Instance.IsProcessRunning("lcore.exe"))
        {
            State = LightsyncSdkState.Conflicted;
            return;
        }

        var unlocked = await DesktopUtils.WaitSessionUnlock();
        if (unlocked)
        {
            await Task.Delay(5000);
        }

        _pipeListener.ClientConnected += PipeListenerOnClientConnected;
        _pipeListener.ClientDisconnected += OnPipeListenerClientDisconnected;
        _pipeListener.CommandReceived += OnPipeListenerCommandReceived;
        _pipeListener.StartListening();

        State = IsInstalled() ? LightsyncSdkState.Waiting : LightsyncSdkState.NotInstalled;
    }

    private static bool IsInstalled()
    {
        return AuroraInstalled() || ArtemisInstalled();
    }

    private static bool AuroraInstalled()
    {
        using var key64 = Registry.LocalMachine.OpenSubKey(RegistryPath64);
        using var key32 = Registry.LocalMachine.OpenSubKey(RegistryPath32);

        const string dllPath32 = @"C:\ProgramData\Aurora\x86\LogitechLed.dll";
        const string dllPath64 = @"C:\ProgramData\Aurora\x64\LogitechLed.dll";

        var is64BitKeyPresent = key64?.GetValue(null)?.ToString() == dllPath64;
        var is32BitKeyPresent = key32?.GetValue(null)?.ToString() == dllPath32;
        
        return is64BitKeyPresent && is32BitKeyPresent;
    }

    private static bool ArtemisInstalled()
    {
        using var key64 = Registry.LocalMachine.OpenSubKey(RegistryPath64);
        using var key32 = Registry.LocalMachine.OpenSubKey(RegistryPath32);

        const string dllPath32 = @"C:\ProgramData\Artemis\Plugins\Artemis.Plugins.Wrappers.Logitech-c6dda69b\x86\LogitechLed.dll";
        const string dllPath64 = @"C:\ProgramData\Artemis\Plugins\Artemis.Plugins.Wrappers.Logitech-c6dda69b\x64\LogitechLed.dll";

        var is64BitKeyPresent = key64?.GetValue(null)?.ToString() == dllPath64;
        var is32BitKeyPresent = key32?.GetValue(null)?.ToString() == dllPath32;
        
        return is64BitKeyPresent && is32BitKeyPresent;
    }

    private void PipeListenerOnClientConnected(object? sender, EventArgs e)
    {
        State = LightsyncSdkState.Connected;
    }

    private void OnPipeListenerClientDisconnected(object? sender, EventArgs e)
    {
        State = LightsyncSdkState.Waiting;
        ClearData();
        ApplicationChanged?.Invoke(this, null);
    }

    private void OnPipeListenerCommandReceived(object? sender, ReadOnlyMemory<byte> e)
    {
        var command = (LogitechPipeCommand)BitConverter.ToInt32(e.Span);

        var span = e.Span[sizeof(int)..];
        
        switch (command)
        {
            case LogitechPipeCommand.Init:
                Init(span);
                break;
            case LogitechPipeCommand.SetTargetDevice:
                SetTargetDevice(span);
                break;
            case LogitechPipeCommand.SetLighting:
                SetLighting(span);
                break;
            case LogitechPipeCommand.SetLightingForKeyWithKeyName:
                SetLightingForKeyWithKeyName(span);
                break;
            case LogitechPipeCommand.SetLightingForKeyWithScanCode:
                SetLightingForKeyWithScanCode(span);
                break;
            case LogitechPipeCommand.SetLightingForKeyWithHidCode:
                SetLightingForKeyWithHidCode(span);
                break;
            case LogitechPipeCommand.SetLightingForKeyWithQuartzCode:
                SetLightingForKeyWithQuartzCode(span);
                break;
            case LogitechPipeCommand.SetLightingFromBitmap:
                SetLightingFromBitmap(span);
                break;
            case LogitechPipeCommand.ExcludeKeysFromBitmap:
                ExcludeKeysFromBitmap(span);
                break;
            case LogitechPipeCommand.SetLightingForTargetZone:
                SetLightingForTargetZone(span);
                break;
            case LogitechPipeCommand.SaveLighting:
                SaveLighting();
                break;
            case LogitechPipeCommand.RestoreLighting:
                RestoreLighting();
                break;
            case LogitechPipeCommand.SaveLightingForKey:
                SaveLightingForKey(span);
                break;
            case LogitechPipeCommand.RestoreLightingForKey:
                RestoreLightingForKey(span);
                break;
            default:
                Global.logger.Information("Unknown command id: {CommandId}", command);
                break;
        }
    }

    private void SetLightingForKeyWithQuartzCode(ReadOnlySpan<byte> span)
    {
        //_logger.Information("SetLightingForKeyWithQuartzCode");
    }

    private void RestoreLightingForKey(ReadOnlySpan<byte> span)
    {
        var key = (LogitechLedId)BitConverter.ToInt32(span);
        if (!LedMapping.LogitechLedIds.TryGetValue(key, out var deviceKey))
        {
            return;
        }

        if (_savedColors.TryGetValue(deviceKey, out var savedColor))
        {
            _colors[deviceKey] = savedColor;
        }

        ColorsUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void SaveLightingForKey(ReadOnlySpan<byte> span)
    {
        var key = (LogitechLedId)BitConverter.ToInt32(span);
        if (!LedMapping.LogitechLedIds.TryGetValue(key, out var deviceKey))
        {
            return;
        }

        BackgroundColor = _savedBackground;
        if (_colors.TryGetValue(deviceKey, out var savedColor))
        {
            _savedColors[deviceKey] = savedColor;
        }
    }

    private void RestoreLighting()
    {
        BackgroundColor = _savedBackground;
        foreach (var (key, value) in _savedColors)
        {
            _colors[key] = value;
        }

        ColorsUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void SaveLighting()
    {
        _savedBackground = BackgroundColor;
        _savedColors = new Dictionary<DeviceKeys, Color>(_colors);
    }

    private void SetLightingForTargetZone(ReadOnlySpan<byte> span)
    {
        var setTargetZone = MemoryMarshal.Read<LogitechSetTargetZone>(span);
        
        var ledMappings = setTargetZone.DeviceType switch
        {
            LogiDeviceType.Mouse => LedMapping.MouseZoneKeys,
            LogiDeviceType.Mousemat => LedMapping.MousepadZoneKeys,
            LogiDeviceType.Headset => LedMapping.HeadsetZoneKeys,
            LogiDeviceType.Speaker => LedMapping.SpeakerZoneKeys,
            _ => null
        };
        if (ledMappings == null)
            return;
        if (!ledMappings.TryGetValue(setTargetZone.ZoneId, out var keys))
        {
            return;
        }

        foreach (var deviceKeys in keys)
        {
            _colors[deviceKeys] = setTargetZone.RgbColor;
        }
    }

    private void Init(ReadOnlySpan<byte> span)
    {
        var name = ReadNullTerminatedUnicodeString(span);
        ApplicationChanged?.Invoke(this, name);
    }

    private void SetTargetDevice(ReadOnlySpan<byte> span)
    {
        DeviceType = (LogiSetTargetDeviceType)BitConverter.ToInt32(span);
    }

    private void SetLighting(ReadOnlySpan<byte> span)
    {
        var color = ReadPercentageColor(span);

        if (DeviceType == LogiSetTargetDeviceType.PerKeyRgb)
        {
            foreach (var key in _colors.Keys)
            {
                _colors[key] = color;
            }
        }
        else
        {
            BackgroundColor = color;
        }

        ColorsUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void SetLightingForKeyWithKeyName(ReadOnlySpan<byte> span)
    {
        var keyNameIdx = BitConverter.ToInt32(span);
        var color = ReadPercentageColor(span[sizeof(int)..]);
        var keyName = (LogitechLedId)keyNameIdx;

        if (LedMapping.LogitechLedIds.TryGetValue(keyName, out var idx))
        {
            _colors[idx] = color;
        }

        ColorsUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void SetLightingForKeyWithScanCode(ReadOnlySpan<byte> span)
    {
        var scanCodeIdx = BitConverter.ToInt32(span);
        var color = ReadPercentageColor(span[sizeof(int)..]);
        var scanCode = (DirectInputScanCode)scanCodeIdx;

        if (LedMapping.DirectInputScanCodes.TryGetValue(scanCode, out var idx2))
        {
            _colors[idx2] = color;
        }

        ColorsUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void SetLightingForKeyWithHidCode(ReadOnlySpan<byte> span)
    {
        var hidCodeIdx = BitConverter.ToInt32(span);
        var color = ReadPercentageColor(span[sizeof(int)..]);
        var hidCode = (HidCode)hidCodeIdx;

        if (LedMapping.HidCodes.TryGetValue(hidCode, out var idx3))
        {
            _colors[idx3] = color;
        }

        ColorsUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void SetLightingFromBitmap(ReadOnlySpan<byte> span)
    {
        var colors = MemoryMarshal.Cast<byte, LogitechArgbColor>(span);
        for (var clr = 0; clr < colors.Length; clr++)
        {
            if (LedMapping.BitmapMap.TryGetValue(clr, out var ledId) && !_excluded.Contains(ledId))
            {
                _colors[ledId] = colors[clr];
            }
        }

        ColorsUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void ExcludeKeysFromBitmap(ReadOnlySpan<byte> span)
    {
        var keys = MemoryMarshal.Cast<byte, LogitechLedId>(span);
        foreach (var excludedLogitechLedId in keys)
        {
            if (!LedMapping.LogitechLedIds.TryGetValue(excludedLogitechLedId, out var excludedLedId))
                continue;

            _excluded.Add(excludedLedId);
        }
    }

    private static string ReadNullTerminatedUnicodeString(ReadOnlySpan<byte> bytes)
    {
        ReadOnlySpan<byte> unicodeNullTerminator = stackalloc byte[] { 0, 0 };

        var nullTerminatorIndex = bytes.IndexOf(unicodeNullTerminator);

        return nullTerminatorIndex == -1 ? "" : Encoding.Unicode.GetString(bytes[..(nullTerminatorIndex + 1)]);
    }

    private static Color ReadPercentageColor(ReadOnlySpan<byte> bytes)
    {
        return bytes.Length != 12 ? Color.Empty : MemoryMarshal.Read<LogitechRgbColor>(bytes);
    }

    private void ClearData()
    {
        _excluded.Clear();
        _colors.Clear();
        DeviceType = LogiSetTargetDeviceType.All;
        BackgroundColor = Color.Empty;
    }

    public void Dispose()
    {
        _colors.Clear();
        _excluded.Clear();

        _pipeListener.ClientDisconnected -= OnPipeListenerClientDisconnected;
        _pipeListener.CommandReceived -= OnPipeListenerCommandReceived;
        _pipeListener.Dispose();
    }
}