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

namespace Aurora.Devices
{
    public class DeviceContainer
    {
        public IDevice Device { get; set; }

        public BackgroundWorker Worker = new BackgroundWorker();
        public Thread UpdateThread { get; set; } = null;

        private Tuple<DeviceColorComposition, bool> currentComp = null;
        private bool newFrame = false;

        public DeviceContainer(IDevice device)
        {
            this.Device = device;
            Worker.DoWork += WorkerOnDoWork;
            Worker.RunWorkerCompleted += (sender, args) =>
            {
                lock (Worker)
                {
                    if (newFrame && !Worker.IsBusy)
                        Worker.RunWorkerAsync();
                }
            };
            //Worker.WorkerSupportsCancellation = true;
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            newFrame = false;
            Device.UpdateDevice(currentComp.Item1, doWorkEventArgs,
                currentComp.Item2);
        }

        public void UpdateDevice(DeviceColorComposition composition, bool forced = false)
        {
            newFrame = true;
            currentComp = new Tuple<DeviceColorComposition, bool>(composition, forced);
            lock (Worker)
            {
                if (Worker.IsBusy)
                    return;
                else
                    Worker.RunWorkerAsync();
            }
        }
    }

    public class DeviceManager
    {
        private const int RETRY_INTERVAL = 10000;
        private const int RETRY_ATTEMPTS = 5;
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
            AddDevicesFromScripts();

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        private void AddDevicesFromScripts()
        {
            string devices_scripts_path = System.IO.Path.Combine(Global.ExecutingDirectory, "Scripts", "Devices");

            if (!Directory.Exists(devices_scripts_path))
                return;

            foreach (string device_script in Directory.EnumerateFiles(devices_scripts_path, "*.*"))
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

        public void RegisterVariables()
        {
            foreach (var device in DeviceContainers)
            {
                Global.Configuration.VarRegistry.Combine(device.Device.GetRegisteredVariables());
            }
        }

        private void RetryAll()
        {
            while (RetryAttempts > 0)
            {
                foreach (var dc in DeviceContainers)
                {
                    if (dc.Device.IsInitialized || Global.Configuration.devices_disabled.Contains(dc.Device.GetType()))
                        continue;

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
                if (dc.Device.IsInitialized || Global.Configuration.devices_disabled.Contains(dc.Device.GetType()))
                    continue;

                if (!dc.Device.Initialize())
                    devicesToRetry++;

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
            foreach (DeviceContainer device in InitializedDeviceContainers)
            {
                device.Device.Shutdown();
                Global.logger.Info("Device, " + device.Device.DeviceName + ", was shutdown");
            }
        }

        public void ResetDevices()
        {
            foreach (DeviceContainer device in InitializedDeviceContainers)
            {
                device.Device.Reset();
            }
        }

        public void UpdateDevices(DeviceColorComposition composition, bool forced = false)
        {
            foreach (var deviceContainer in InitializedDeviceContainers)
            {
                deviceContainer.UpdateDevice(composition, forced);
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
