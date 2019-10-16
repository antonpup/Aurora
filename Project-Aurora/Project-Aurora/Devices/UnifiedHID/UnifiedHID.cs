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
    class UnifiedHIDDevice : Device
    {
        private string devicename = "UnifiedHID";
        private bool isInitialized = false;
        private bool peripheral_updated = false;
        private readonly object action_lock = new object();
        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;
        private VariableRegistry default_registry = null;

        List<ISSDevice> AllDevices = new List<ISSDevice>();



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
                                       .ForEach(class_ => AllDevices.Add((ISSDevice)Activator.CreateInstance(class_)));
            }
            catch (Exception exc)
            {
                Global.logger.Error("UnifiedHID class could not be constructed: " + exc);
            }
        }

        List<ISSDevice> FoundDevices = new List<ISSDevice>();

        public bool Initialize()
        {
            lock (action_lock)
            {
                if (!isInitialized)
                {
                    this.FoundDevices.Clear();
                    try
                    {
                        foreach (ISSDevice dev in AllDevices)
                        {
                            if (dev.Connect())
                                FoundDevices.Add(dev);
                        }
                    }
                    catch (Exception e)
                    {
                        Global.logger.Error("UnifiedHID could not be initialized: " + e);
                        isInitialized = false;
                    }
                    if (FoundDevices.Count > 0)
                        isInitialized = true;
                }

                return isInitialized;
            }
        }

        public void Shutdown()
        {
            lock (action_lock)
            {
                try
                {
                    if (isInitialized)
                    {
                        foreach (ISSDevice dev in FoundDevices)
                        {
                            dev.Disconnect();
                        }
                        this.FoundDevices.Clear();
                        this.Reset();

                        isInitialized = false;
                    }
                }
                catch (Exception ex)
                {
                    Global.logger.Error("There was an error shutting down UnifiedHID: " + ex);
                    isInitialized = false;
                }

            }
        }

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                return devicename + ": Connected";
            }
            else
            {
                return devicename + ": Not connected";
            }
        }

        public string GetDeviceName()
        {
            return devicename;
        }

        public void Reset()
        {
            if (this.IsInitialized() && (peripheral_updated))
            {
                peripheral_updated = false;
            }
        }

        public bool Reconnect()
        {
            Shutdown();
            return Initialize();
        }

        public bool IsConnected()
        {
            return this.isInitialized;
        }

        public bool IsInitialized()
        {
            return this.isInitialized;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel) return false;
            try
            {
                List<Tuple<byte, byte, byte>> colors = new List<Tuple<byte, byte, byte>>();

                foreach (ISSDevice device in FoundDevices)
                {
                    if (e.Cancel) return false;

                    if (!device.IsKeyboard)
                    {
                        foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                        {
                            Color color = (Color)key.Value;
                            //Apply and strip Alpha
                            color = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));

                            if (e.Cancel) return false;
                            else if (Global.Configuration.allow_peripheral_devices && !Global.Configuration.devices_disable_mouse)
                            {
                                if (key.Key == DeviceKeys.Peripheral_Logo || key.Key == DeviceKeys.Peripheral_ScrollWheel || key.Key == DeviceKeys.Peripheral_FrontLight)
                                {
                                    device.SetLEDColour(key.Key, color.R, color.G, color.B);
                                }
                                peripheral_updated = true;
                            }
                            else
                            {
                                peripheral_updated = false;
                            }
                        }
                    }
                    else
                    {
                        if (!Global.Configuration.devices_disable_keyboard)
                        {
                            device.SetMultipleLEDColour(keyColors);
                            peripheral_updated = true;
                        }
                        else
                        {
                            peripheral_updated = false;
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
            watch.Restart();

            bool update_result = UpdateDevice(colorComposition.keyColors, e, forced);

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return update_result;
        }

        public bool IsPeripheralConnected()
        {
            return isInitialized;
        }

        public bool IsKeyboardConnected()
        {
            return isInitialized;
            //return false;
        }

        public string GetDeviceUpdatePerformance()
        {
            return (isInitialized ? lastUpdateTime + " ms" : "");
        }

        public VariableRegistry GetRegisteredVariables()
        {
            if (default_registry == null)
            {
                default_registry = new VariableRegistry();
                foreach (ISSDevice device in AllDevices)
                {
                    default_registry.Register($"UnifiedHID_{device.GetType().Name}_enable", false, $"Enable {(string.IsNullOrEmpty(device.PrettyName) ? device.GetType().Name : device.PrettyName)} in {devicename}");
                }
            }
            return default_registry;
        }

    }

    interface ISSDevice
    {
        bool IsConnected { get; }
        bool IsKeyboard { get; }
        string PrettyName { get; }
        bool Connect();
        bool Disconnect();
        bool SetLEDColour(DeviceKeys key, byte red, byte green, byte blue);
        bool SetMultipleLEDColour(Dictionary<DeviceKeys, Color> keyColors);
    }

    abstract class UnifiedBase : ISSDevice
    {
        protected HidDevice device;
        protected Dictionary<DeviceKeys, Func<byte, byte, byte, bool>> deviceKeyMap;
        public bool IsConnected { get; protected set; } = false;
        public bool IsKeyboard { get; protected set; } = false;
        public string PrettyName { get; protected set; }

        protected bool Connect(int vendorID, int[] productIDs, short usagePage)
        {
            IEnumerable<HidDevice> devices = HidDevices.Enumerate(vendorID, productIDs);

            if (devices.Count() > 0)
            {
                try
                {
                    device = devices.First(dev => dev.Capabilities.UsagePage == usagePage);
                    device.OpenDevice();
                    return (IsConnected = true);
                }
                catch (Exception exc)
                {
                    Global.logger.LogLine($"Error when attempting to open UnifiedHID device:\n{exc}", Logging_Level.Error);
                }
            }
            return false;
        }

        public abstract bool Connect();

        public virtual bool Disconnect()
        {
            try
            {
                device.CloseDevice();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public virtual bool SetLEDColour(DeviceKeys key, byte red, byte green, byte blue)
        {
            if (this.deviceKeyMap.TryGetValue(key, out Func<byte, byte, byte, bool> func))
                return func.Invoke(red, green, blue);

            return false;
        }

        public virtual bool SetMultipleLEDColour(Dictionary<DeviceKeys, Color> keyColors)
        {
            return false;
        }

    }

}