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

namespace Aurora.Devices
{
    public sealed class DeviceContainer : IDisposable
    {
        public IDevice Device { get; }

        private readonly SmartThreadPool _worker = new(1000, 1);

        private Tuple<DeviceColorComposition, bool> currentComp;

        public readonly SemaphoreSlim ActionLock = new(1);
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

        private async void WorkerOnDoWork(DoWorkEventArgs doWorkEventArgs)
        {
            if (!Device.IsInitialized)
            {
                await Device.Initialize().ConfigureAwait(false);
            }

            await ActionLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await Device.UpdateDevice(currentComp.Item1, doWorkEventArgs, currentComp.Item2).ConfigureAwait(false);
            }
            finally
            {
                ActionLock.Release();
            }
        }

        public void UpdateDevice(DeviceColorComposition composition, bool forced = false)
        {
            currentComp = new Tuple<DeviceColorComposition, bool>(composition, forced);
            if (_worker.WaitingCallbacks < 1)
            {
                _worker.QueueWorkItem(_updateAction);
            }
        }

        public void Dispose()
        {
            _worker.Shutdown(250);
            _worker?.Dispose();
        }
    }

    public sealed class DeviceManager: IDisposable
    {
        private bool _InitializeOnceAllowed;
        private bool suspended;
        private bool resumed;

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
            dynamic scriptAssembly = Assembly.Load(deviceScript);
            Type typ = scriptAssembly.ExportedTypes[1];
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
            var deviceTypes = from type in Assembly.GetExecutingAssembly().GetTypes()
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

        private void AddDevicesFromDlls(string dllFolder)
        {
            if (!Directory.Exists(dllFolder))
                Directory.CreateDirectory(dllFolder);

            var files = Directory.GetFiles(dllFolder, "*.dll");
            if (files.Length == 0)
                return;

            Global.logger.Info($"Loading devices plugins from {dllFolder}");

            deviceAssemblies = new List<Assembly>();
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
                            deviceAssemblies.Add(deviceAssembly);
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

        private List<Assembly> deviceAssemblies;

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.RequestingAssembly != null && deviceAssemblies.Contains(args.RequestingAssembly))
            {
                var searchDir = Path.GetDirectoryName(args.RequestingAssembly.Location);
                foreach (var file in Directory.GetFiles(searchDir, "*.dll"))
                {
                    var assemblyName = AssemblyName.GetAssemblyName(file);
                    if (assemblyName.FullName == args.Name)
                    {
                        return AppDomain.CurrentDomain.Load(assemblyName);
                    }
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

        public void InitializeOnce()
        {
            if (!InitializedDeviceContainers.Any() && _InitializeOnceAllowed)
            {
                Task.Run(async () => await InitializeDevices());
            }
        }

        public async Task InitializeDevices()
        {
            if (suspended)
                return;

            var devices = DeviceContainers
                .Select(dc => dc.Device)
                .Where(device => !device.IsInitialized && !Global.Configuration.DevicesDisabled.Contains(device.GetType()));
            var initializeTasks = new List<Task>();
            foreach (var device in devices)
            {
                var task = device.Initialize()
                    .ContinueWith(initTask =>
                    {
                        if (initTask.IsCompletedSuccessfully && initTask.Result)
                        {
                            Global.logger.Info($"[Device][{device.DeviceName}] Initialized Successfully.");
                        }
                        else
                        {
                            Global.logger.Info($"[Device][{device.DeviceName}] Failed to initialize.");
                        }
                    });
                task.ConfigureAwait(false);

                initializeTasks.Add(task);
            }

            _InitializeOnceAllowed = false;
            await Task.WhenAll(initializeTasks).ConfigureAwait(false);
        }

        public async Task ShutdownDevices()
        {
            var shutdownTasks = new List<Task>();
            foreach (var dc in InitializedDeviceContainers)
            {
                var task = Task.Run(async () =>
                {
                    await dc.ActionLock.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        await DisableDevice(dc.Device).ConfigureAwait(false);
                    }
                    finally
                    {
                        dc.ActionLock.Release();
                    }
                    Global.logger.Info($"[Device][{dc.Device.DeviceName}] Shutdown");
                });
                task.ConfigureAwait(false);

                shutdownTasks.Add(task);
            }

            await Task.WhenAll(shutdownTasks).ConfigureAwait(false);
        }

        public async Task ResetDevices()
        {
            List<Task> resetTasks = new();
            foreach (var dc in InitializedDeviceContainers)
            {
                var task = dc.Device.Reset();
                task.ConfigureAwait(false);
                resetTasks.Add(task);
            }

            await Task.WhenAll(resetTasks).ConfigureAwait(false);
        }

        public async Task EnableDevice(IDevice device)
        {
            await device.Initialize().ConfigureAwait(false);
        }

        public async Task DisableDevice(IDevice device)
        {
            await device.Shutdown().ConfigureAwait(false);
        }

        public void UpdateDevices(DeviceColorComposition composition, bool forced = false)
        {
            foreach (var dc in InitializedDeviceContainers)
            {
                dc.UpdateDevice(composition, forced);
            }
        }

        #region SystemEvents
        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            Global.logger.Info($"SessionSwitch triggered with {e.Reason}");
            if (e.Reason.Equals(SessionSwitchReason.SessionUnlock) && (suspended || resumed))
            {
                Global.logger.Info("Resuming Devices -- Session Switch Session Unlock");
                suspended = false;
                resumed = false;
                Dispatcher.CurrentDispatcher.Invoke(async () => await InitializeDevices());
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    Global.logger.Info("Suspending Devices");
                    suspended = true;
                    Dispatcher.CurrentDispatcher.Invoke(async () => await this.ShutdownDevices());
                    break;
                case PowerModes.Resume:
                    Global.logger.Info("Resuming Devices -- PowerModes.Resume");
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    resumed = true;
                    suspended = false;
                    Dispatcher.CurrentDispatcher.Invoke(async () => await this.InitializeDevices());
                    break;
            }
        }
        #endregion

        public void Dispose()
        {
            DeviceContainers.Clear();
        }
    }
}
