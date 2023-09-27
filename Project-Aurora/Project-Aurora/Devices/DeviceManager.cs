using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Data;
using Common.Devices;
using RazerSdkReader;

namespace Aurora.Devices;

public sealed class DeviceManager: IDisposable
{
    private const string DeviceManagerFolder = @".\AurorDeviceManager";
    private const string DeviceManagerProcess = "AurorDeviceManager";
    private const string DeviceManagerExe = "AurorDeviceManager.exe";

    private bool _disposed;

    public event EventHandler? DevicesUpdated;

    public IEnumerable<DeviceContainer> DeviceContainers { get; private set; } = Enumerable.Empty<DeviceContainer>();
    public IEnumerable<DeviceContainer> InitializedDeviceContainers => DeviceContainers.Where(d => d.Device.IsInitialized);
    
    private readonly CancellationTokenSource _cancel = new();

    private readonly Task<ChromaReader?> _rzSdkManager;
    private readonly MemorySharedArray<SimpleColor> _sharedDeviceColor;
    
    private readonly MemorySharedStruct<DeviceManagerInfo> _dimma;
    private Process? _process;

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
            var device = new MemorySharedDevice(deviceName, Global.Configuration.VarRegistry);
            Global.Configuration.VarRegistry.Combine(device.RegisteredVariables);
            return new DeviceContainer(device);
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
            var client = new NamedPipeClientStream(".", Constants.DeviceManagerPipe, PipeDirection.Out, PipeOptions.Asynchronous);
            await client.ConnectAsync(2000);
            if (!client.IsConnected)
                throw new InvalidOperationException("Connection to DeviceManager failed");

            var process = _process;
            _process = null;

            var command = "restore\n"u8.ToArray();
            client.Write(command, 0, command.Length);
            client.Flush();
            client.Close();

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
        _disposed = true;
        _dimma?.Dispose();
        _sharedDeviceColor.Dispose();
    }
}