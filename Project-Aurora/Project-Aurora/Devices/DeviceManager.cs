using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Amib.Threading;
using CSScriptLib;
using System.Windows.Threading;

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
        AddDevicesFromAssembly();
        const string devicesPath = "Devices";
        AddDevicesFromScripts(Path.Combine(Global.ExecutingDirectory, "Scripts", devicesPath));
        AddDevicesFromScripts(Path.Combine(Global.AppDataDirectory, "Scripts", devicesPath));
        AddDevicesFromDlls(Path.Combine(Global.ExecutingDirectory, "Plugins", devicesPath));
        AddDevicesFromDlls(Path.Combine(Global.AppDataDirectory, "Plugins", devicesPath));

        SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
    }

    private void AddDevicesFromScripts(string scriptFolder)
    {
        if (!Directory.Exists(scriptFolder))
            Directory.CreateDirectory(scriptFolder);

        var files = Directory.GetFiles(scriptFolder);
        if (files.Length == 0)
            return;

        Global.logger.Info($"Loading device scripts from {scriptFolder}");

        foreach (var deviceScript in files)
        {
            try
            {
                LoadScript(deviceScript);
            }
            catch (Exception exc)
            {
                Global.logger.Error(exc, "An error occured while trying to load script {Path}", deviceScript);
            }
        }
    }

    private void LoadScript(string deviceScript)
    {
        var ext = Path.GetExtension(deviceScript);
        switch (ext)
        {
            case ".py":
                LoadPython(deviceScript);
                break;
            case ".cs":
                CompileCs(deviceScript);
                break;
            case ".dll":
                LoadDll(deviceScript);
                break;
            default:
                Global.logger.Error("Script with path {Path} has an unsupported type/ext! ({Extension})",
                    deviceScript, ext);
                break;
        }
    }

    private void LoadPython(string deviceScript)
    {
        var scope = Global.PythonEngine.ExecuteFile(deviceScript);
        if (scope.TryGetVariable("main", out var mainType))
        {
            var script = Global.PythonEngine.Operations.CreateInstance(mainType);

            IDevice scriptedDevice = new Devices.ScriptedDevice.ScriptedDevice(script);

            DeviceContainers.Add(new DeviceContainer(scriptedDevice));
            Global.logger.Info($"Loaded device script {deviceScript}");
        }
        else
            Global.logger.Error("Script \"{Script}\" does not contain a public 'main' class", deviceScript);
    }

    private static void CompileCs(string deviceScript)
    {
        CSScript.RoslynEvaluator.CompileAssemblyFromFile(deviceScript, deviceScript + ".dll");
        File.Delete(deviceScript);
        MessageBox.Show(deviceScript + " is compiled. Aurora will crash but script will be loaded next time.");
    }

    private void LoadDll(string deviceScript)
    {
        byte[] data = File.ReadAllBytes(deviceScript);
        Assembly scriptAssembly = Assembly.Load(data);
        Type typ = scriptAssembly.ExportedTypes.First( type => type.FullName?.StartsWith("css_root+") ?? false);
        var constructorInfo = typ.GetConstructor(Type.EmptyTypes);
        if (constructorInfo == null)
        {
            Global.logger.Info($"Script {deviceScript} does not have parameterless constructor or device class isn't the first one");
            return;
        }
        dynamic script = Activator.CreateInstance(typ);
        IDevice scriptedDevice = new Devices.ScriptedDevice.ScriptedDevice(script);

        DeviceContainers.Add(new DeviceContainer(scriptedDevice));
        Global.logger.Info($"Loaded device script {deviceScript}");
    }

    private void AddDevicesFromAssembly()
    {
        Global.logger.Info("Loading devices from assembly...");
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var deviceTypes = from type in assembly.GetTypes()
                where typeof(IDevice).IsAssignableFrom(type)
                      && !type.IsAbstract
                      && type != typeof(ScriptedDevice.ScriptedDevice)
                let inst = (IDevice)Activator.CreateInstance(type)
                orderby inst.DeviceName
                select inst;

            foreach (var inst in deviceTypes)
            {
                DeviceContainers.Add(new DeviceContainer(inst));
            }
        }
    }

    private void AddDevicesFromDlls(string dllFolder)
    {
        if (!Directory.Exists(dllFolder))
            Directory.CreateDirectory(dllFolder);

        var files = Directory.GetFiles(dllFolder, "*.dll");
        if (files.Length == 0)
            return;

        Global.logger.Info($"Loading devices plugins from {dllFolder}");

        _deviceAssemblies = new List<Assembly>();
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        foreach (var deviceDll in files)
        {
            try
            {
                var deviceAssembly = Assembly.LoadFile(deviceDll);

                foreach (var type in deviceAssembly.GetExportedTypes())
                {
                    if (typeof(IDevice).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        _deviceAssemblies.Add(deviceAssembly);
                        IDevice devDll = (IDevice)Activator.CreateInstance(type);

                        DeviceContainers.Add(new DeviceContainer(devDll));

                        Global.logger.Info($"Loaded device plugin {deviceDll}");
                    }
                }
            }
            catch (Exception e)
            {
                Global.logger.Error($"Error loading device dll: {deviceDll}. Exception: {e.Message}");
            }
        }
    }

    private List<Assembly> _deviceAssemblies;

    private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        if (args.RequestingAssembly == null || !_deviceAssemblies.Contains(args.RequestingAssembly)) return null;
        var searchDir = Path.GetDirectoryName(args.RequestingAssembly.Location);
        foreach (var file in Directory.GetFiles(searchDir, "*.dll"))
        {
            var assemblyName = AssemblyName.GetAssemblyName(file);
            if (assemblyName.FullName == args.Name)
            {
                return AppDomain.CurrentDomain.Load(assemblyName);
            }
        }
        foreach (var file in Directory.GetFiles(Path.Combine(searchDir, "x64"), "*.dll"))
        {
            var assemblyName = AssemblyName.GetAssemblyName(file);
            if (assemblyName.FullName == args.Name)
            {
                return AppDomain.CurrentDomain.Load(assemblyName);
            }
        }
        return null;
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