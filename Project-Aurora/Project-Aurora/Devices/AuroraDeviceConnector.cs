using Aurora.Settings;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Devices
{
    public class OldAuroraDeviceWrapper : IDevice
    {
        AuroraDeviceConnector Connector;
        public string DeviceName => Connector.GetConnectorName();

        public string DeviceDetails => string.Join(", ", Connector.Devices.Select(d => d.GetDeviceName()));

        public string DeviceUpdatePerformance => string.Join(", ", Connector.Devices.Select(d => d.GetDeviceUpdatePerformance()));

        public bool IsInitialized => Connector.IsInitialized();

        public VariableRegistry RegisteredVariables => Connector.GetRegisteredVariables();

        public OldAuroraDeviceWrapper(AuroraDeviceConnector connector)
        {
            Connector = connector;
        }

        public bool Initialize()
        {
            Connector.Initialize();
            return true;
        }

        public void Reset()
        {
            Connector.Reset();
        }

        public void Shutdown()
        {
            Connector.Shutdown();
        }

        public bool UpdateDevice(Dictionary<int, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            return true;
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            return true;
        }
    }
    public abstract class AuroraDeviceConnector
    {
        //private Dictionary<string, AuroraDevice> Devices = new Dictionary<string, AuroraDevice>();
        protected List<AuroraDevice> devices = new List<AuroraDevice>();
        public IReadOnlyList<AuroraDevice> Devices => devices.AsReadOnly();
        private bool isInitialized;
        public event EventHandler NewSuccessfulInitiation;
        private int DisconnectedDeviceCount = 0;
        private int UpdatedDeviceCount = 0;
        private SemaphoreSlim SingleThread = new SemaphoreSlim(1, 1);

        protected abstract string ConnectorName { get; }
        public string GetConnectorName() => ConnectorName;
        public virtual void Reset()
        {
            Shutdown();
            Initialize();
        }
        private void RegisterDeviceId (AuroraDevice dev)
        {
            UniqueDeviceId id = new UniqueDeviceId(this, dev);
            while (Devices.Where(d => d.id == id).Any())
            {
                id.Index++;
            }
            var usedIdConfig = Global.devicesLayout.DevicesConfig.Values.Where(c => c.Id == id);
            if(!usedIdConfig.Any())
            {
                dev.id = id;
            }
            else
            {
                dev.id = usedIdConfig.First().Id;
            }
            
        }

        /// <summary>
        /// Is called first. Initialize the device here
        /// </summary>
        public async void Initialize()
        {
            await SingleThread.WaitAsync();

            if (!IsInitialized() && !Global.Configuration.DevicesDisabled.Contains(GetType()))
            {
                Global.logger.Info("Start initializing Connector: " + GetConnectorName());
                try
                {
                    /*if (!Global.Configuration.devices_not_first_time.Contains(GetType()))
                    {
                        RunFirstTime();
                        Global.Configuration.devices_not_first_time.Add(GetType());
                    }*/
                    if (await Task.Run(() => InitializeImpl()))
                    {
                        DisconnectedDeviceCount = 0;
                        foreach (var device in Devices)
                        {
                            Global.Configuration.VarRegistry.Combine(device.GetRegisteredVariables());
                            device.ConnectionHandler += ConnectionHandling;
                            device.UpdateFinished += DeviceUpdated;
                            RegisterDeviceId(device);
                            device.Connect();
                        }
                        if (Devices.Any())
                        {
                            isInitialized = true;
                            NewSuccessfulInitiation?.Invoke(this, new EventArgs());
                        }
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.Info("Connector, " + GetConnectorName() + ", throwed exception:" + exc.ToString());
                }
                Global.logger.Info("Connector, " + GetConnectorName() + ", was" + (IsInitialized() ? "" : " not") + " initialized");
            }
            SingleThread.Release();

        }

        protected abstract bool InitializeImpl();
        protected virtual void RunFirstTime() { }

        private void ConnectionHandling(object sender, EventArgs args)
        {
            AuroraDevice device = sender as AuroraDevice;
            if (device.IsConnected())
            {
                DisconnectedDeviceCount--;
            }
            else
            {
                DisconnectedDeviceCount++;
            }
            if (DisconnectedDeviceCount == 0)
            {
                Shutdown();
            }
        }
        private void DeviceUpdated(object sender, EventArgs args)
        {
            AuroraDevice device = sender as AuroraDevice;
            UpdatedDeviceCount++;
            if (UpdatedDeviceCount == Devices.Count)
            {
                UpdateDevices();
            }
        }
        protected virtual void UpdateDevices()
        {

        }
        /// <summary>
        /// Is called last. Dispose of the devices here
        /// </summary>
        public async void Shutdown()
        {
            await SingleThread.WaitAsync();

            try
            {
                if (IsInitialized())
                {
                    foreach (var device in Devices)
                    {
                        device.Disconnect();
                    }
                    devices.Clear();
                    await Task.Run(() => ShutdownImpl());
                    isInitialized = false;
                    Global.logger.Info("Connector, " + GetConnectorName() + ", was shutdown");
                }
            }
            catch (Exception exc)
            {
                Global.logger.Info("Connector, " + GetConnectorName() + ", throwed exception:" + exc.ToString());
            }
            SingleThread.Release();
        }

        protected abstract void ShutdownImpl();


        public bool IsInitialized() => isInitialized;

        public string GetConnectorDetails() => isInitialized ?
                                                    ConnectorName + ": " + ConnectorSubDetails :
                                                    ConnectorName + ": Not Initialized";
        protected virtual string ConnectorSubDetails => "Initialized";

        protected void LogInfo(string s) => Global.logger.Info(s);
        protected void LogError(string s) => Global.logger.Error(s);

        private VariableRegistry variableRegistry;
        public virtual VariableRegistry GetRegisteredVariables()
        {
            if (variableRegistry == null)
            {
                variableRegistry = new VariableRegistry();
                RegisterVariables(variableRegistry);
                foreach (var dev in Devices)
                {
                    variableRegistry.Combine(dev.GetRegisteredVariables());
                }
            }
            return variableRegistry;
        }
        /// <summary>
        /// Only called once when registering variables. Can be empty if not needed
        /// </summary>
        protected virtual void RegisterVariables(VariableRegistry local)
        {
            //purposefully empty, if varibles are needed, this should be overridden
        }
        protected VariableRegistry GlobalVarRegistry => Global.Configuration.VarRegistry;

    }
}