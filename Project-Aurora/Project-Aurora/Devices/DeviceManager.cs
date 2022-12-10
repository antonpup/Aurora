using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Amib.Threading;
using CSScriptLib;

namespace Aurora.Devices
{
    public sealed class DeviceContainer : IDisposable
    {
        public IDevice Device { get; }

        private readonly SmartThreadPool _worker = new(1000, 1);

        private Tuple<DeviceColorComposition, bool> currentComp;

        public readonly object ActionLock = new();
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
            if (Device.InitializeTask == null)
            {
                return;
            }
            await Device.InitializeTask;
            lock(ActionLock)
            {
                Device.UpdateDevice(currentComp.Item1, doWorkEventArgs, currentComp.Item2);
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
        private const int RETRY_ATTEMPTS = 6;
        private bool _InitializeOnceAllowed;
        private bool suspended;
        private bool resumed;
        private int _retryAttempts = RETRY_ATTEMPTS;
        public int RetryAttempts
        {
            get => _retryAttempts;
            private set
            {
                _retryAttempts = value;
                RetryAttemptsChanged?.Invoke(this, null);
            }
        }

        public List<DeviceContainer> DeviceContainers { get; } = new();

        public IEnumerable<DeviceContainer> InitializedDeviceContainers => DeviceContainers.Where(d => d.Device.IsInitialized);

        public event EventHandler RetryAttemptsChanged;

        public DeviceManager()
        {
            AddDevicesFromAssembly();
            AddDevicesFromScripts(Path.Combine(Global.ExecutingDirectory, "Scripts", "Devices"));
            AddDevicesFromScripts(Path.Combine(Global.AppDataDirectory, "Scripts", "Devices"));
            AddDevicesFromDlls(Path.Combine(Global.ExecutingDirectory, "Plugins", "Devices"));
            AddDevicesFromDlls(Path.Combine(Global.AppDataDirectory, "Plugins", "Devices"));

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

            foreach (string device_script in files)
            {
                try
                {
                    string ext = Path.GetExtension(device_script);
                    switch (ext)
                    {
                        case ".py":
                            var scope = Global.PythonEngine.ExecuteFile(device_script);
                            dynamic main_type;
                            if (scope.TryGetVariable("main", out main_type))
                            {
                                dynamic script = Global.PythonEngine.Operations.CreateInstance(main_type);

                                IDevice scriptedDevice = new Devices.ScriptedDevice.ScriptedDevice(script);

                                DeviceContainers.Add(new DeviceContainer(scriptedDevice));
                                Global.logger.Info($"Loaded device script {device_script}");
                            }
                            else
                                Global.logger.Error("Script \"{0}\" does not contain a public 'main' class", device_script);

                            break;
                        case ".cs":
                            dynamic script_assembly = CSScript.Evaluator.LoadFile(device_script);
                            IDevice csDevice = new Devices.ScriptedDevice.ScriptedDevice(script_assembly);
                            DeviceContainers.Add(new DeviceContainer(csDevice));
                            Global.logger.Info($"Loaded device script {device_script}");
                            break;
                        default:
                            Global.logger.Error("Script with path {0} has an unsupported type/ext! ({1})", device_script, ext);
                            break;
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.Error("An error occured while trying to load script {0}. Exception: {1}", device_script, exc);
                }
            }
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
                InitializeDevices();
            }
        }

        public void InitializeDevices()
        {
            if (suspended)
                return;

            var devices = DeviceContainers
                .Select(dc => dc.Device)
                .Where(device => !device.IsInitialized && !Global.Configuration.DevicesDisabled.Contains(device.GetType()));
            foreach (var device in devices)
            {
                device.InitializeTask = Task.Run(device.Initialize);
                device.InitializeTask.ContinueWith(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        Global.logger.Info($"[Device][{device.DeviceName}] Initialized Successfully.");
                    }
                    else
                    {
                        Global.logger.Info($"[Device][{device.DeviceName}] Failed to initialize.");
                    }
                });
            }

            _InitializeOnceAllowed = false;
        }

        public void ShutdownDevices()
        {
            foreach (var dc in InitializedDeviceContainers)
            {
                lock (dc.ActionLock)
                {
                    DisableDevice(dc.Device);
                }
                Global.logger.Info($"[Device][{dc.Device.DeviceName}] Shutdown");
            }
        }

        public Task ResetDevices()
        {
            foreach (var dc in InitializedDeviceContainers)
            {
                dc.Device.InitializeTask = Task.Run(dc.Device.Reset);
            }
            
            return Task.WhenAll(InitializedDeviceContainers.Select(container => container.Device.InitializeTask));
        }

        public Task EnableDevice(IDevice device)
        {
            device.InitializeTask = Task.Run(device.Initialize);
            return device.InitializeTask;
        }

        public void DisableDevice(IDevice device)
        {
            device.InitializeTask = null;
            device.Shutdown();
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
                InitializeDevices();
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    Global.logger.Info("Suspending Devices");
                    suspended = true;
                    this.ShutdownDevices();
                    break;
                case PowerModes.Resume:
                    Global.logger.Info("Resuming Devices -- PowerModes.Resume");
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    resumed = true;
                    suspended = false;
                    this.InitializeDevices();
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
