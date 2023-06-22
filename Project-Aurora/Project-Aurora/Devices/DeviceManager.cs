using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amib.Threading;
using System.Windows.Threading;
using Aurora.Devices.ScriptedDevice;

namespace Aurora.Devices;

public sealed class DeviceContainer : IDisposable
{
    public IDevice Device { get; }

    private readonly SmartThreadPool _worker = new(1000, 1);

    private Tuple<DeviceColorComposition, bool> _currentComp;

    private readonly SemaphoreSlim _actionLock = new(1);
    private readonly Action _updateAction;

    public DeviceContainer(IDevice device)
    {
        Device = device;
        _worker.Name = device.DeviceName + " Thread Pool";
        var args = new DoWorkEventArgs(null);
        _updateAction = () =>
        {
            WorkerOnDoWork(args);
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
            await Device.UpdateDevice(_currentComp.Item1, doWorkEventArgs, _currentComp.Item2);
        }
        finally
        {
            _actionLock.Release();
        }
    }

    public void UpdateDevice(DeviceColorComposition composition, bool forced = false)
    {
        _currentComp = new Tuple<DeviceColorComposition, bool>(composition, forced);
        if (_worker.WaitingCallbacks < 1 && !Device.isDoingWork)
        {
            _worker.QueueWorkItem(_updateAction);
        }
    }

    public async Task EnableDevice()
    {
        var initTask = Device.Initialize();
        Global.logger.Info(initTask is { IsCompletedSuccessfully: true, Result: true }
            ? $"[Device][{Device.DeviceName}] Initialized Successfully."
            : $"[Device][{Device.DeviceName}] Failed to initialize.");
    }

    public async Task DisableDevice()
    {
        _actionLock.WaitAsync(500);
        try
        {
            await Device.ShutdownDevice();
            Global.logger.Info($"[Device][{Device.DeviceName}] Shutdown");
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

    public List<DeviceContainer> DeviceContainers { get; } = new();

    public IEnumerable<DeviceContainer> InitializedDeviceContainers => DeviceContainers.Where(d => d.Device.IsInitialized);

    public DeviceManager()
    {
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
            foreach (var device in deviceLoader.LoadDevices())
            {
                DeviceContainers.Add(new DeviceContainer(device));
            }
        }

        SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
    }

    public void RegisterVariables()
    {
        foreach (var device in DeviceContainers)
        {
            Global.Configuration.VarRegistry.Combine(device.Device.RegisteredVariables);
        }
    }

    public Task InitializeDevices()
    {
        if (_suspended)
            return Task.CompletedTask;

        var initializeTasks = DeviceContainers
            .Where(dc => dc.Device is { IsInitialized: false, isDoingWork: false })
            .Where(dc => Global.Configuration.EnabledDevices.Contains(dc.Device.GetType()))
            .Select(deviceContainer => deviceContainer.EnableDevice());

        return Task.WhenAll(initializeTasks);
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

    public void UpdateDevices(DeviceColorComposition composition, bool forced = false)
    {
        foreach (var dc in InitializedDeviceContainers)
        {
            dc.UpdateDevice(composition, forced);
        }
    }

    #region SystemEvents
    private async void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        Global.logger.Info($"SessionSwitch triggered with {e.Reason}");
        if (!e.Reason.Equals(SessionSwitchReason.SessionUnlock) || (!_suspended && !_resumed)) return;
        Global.logger.Info("Resuming Devices -- Session Switch Session Unlock");
        _suspended = false;
        _resumed = false;
        Dispatcher.CurrentDispatcher.Invoke(async () => await InitializeDevices());
    }

    private async void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        switch (e.Mode)
        {
            case PowerModes.Suspend:
                Global.logger.Info("Suspending Devices");
                _suspended = true;
                Dispatcher.CurrentDispatcher.Invoke(async () => await ShutdownDevices());
                break;
            case PowerModes.Resume:
                Global.logger.Info("Resuming Devices -- PowerModes.Resume");
                Thread.Sleep(TimeSpan.FromSeconds(2));
                _resumed = true;
                _suspended = false;
                Dispatcher.CurrentDispatcher.Invoke(async () => await InitializeDevices());
                break;
        }
    }
    #endregion

    public void Dispose()
    {
        DeviceContainers.Clear();
    }
}