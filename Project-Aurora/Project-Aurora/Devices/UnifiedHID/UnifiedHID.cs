using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aurora.Settings;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;
using System.ComponentModel;
using System.Reflection;

namespace Aurora.Devices.UnifiedHID
{
    class UnifiedHIDDevice : IDevice
    {
        private string deviceName = "UnifiedHID";
        private bool isInitialized = false;

        private readonly object actionLock = new object();

        private VariableRegistry variableRegistry = null;

        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

        List<ISSDevice> allDevices = new List<ISSDevice>();
        List<ISSDevice> foundDevices = new List<ISSDevice>();

        public string DeviceName => deviceName;
        public bool IsInitialized => isInitialized;
        public string DeviceDetails => IsInitialized ? "Initialized" : "Not Initialized";
        public string DeviceUpdatePerformance => (isInitialized ? lastUpdateTime + " ms" : "");

        public VariableRegistry RegisteredVariables
        {
            get
            {
                if (variableRegistry == null)
                {
                    variableRegistry = new VariableRegistry();

                    variableRegistry.Register($"{DeviceName}_update_interval", 0, "Update interval", null, 0);

                    foreach (ISSDevice device in allDevices)
                        variableRegistry.Register($"UnifiedHID_{device.GetType().Name}_enable", false, $"Enable {(string.IsNullOrEmpty(device.PrettyName) ? device.GetType().Name : device.PrettyName)} in {deviceName}");
                }

                return variableRegistry;
            }
        }

        public UnifiedHIDDevice()
        {
            //Copied GetLoadableTypes from https://haacked.com/archive/2012/07/23/get-all-types-in-an-assembly.aspx/
            IEnumerable<Type> GetLoadableTypes(Assembly assembly)
            {
                if (assembly == null) throw new ArgumentNullException(nameof(assembly));
                try
                {
                    return assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    return e.Types.Where(t => t != null);
                }
            }

            try
            {
                AppDomain.CurrentDomain.GetAssemblies()
                                       .SelectMany(assembly => GetLoadableTypes(assembly))
                                       .Where(type => type.IsSubclassOf(typeof(UnifiedBase))).ToList()
                                       .ForEach(class_ => allDevices.Add((ISSDevice)Activator.CreateInstance(class_)));
            }
            catch (Exception exc)
            {
                Global.logger.Error("UnifiedHID class could not be constructed: " + exc);
            }
        }

        public bool Initialize()
        {
            lock (actionLock)
            {
                if (!isInitialized)
                {
                    this.foundDevices.Clear();
                    try
                    {
                        foreach (ISSDevice dev in allDevices)
                        {
                            if (dev.Connect())
                            {
                                foundDevices.Add(dev);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Global.logger.Error("UnifiedHID could not be initialized: " + e);
                    }

                    if (foundDevices.Count > 0)
                        isInitialized = true;
                }

                return isInitialized;
            }
        }

        public void Shutdown()
        {
            lock (actionLock)
            {
                try
                {
                    if (isInitialized)
                    {
                        foreach (ISSDevice dev in foundDevices)
                        {
                            dev.Disconnect();
                        }

                        foundDevices.Clear();
                    }
                }
                catch (Exception ex)
                {
                    Global.logger.Error("There was an error shutting down UnifiedHID: " + ex);
                }

                isInitialized = false;
            }
        }

        public void Reset()
        { }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            try
            {
                foreach (ISSDevice device in foundDevices)
                {
                    foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                    {
                        if (e.Cancel) return false;

                        if (Global.Configuration.AllowPeripheralDevices && !Global.Configuration.DevicesDisableMouse)
                        {
                            if (device.DeviceColorMap.TryGetValue(key.Key, out Color currentColor) && currentColor != key.Value)
                            {
                                // Update current colour
                                device.DeviceColorMap[key.Key] = key.Value;

                                // Set LED colour
                                device.SetLEDColour(key.Key, key.Value.R, key.Value.G, key.Value.B);
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Global.logger.Error("UnifiedHID, error when updating device: " + ex);
                return false;
            }
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            var sleep = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_update_interval");

            watch.Stop();
            var lastUpdateTime = watch.ElapsedMilliseconds;

            if (lastUpdateTime > sleep)
            {
                watch.Restart();
                this.lastUpdateTime = lastUpdateTime;
                return UpdateDevice(colorComposition.keyColors, e, forced);
            }
            else
            {
                watch.Start();
                return !e.Cancel;
            }
        }
    }

    interface ISSDevice
    {
        Dictionary<DeviceKeys, Func<byte, byte, byte, bool>> DeviceFuncMap { get; }
        Dictionary<DeviceKeys, Color> DeviceColorMap { get; }
        bool IsConnected { get; }
        string PrettyName { get; }
        bool Connect();
        bool Disconnect();
        bool SetLEDColour(DeviceKeys key, byte red, byte green, byte blue);
    }

    abstract class UnifiedBase : ISSDevice
    {
        protected HidDevice device;

        public Dictionary<DeviceKeys, Func<byte, byte, byte, bool>> DeviceFuncMap { get; protected set; } = new Dictionary<DeviceKeys, Func<byte, byte, byte, bool>>();
        public Dictionary<DeviceKeys, Color> DeviceColorMap { get; protected set; } = new Dictionary<DeviceKeys, Color>();
        public bool IsConnected { get; protected set; } = false;
        public string PrettyName { get; protected set; } = "";

        protected bool Connect(int vendorID, int[] productIDs, short usagePage)
        {
            IEnumerable<HidDevice> devices = HidDevices.Enumerate(vendorID, productIDs);

            if (devices.Count() > 0)
            {
                try
                {
                    device = devices.First(dev => dev.Capabilities.UsagePage == usagePage);
                    device.OpenDevice();

                    DeviceColorMap.Clear();

                    foreach (var key in DeviceFuncMap)
                    {
                        // Set black as default color
                        DeviceColorMap.Add(key.Key, Color.Black);
                    }

                    IsConnected = true;
                }
                catch (Exception exc)
                {
                    Global.logger.LogLine($"Error when attempting to open UnifiedHID device:\n{exc}", Logging_Level.Error);
                    IsConnected = false;
                }
            }

            return IsConnected;
        }

        public abstract bool Connect();

        public virtual bool Disconnect()
        {
            try
            {
                device.CloseDevice();
                IsConnected = false;
            }
            catch
            {
                IsConnected = true;
            }

            return !IsConnected;
        }

        public virtual bool SetLEDColour(DeviceKeys key, byte red, byte green, byte blue)
        {
            if (IsConnected && DeviceFuncMap.TryGetValue(key, out Func<byte, byte, byte, bool> func))
                return func.Invoke(red, green, blue);

            return false;
        }
    }
}
