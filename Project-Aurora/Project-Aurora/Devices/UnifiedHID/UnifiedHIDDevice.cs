using Aurora.Settings;
using Aurora.Utils;
using HidLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Devices.UnifiedHID
{
    public class UnifiedHIDDevice : IDevice
    {
        private readonly object actionLock = new object();

        private VariableRegistry variableRegistry = null;
        private Stopwatch watch = new Stopwatch();
        private long lastUpdateTime = 0;

        List<UnifiedBase> allDevices = new List<UnifiedBase>();
        List<UnifiedBase> connectedDevices = new List<UnifiedBase>();

        public string DeviceName => "UnifiedHID";
        public bool IsInitialized => connectedDevices.Count != 0;
        public string DeviceDetails => IsInitialized ? "Initialized" : "Not Initialized";
        public string DeviceUpdatePerformance => (IsInitialized ? lastUpdateTime + " ms" : "");

        public VariableRegistry RegisteredVariables
        {
            get
            {
                if (variableRegistry == null)
                {
                    variableRegistry = new VariableRegistry();

                    variableRegistry.Register($"{DeviceName}_update_interval", 0, "Update interval", null, 0);
                    variableRegistry.Register($"{DeviceName}_enable_shutdown_color", false, "Enable shutdown color");
                    variableRegistry.Register($"{DeviceName}_shutdown_color", new Utils.RealColor(Color.FromArgb(255, 255, 255, 255)), "Shutdown color");

                    foreach (UnifiedBase device in allDevices)
                        variableRegistry.Register($"UnifiedHID_{device.GetType().Name}_enable", false, $"Enable {(string.IsNullOrEmpty(device.PrettyName) ? device.GetType().Name : device.PrettyName)} in {DeviceName}");
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
                                       .ForEach(class_ => allDevices.Add((UnifiedBase)Activator.CreateInstance(class_)));
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
                if (!IsInitialized)
                {
                    connectedDevices.Clear();
                    try
                    {
                        foreach (UnifiedBase dev in allDevices)
                        {
                            if (dev.Connect())
                            {
                                connectedDevices.Add(dev);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Global.logger.Error("UnifiedHID could not be initialized: " + e);
                    }
                }
            }

            return IsInitialized;
        }

        public void Shutdown()
        {
            lock (actionLock)
            {
                try
                {
                    if (IsInitialized)
                    {
                        var enableShutdownColor = Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_enable_shutdown_color");
                        var shutdownColor = Global.Configuration.VarRegistry.GetVariable<RealColor>($"{DeviceName}_shutdown_color").GetDrawingColor();

                        foreach (UnifiedBase dev in connectedDevices)
                        {
                            foreach (var map in dev.DeviceFuncMap)
                            {
                                if (enableShutdownColor)
                                    map.Value.Invoke(shutdownColor.R, shutdownColor.G, shutdownColor.B);
                            }

                            dev.Disconnect();
                        }

                        connectedDevices.Clear();
                    }
                }
                catch (Exception ex)
                {
                    Global.logger.Error("There was an error shutting down UnifiedHID: " + ex);
                }
            }
        }

        public void Reset()
        {
            Shutdown();
            Initialize();
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            try
            {
                foreach (UnifiedBase device in connectedDevices)
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

                                // Set color
                                device.SetColor(key.Key, key.Value.R, key.Value.G, key.Value.B);
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
}
