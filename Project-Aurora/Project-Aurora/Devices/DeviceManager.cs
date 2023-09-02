using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amib.Threading;
using Aurora.Devices.ScriptedDevice;
using RazerSdkReader;

namespace Aurora.Devices;

public sealed class DeviceContainer : IDisposable
{
    public IDevice Device { get; }

    private readonly SmartThreadPool _worker = new(1000, 1);

    private DeviceColorComposition _currentComp = new(new Dictionary<DeviceKeys, Color>());

    private readonly SemaphoreSlim _actionLock = new(1);
    private readonly Action _updateAction;

    public DeviceContainer(IDevice device)
    {
        Device = device;
        _worker.Name = device.DeviceName + " Thread Pool";
        var args = new DoWorkEventArgs(null);
        _updateAction = () =>
        {
            WorkerOnDoWork(args).Wait();
        };
    }

    private async Task WorkerOnDoWork(DoWorkEventArgs doWorkEventArgs)
    {
        if (Device is { IsInitialized: false, isDoingWork: false })
        {
            await Device.Initialize();
        }

        await _actionLock.WaitAsync();
        try
        {
            await Device.UpdateDevice(_currentComp, doWorkEventArgs);
        }
        finally
        {
            _actionLock.Release();
        }
    }

    public void UpdateDevice(DeviceColorComposition composition)
    {
        _currentComp = composition;
        if (_worker.WaitingCallbacks < 1 && !Device.isDoingWork)
        {
            _worker.QueueWorkItem(_updateAction);
        }
    }

    public async Task EnableDevice()
    {
        var initTask = await Device.Initialize();
        if (initTask)
        {
            Global.logger.Information("[Device][{DeviceName}] Initialized Successfully", Device.DeviceName);
        }
        else
        {
            Global.logger.Information("[Device][{DeviceName}] Failed to initialize", Device.DeviceName);
        }
    }

    public async Task DisableDevice()
    {
        await _actionLock.WaitAsync(500);
        try
        {
            await Device.ShutdownDevice();
            Global.logger.Information("[Device][{DeviceName}] Shutdown", Device.DeviceName);
        }
        finally
        {
            _actionLock.Release();
        }
    }

    public void Dispose()
    {
        _worker.Shutdown(250);
        _worker.Dispose();
    }
}

public sealed class DeviceManager: IDisposable
{
    private bool _suspended;
    private bool _resumed;
    private bool _disposed;

    public List<DeviceContainer> DeviceContainers { get; } = new();

    public IEnumerable<DeviceContainer> InitializedDeviceContainers => DeviceContainers.Where(d => d.Device.IsInitialized);

    private readonly Task<ChromaReader?> _rzSdkManager;

    public DeviceManager(Task<ChromaReader?> rzSdkManager)
    {
        _rzSdkManager = rzSdkManager;

        const string devicesPath = "Devices";
        IEnumerable<IDeviceLoader> deviceLoaders = new IDeviceLoader[]
        {
            new AssemblyDeviceLoader(),
            new ScriptDeviceLoader(Path.Combine(Global.ExecutingDirectory, "Scripts", devicesPath)),
            new ScriptDeviceLoader(Path.Combine(Global.AppDataDirectory, "Scripts", devicesPath)),
            new DllDeviceLoader(Path.Combine(Global.ExecutingDirectory, "Plugins", devicesPath)),
            new DllDeviceLoader(Path.Combine(Global.AppDataDirectory, "Plugins", devicesPath))
        };

        foreach (var deviceLoader in deviceLoaders)
        {
            foreach (var device in deviceLoader.LoadDevices().Where(d => d != null))
            {
                DeviceContainers.Add(new DeviceContainer(device!));
            }
            deviceLoader.Dispose();
        }

        SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
    }

    public void RegisterVariables()
    {
        foreach (var device in DeviceContainers)
        {
            Global.Configuration.VarRegistry.Combine(device.Device.RegisteredVariables);
        }
    }

    public async Task InitializeDevices()
    {
        if (_suspended)
            return;

        await _rzSdkManager;

        var initializeTasks = DeviceContainers
            .Where(dc => dc.Device is { IsInitialized: false, isDoingWork: false })
            .Where(dc => Global.Configuration.EnabledDevices.Contains(dc.Device.GetType()))
            .Select(deviceContainer => deviceContainer.EnableDevice());

        await Task.WhenAll(initializeTasks);
    }

    public Task ShutdownDevices()
    {
        var shutdownTasks = InitializedDeviceContainers.Select(dc => dc.DisableDevice());

        return Task.WhenAll(shutdownTasks);
    }

    public async Task ResetDevices()
    {
        var resetTasks = InitializedDeviceContainers.Select(dc => dc.Device.Reset()).ToList();

        await Task.WhenAll(resetTasks);
    }

    public void UpdateDevices(DeviceColorComposition composition)
    {
        if(_disposed)
            return;
        foreach (var dc in InitializedDeviceContainers)
        {
            dc.UpdateDevice(composition);
        }
    }

    #region SystemEvents

    private async void SystemEvents_PowerModeChanged(object? sender, PowerModeChangedEventArgs e)
    {
        switch (e.Mode)
        {
            case PowerModes.Suspend:
                Global.logger.Information("Suspending Devices");
                _suspended = true;
                await Task.Run(async () => await ShutdownDevices());
                break;
            case PowerModes.Resume:
                Global.logger.Information("Resuming Devices -- PowerModes.Resume");
                Thread.Sleep(TimeSpan.FromSeconds(5));
                _resumed = true;
                _suspended = false;
                await Task.Run(async () => await InitializeDevices());
                break;
        }
    }
    #endregion

    public void Dispose()
    {
        _disposed = true;
        DeviceContainers.Clear();
    }
}