using CSScriptLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Amib.Threading;

namespace Aurora.Devices
{
    public class DeviceContainer
    {
        public IDevice Device { get; }

        private SmartThreadPool Worker = new SmartThreadPool(1000, 1);

        private Tuple<DeviceColorComposition, bool> currentComp;

        public readonly object actionLock = new();
        private readonly Action _updateAction;

        public DeviceContainer(IDevice device)
        {
            Device = device;
            var args = new DoWorkEventArgs(null);
            _updateAction = () =>
            {
                WorkerOnDoWork(this, args);
            };
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            lock(actionLock)
            {
                Device.UpdateDevice(currentComp.Item1, doWorkEventArgs,
                currentComp.Item2);
            }
        }
        public void UpdateDevice(DeviceColorComposition composition, bool forced = false)
        {
            currentComp = new Tuple<DeviceColorComposition, bool>(composition, forced);
            if (Worker.WaitingCallbacks < 1)
            {
                Worker.QueueWorkItem(_updateAction);
            }
        }
    }

    public class DeviceManager
    {
        private const int RETRY_INTERVAL = 2000;
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

        public List<DeviceContainer> DeviceContainers { get; } = new List<DeviceContainer>();

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

                                IDevice scripted_device = new Devices.ScriptedDevice.ScriptedDevice(script);

                                DeviceContainers.Add(new DeviceContainer(scripted_device));
                                Global.logger.Info($"Loaded device script {device_script}");

                            }
                            else
                                Global.logger.Error("Script \"{0}\" does not contain a public 'main' class", device_script);

                            break;
                        case ".cs":
                            System.Reflection.Assembly script_assembly = CSScript.LoadFile(device_script);
                            foreach (Type typ in script_assembly.ExportedTypes)
                            {
                                dynamic script = Activator.CreateInstance(typ);

                                IDevice scripted_device = new Devices.ScriptedDevice.ScriptedDevice(script);

                                DeviceContainers.Add(new DeviceContainer(scripted_device));
                                Global.logger.Info($"Loaded device script {device_script}");
                            }

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

        private void RetryAll()
        {
            while (RetryAttempts > 0)
            {
                foreach (var dc in DeviceContainers)
                {
                    if (dc.Device.IsInitialized || Global.Configuration.DevicesDisabled.Contains(dc.Device.GetType()))
                        continue;
                    lock (dc.actionLock)
                        dc.Device.Initialize();
                }
                RetryAttempts--;
                Thread.Sleep(RETRY_INTERVAL);
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

            int devicesToRetry = 0;

            foreach (var dc in DeviceContainers)
            {
                if (dc.Device.IsInitialized || Global.Configuration.DevicesDisabled.Contains(dc.Device.GetType()))
                    continue;

                lock (dc.actionLock)
                {
                    if (!dc.Device.Initialize())
                        devicesToRetry++;
                }

                var s = $"[Device][{dc.Device.DeviceName}] ";

                if (dc.Device.IsInitialized)
                    s += "Initialized Successfully.";
                else
                    s += "Failed to initialize.";

                Global.logger.Info(s);
            }

            if (devicesToRetry > 0)
                Task.Run(RetryAll);

            _InitializeOnceAllowed = InitializedDeviceContainers.Any();
        }

        public void ShutdownDevices()
        {
            foreach (var dc in InitializedDeviceContainers)
            {
                lock (dc.actionLock)
                {
                    dc.Device.Shutdown();
                }
                Global.logger.Info($"[Device][{dc.Device.DeviceName}] Shutdown");
            }
        }

        public void ResetDevices()
        {
            foreach (var dc in InitializedDeviceContainers)
            {
                lock (dc.actionLock)
                {
                    dc.Device.Reset();
                }
            }
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
    }
}
