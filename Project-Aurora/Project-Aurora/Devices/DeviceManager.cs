using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Modules;
using Common;
using Common.Data;
using Common.Devices;
using Common.Devices.RGBNet;
using RazerSdkReader;
using RGB.NET.Core;

namespace Aurora.Devices;

public sealed class DeviceManager : IDisposable
{
    private const string DeviceManagerFolder = @".\AuroraDeviceManager";
    private const string DeviceManagerProcess = "AuroraDeviceManager";
    private const string DeviceManagerExe = "AuroraDeviceManager.exe";

    private bool _disposed;

    public event EventHandler? DevicesUpdated;

    public IEnumerable<DeviceContainer> DeviceContainers { get; private set; } = Enumerable.Empty<DeviceContainer>();

    private readonly CancellationTokenSource _cancel = new();

    private readonly Task<ChromaReader?> _rzSdkManager;
    private readonly MemorySharedArray<SimpleColor> _sharedDeviceColor;

    private readonly MemorySharedStruct<DeviceManagerInfo> _dimma;
    private Process? _process;

    private readonly byte[] _end = "\n"u8.ToArray();

    public DeviceManager(Task<ChromaReader?> rzSdkManager)
    {
        _rzSdkManager = rzSdkManager;
        _sharedDeviceColor = new MemorySharedArray<SimpleColor>(Constants.DeviceLedMap, Constants.MaxKeyId);

        _dimma = new MemorySharedStruct<DeviceManagerInfo>(Constants.DeviceInformations);
        _dimma.Updated += OnDimmaOnUpdated;
    }

    public async Task InitializeDevices()
    {
        await _rzSdkManager;

        //TODO reduce to 1 process
        _process = Process.GetProcessesByName(DeviceManagerProcess).FirstOrDefault();
        if (_process != null)
        {
            UpdateDevices();
        }
        else
        {
            _process = StartDmProcess();
        }
    }

    private void OnDimmaOnUpdated(object? o, EventArgs eventArgs)
    {
        UpdateDevices();
    }

    private void UpdateDevices()
    {
        var deviceManagerInfo = _dimma.ReadElement();
        var deviceNames = deviceManagerInfo.DeviceNames.Split(Constants.StringSplit);
        var deviceContainers = new List<DeviceContainer>(deviceNames.Length);
        deviceContainers.AddRange(deviceNames.Select(deviceName =>
        {
            var device = new MemorySharedDevice(deviceName, Global.DeviceConfigration.VarRegistry);
            if (OnlineSettings.DeviceTooltips.TryGetValue(deviceName, out var tooltips))
            {
                device.Tooltips = tooltips;
            }
            Global.DeviceConfigration.VarRegistry.Combine(device.RegisteredVariables);
            return new DeviceContainer(device, this);
        }));

        //TODO reuse
        DeviceContainers = deviceContainers;
        DevicesUpdated?.Invoke(this, EventArgs.Empty);
    }

    private Process? StartDmProcess()
    {
        var updaterProc = new ProcessStartInfo
        {
            FileName = Path.Combine(DeviceManagerFolder, DeviceManagerExe),
            WorkingDirectory = DeviceManagerFolder,
            ErrorDialog = true,
        };
        var process = Process.Start(updaterProc);
        process?.WaitForExitAsync().ContinueWith(DeviceManagerClosed);

        return process;
    }

    private async Task DeviceManagerClosed(Task processTask)
    {
        if (processTask.IsFaulted)
        {
            Global.logger.Error(processTask.Exception, "Device Manager closed unexpectedly");
        }

        //TODO get process stack if fails
        if (_process != null)
        {
            await InitializeDevices();
        }
    }

    public async Task ShutdownDevices()
    {
        _process ??= Process.GetProcessesByName(DeviceManagerProcess).FirstOrDefault();
        if (_process != null)
        {
            var process = _process;
            _process = null;

            await SendCommand(DeviceCommands.Quit);
            await process.WaitForExitAsync();
        }
    }

    public async Task ResetDevices()
    {
        await ShutdownDevices();
        await InitializeDevices();
    }

    public void Detach()
    {
        _process = null;
    }

    public void UpdateDevices(IReadOnlyDictionary<DeviceKeys, SimpleColor> keyColors)
    {
        _sharedDeviceColor.WriteDictionary(keyColors);
    }

    public void Dispose()
    {
        _cancel.Cancel();
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _dimma.Dispose();
        _sharedDeviceColor.Dispose();
    }

    public async void BlinkRemappableKey(RemappableDevice remappableDevice, LedId led)
    {
        if (_process == null)
        {
            return;
        }

        var parameters = remappableDevice.DeviceId + Constants.StringSplit + (int)led;
        var command = DeviceCommands.Blink + Constants.StringSplit + parameters;
        await SendCommand(command);
    }

    public async Task RemapKey(string deviceId, LedId led, DeviceKeys? newKey)
    {
        if (_process == null)
        {
            return;
        }

        if (newKey == null)
        {
            var parameters = deviceId + Constants.StringSplit + (int)led;
            var command = DeviceCommands.Unmap + Constants.StringSplit + parameters;

            await SendCommand(command);
        }
        else
        {
            var parameters = deviceId + Constants.StringSplit + (int)led + Constants.StringSplit + (int)newKey;
            var command = DeviceCommands.Remap + Constants.StringSplit + parameters;

            await SendCommand(command);
        }
    }

    public async Task RequestRemappableDevices()
    {
        await SendCommand( DeviceCommands.Share);
    }

    public async Task EnableDevice(string deviceDeviceName)
    {
        var command = DeviceCommands.Enable + Constants.StringSplit + deviceDeviceName;
        await SendCommand( command);
    }

    public async Task DisableDevice(string deviceDeviceName)
    {
        var command = DeviceCommands.Disable + Constants.StringSplit + deviceDeviceName;
        await SendCommand( command);
    }

    private async Task SendCommand(string command)
    {
        await SendCommand(Encoding.UTF8.GetBytes(command));
    }

    private async Task SendCommand(byte[] command)
    {
        await using var client = new NamedPipeClientStream(".", Constants.DeviceManagerPipe, PipeDirection.Out, PipeOptions.None);
        await client.ConnectAsync(1000);
        if (!client.IsConnected)
            throw new InvalidOperationException("Connection to DeviceManager failed");
        
        client.Write(command, 0, command.Length);
        client.Write(_end, 0, _end.Length);
        
        client.Flush();
    }
}