using Aurora.Profiles;
using CSScriptLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using Microsoft.Win32;

namespace Aurora.Devices
{
    public class DeviceContainer
    {
        public Device Device { get; set; }

        public BackgroundWorker Worker = new BackgroundWorker();
        public Thread UpdateThread { get; set; } = null;

        private Tuple<DeviceColorComposition, bool> currentComp = null;
        private bool newFrame = false;

        public DeviceContainer(Device device)
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
            /*lock (Worker)
            {
                try
                {
                    if (!Worker.IsBusy)
                        Worker.RunWorkerAsync();
                }
                catch(Exception e)
                {
                    Global.logger.LogLine(e.ToString(), Logging_Level.Error);
                }
            }*/
        }
    }

    public class DeviceManager : IDisposable
    {
        private List<DeviceContainer> devices = new List<DeviceContainer>();

        public DeviceContainer[] Devices { get { return devices.ToArray(); } }

        private bool anyInitialized = false;
        private bool retryActivated = false;
        private const int retryInterval = 10000;
        private const int retryAttemps = 5;
        private int retryAttemptsLeft = retryAttemps;
        private Thread retryThread;
        private bool suspended = false;

        private bool _InitializeOnceAllowed = false;

        public int RetryAttempts
        {
            get
            {
                return retryAttemptsLeft;
            }
        }
        public event EventHandler NewDevicesInitialized;

        public DeviceManager()
        {
            devices.Add(new DeviceContainer(new Devices.Logitech.LogitechDevice()));         // Logitech Device
            devices.Add(new DeviceContainer(new Devices.Corsair.CorsairDevice()));           // Corsair Device
            devices.Add(new DeviceContainer(new Devices.Razer.RazerDevice()));               // Razer Device
            devices.Add(new DeviceContainer(new Devices.Roccat.RoccatDevice()));             // Roccat Device
            devices.Add(new DeviceContainer(new Devices.Clevo.ClevoDevice()));               // Clevo Device
            devices.Add(new DeviceContainer(new Devices.CoolerMaster.CoolerMasterDevice())); // CoolerMaster Device
            devices.Add(new DeviceContainer(new Devices.AtmoOrbDevice.AtmoOrbDevice()));     // AtmoOrb Ambilight Device
            devices.Add(new DeviceContainer(new Devices.SteelSeries.SteelSeriesDevice()));   // SteelSeries Device
            devices.Add(new DeviceContainer(new Devices.UnifiedHID.UnifiedHIDDevice()));     // UnifiedHID Device
            devices.Add(new DeviceContainer(new Devices.Wooting.WootingDevice()));           // Wooting Device
            devices.Add(new DeviceContainer(new Devices.Creative.SoundBlasterXDevice()));    // SoundBlasterX Device
            devices.Add(new DeviceContainer(new Devices.LightFX.LightFxDevice()));           //Alienware
            devices.Add(new DeviceContainer(new Devices.Dualshock.DualshockDevice()));       //DualShock 4 Device
            devices.Add(new DeviceContainer(new Devices.Drevo.DrevoDevice()));               // Drevo Device
            string devices_scripts_path = System.IO.Path.Combine(Global.ExecutingDirectory, "Scripts", "Devices");

            if (Directory.Exists(devices_scripts_path))
            {
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

                                    Device scripted_device = new Devices.ScriptedDevice.ScriptedDevice(script);

                                    devices.Add(new DeviceContainer(scripted_device));
                                }
                                else
                                    Global.logger.Error("Script \"{0}\" does not contain a public 'main' class", device_script);

                                break;
                            case ".cs":
                                System.Reflection.Assembly script_assembly = CSScript.LoadFile(device_script);
                                foreach (Type typ in script_assembly.ExportedTypes)
                                {
                                    dynamic script = Activator.CreateInstance(typ);

                                    Device scripted_device = new Devices.ScriptedDevice.ScriptedDevice(script);

                                    devices.Add(new DeviceContainer(scripted_device));
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

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }
        bool resumed = false;
        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            Global.logger.Info($"SessionSwitch triggered with {e.Reason}");
            if (e.Reason.Equals(SessionSwitchReason.SessionUnlock) && (suspended || resumed))
            {
                Global.logger.Info("Resuming Devices -- Session Switch Session Unlock");
                suspended = false;
                resumed = false;
                this.Initialize(true);
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    Global.logger.Info("Suspending Devices");
                    suspended = true;
                    this.Shutdown();
                    break;
                case PowerModes.Resume:
                    Global.logger.Info("Resuming Devices -- PowerModes.Resume");
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    resumed = true;
                    suspended = false;
                    this.Initialize();
                    break;
            }
        }

        public void RegisterVariables()
        {
            //Register any variables
            foreach (var device in devices)
                Global.Configuration.VarRegistry.Combine(device.Device.GetRegisteredVariables());
        }

        public void Initialize(bool forceRetry = false)
        {
            if (suspended)
                return;

            int devicesToRetryNo = 0;
            foreach (DeviceContainer device in devices)
            {
                if (device.Device.IsInitialized() || Global.Configuration.devices_disabled.Contains(device.Device.GetType()))
                    continue;

                if (device.Device.Initialize())
                    anyInitialized = true;
                else
                    devicesToRetryNo++;

                Global.logger.Info("Device, " + device.Device.GetDeviceName() + ", was" + (device.Device.IsInitialized() ? "" : " not") + " initialized");
            }


            if (anyInitialized)
            {
                _InitializeOnceAllowed = true;
                NewDevicesInitialized?.Invoke(this, new EventArgs());
            }

            if (devicesToRetryNo > 0 && (retryThread == null || forceRetry || retryThread?.ThreadState == System.Threading.ThreadState.Stopped))
            {
                retryActivated = true;
                if (forceRetry)
                    retryThread?.Abort();
                retryThread = new Thread(RetryInitialize);
                retryThread.Start();
                return;
            }
        }

        private void RetryInitialize()
        {
            if (suspended)
                return;
            for (int try_count = 0; try_count < retryAttemps; try_count++)
            {
                Global.logger.Info("Retrying Device Initialization");
                if (suspended)
                    continue;
                int devicesAttempted = 0;
                bool _anyInitialized = false;
                foreach (DeviceContainer device in devices)
                {
                    if (device.Device.IsInitialized() || Global.Configuration.devices_disabled.Contains(device.Device.GetType()))
                        continue;

                    devicesAttempted++;
                    if (device.Device.Initialize())
                        _anyInitialized = true;

                    Global.logger.Info("Device, " + device.Device.GetDeviceName() + ", was" + (device.Device.IsInitialized() ? "" : " not") + " initialized");
                }

                retryAttemptsLeft--;

                //We don't need to continue the loop if we aren't trying to initialize anything
                if (devicesAttempted == 0)
                    break;

                //There is only a state change if something suddenly becomes initialized
                if (_anyInitialized)
                {
                    NewDevicesInitialized?.Invoke(this, new EventArgs());
                    anyInitialized = true;
                }

                Thread.Sleep(retryInterval);
            }
        }

        public void InitializeOnce()
        {
            if (!anyInitialized && _InitializeOnceAllowed)
                Initialize();
        }

        public bool AnyInitialized()
        {
            return anyInitialized;
        }

        public Device[] GetInitializedDevices()
        {
            List<Device> ret = new List<Device>();

            foreach (DeviceContainer device in devices)
            {
                if (device.Device.IsInitialized())
                {
                    ret.Add(device.Device);
                }
            }

            return ret.ToArray();
        }

        public void Shutdown()
        {
            foreach (DeviceContainer device in devices)
            {
                if (device.Device.IsInitialized())
                {
                    device.Device.Shutdown();
                    Global.logger.Info("Device, " + device.Device.GetDeviceName() + ", was shutdown");
                }
            }

            anyInitialized = false;
        }

        public void ResetDevices()
        {
            foreach (DeviceContainer device in devices)
            {
                if (device.Device.IsInitialized())
                {
                    device.Device.Reset();
                }
            }
        }

        public void UpdateDevices(DeviceColorComposition composition, bool forced = false)
        {
            foreach (DeviceContainer device in devices)
            {
                if (device.Device.IsInitialized())
                {
                    if (Global.Configuration.devices_disabled.Contains(device.Device.GetType()))
                    {
                        //Initialized when it's supposed to be disabled? SMACK IT!
                        device.Device.Shutdown();
                        continue;
                    }
                    
                    device.UpdateDevice(composition, forced);
                }
            }
        }

        public string GetDevices()
        {
            string devices_info = "";

            foreach (DeviceContainer device in devices)
                devices_info += device.Device.GetDeviceDetails() + "\r\n";

            if (retryAttemptsLeft > 0)
                devices_info += "Retries: " + retryAttemptsLeft + "\r\n";

            return devices_info;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).

                    if (retryThread != null)
                    {
                        retryThread.Abort();
                        retryThread = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DeviceManager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
