using Aurora.Profiles;
using CSScriptLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;

namespace Aurora.Devices
{
    public class DeviceManager : IDisposable
    {
        private List<Device> devices = new List<Device>();

        public Device[] Devices { get { return devices.ToArray(); } }

        private bool anyInitialized = false;
        private bool retryActivated = false;
        private const int retryInterval = 5000;
        private const int retryAttemps = 15;
        private int retryAttemptsLeft = retryAttemps;
        private Thread retryThread;

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
            devices.Add(new Devices.Logitech.LogitechDevice());         // Logitech Device
            devices.Add(new Devices.Corsair.CorsairDevice());           // Corsair Device
            devices.Add(new Devices.Razer.RazerDevice());               // Razer Device
            //devices.Add(new Devices.Roccat.RoccatDevice());             // Roccat Device
            devices.Add(new Devices.Clevo.ClevoDevice());               // Clevo Device
            devices.Add(new Devices.CoolerMaster.CoolerMasterDevice()); // CoolerMaster Device
            devices.Add(new Devices.AtmoOrbDevice.AtmoOrbDevice());     // AtmoOrb Ambilight Device
            devices.Add(new Devices.SteelSeries.SteelSeriesDevice());   // SteelSeries Device


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

                                    devices.Add(scripted_device);
                                }
                                else
                                    Global.logger.LogLine(string.Format("Script \"{0}\" does not contain a public 'main' class", device_script), Logging_Level.External);

                                break;
                            case ".cs":
                                System.Reflection.Assembly script_assembly = CSScript.LoadCodeFrom(device_script);
                                foreach (Type typ in script_assembly.ExportedTypes)
                                {
                                    dynamic script = Activator.CreateInstance(typ);

                                    Device scripted_device = new Devices.ScriptedDevice.ScriptedDevice(script);

                                    devices.Add(scripted_device);
                                }

                                break;
                            default:
                                Global.logger.LogLine(string.Format("Script with path {0} has an unsupported type/ext! ({1})", device_script, ext), Logging_Level.External);
                                break;
                        }
                    }
                    catch (Exception exc)
                    {
                        Global.logger.LogLine(string.Format("An error occured while trying to load script {0}. Exception: {1}", device_script, exc, Logging_Level.External));
                    }
                }
            }
        }

        public void RegisterVariables()
        {
            //Register any variables
            foreach (var device in devices)
                Global.Configuration.VarRegistry.Combine(device.GetRegisteredVariables());
        }

        public void Initialize()
        {
            int devicesToRetryNo = 0;
            foreach (Device device in devices)
            {
                if (device.IsInitialized() || Global.Configuration.devices_disabled.Contains(device.GetType()))
                    continue;

                if (device.Initialize())
                    anyInitialized = true;
                else
                    devicesToRetryNo++;

                Global.logger.LogLine("Device, " + device.GetDeviceName() + ", was" + (device.IsInitialized() ? "" : " not") + " initialized", Logging_Level.Info);
            }

            NewDevicesInitialized?.Invoke(this, new EventArgs());

            if (devicesToRetryNo > 0 && !retryActivated)
            {
                retryThread = new Thread(RetryInitialize);
                retryThread.Start();

                retryActivated = true;
            }

            _InitializeOnceAllowed = true;
        }

        private void RetryInitialize()
        {
            for (int try_count = 0; try_count < retryAttemps; try_count++)
            {
                Global.logger.LogLine("Retrying Device Initialization", Logging_Level.Info);
                int devicesAttempted = 0;
                bool _anyInitialized = false;
                foreach (Device device in devices)
                {
                    if (device.IsInitialized() || Global.Configuration.devices_disabled.Contains(device.GetType()))
                        continue;

                    devicesAttempted++;
                    if (device.Initialize())
                        _anyInitialized = true;

                    Global.logger.LogLine("Device, " + device.GetDeviceName() + ", was" + (device.IsInitialized() ? "" : " not") + " initialized", Logging_Level.Info);
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

            foreach (Device device in devices)
            {
                if (device.IsInitialized())
                {
                    ret.Add(device);
                }
            }

            return ret.ToArray();
        }

        public void Shutdown()
        {
            foreach (Device device in devices)
            {
                if (device.IsInitialized())
                {
                    device.Shutdown();
                    Global.logger.LogLine("Device, " + device.GetDeviceName() + ", was shutdown", Logging_Level.Info);
                }
            }

            anyInitialized = false;
        }

        public void ResetDevices()
        {
            foreach (Device device in devices)
            {
                if (device.IsInitialized())
                {
                    device.Reset();
                }
            }
        }

        public bool UpdateDevices(DeviceColorComposition composition, bool forced = false)
        {
            bool anyUpdated = false;

            foreach (Device device in devices)
            {
                if (device.IsInitialized())
                {
                    if (Global.Configuration.devices_disabled.Contains(device.GetType()))
                    {
                        //Initialized when it's supposed to be disabled? SMACK IT!
                        device.Shutdown();
                        continue;
                    }

                    if (device.UpdateDevice(composition, forced))
                        anyUpdated = true;
                }
            }

            return anyUpdated;
        }

        public string GetDevices()
        {
            string devices_info = "";

            foreach (Device device in devices)
                devices_info += device.GetDeviceDetails() + "\r\n";

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
