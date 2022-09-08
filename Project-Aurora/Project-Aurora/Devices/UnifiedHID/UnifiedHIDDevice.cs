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
    public class UnifiedHIDDevice : DefaultDevice
    {
        private Stopwatch sleepWatch = new Stopwatch();
        private long lastUpdateTime = 0;
        private long lastSleepUpdateTime = 0;

        List<UnifiedBase> allDevices = new List<UnifiedBase>();
        List<UnifiedBase> connectedDevices = new List<UnifiedBase>();

        public override string DeviceName => "UnifiedHID";
        protected string DeviceInfo => string.Join(", ", connectedDevices.Select(hd => hd.PrettyName));

        public string DeviceDetails => IsInitialized
            ? $"Initialized{(string.IsNullOrWhiteSpace(DeviceInfo) ? "" : ": " + DeviceInfo)}"
            : "Not Initialized";

        public string DeviceUpdatePerformance => IsInitialized
            ? lastUpdateTime + " ms"
            : "";

        public bool IsInitialized => connectedDevices.Count != 0;

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry = new VariableRegistry();

            variableRegistry.Register($"{DeviceName}_update_interval", 0, "Update interval", null, 0);
            variableRegistry.Register($"{DeviceName}_enable_shutdown_color", false, "Enable shutdown color");
            variableRegistry.Register($"{DeviceName}_shutdown_color", new RealColor(Color.FromArgb(255, 255, 255, 255)), "Shutdown color");

            foreach (UnifiedBase device in allDevices)
                variableRegistry.Register($"UnifiedHID_{device.GetType().Name}_enable", false, 
                    $"Enable {(string.IsNullOrEmpty(device.PrettyName) ? device.GetType().Name : device.PrettyName)} in {DeviceName}");

        }

        public UnifiedHIDDevice()
        {
            // Copied GetLoadableTypes from https://haacked.com/archive/2012/07/23/get-all-types-in-an-assembly.aspx/
            IEnumerable<Type> GetLoadableTypes(Assembly assembly)
            {
                if (assembly == null)
                    throw new ArgumentNullException(nameof(assembly));

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
                Global.logger.Error("[UnifiedHID] class could not be constructed: " + exc);
            }
        }

        public override bool Initialize()
        {
            if (!IsInitialized)
            {
                // Clear list from old data
                connectedDevices.Clear();

                try
                {
                    foreach (UnifiedBase device in allDevices)
                    {
                        // Force disconnection and try a new connection
                        if (device.Disconnect() && device.Connect())
                        {
                            connectedDevices.Add(device);
                        }
                    }
                }
                catch (Exception e)
                {
                    Global.logger.Error($"[UnifiedHID] device could not be initialized: " + e);
                }
            }

            return IsInitialized;
        }

        public override void Shutdown()
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
                Global.logger.Error("[UnifiedHID] there was an error shutting down devices" + ex);
            }
        }

        public override void Reset()
        {
            Shutdown();
            Initialize();
        }

        protected override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            var sleep = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_update_interval");
            sleepWatch.Stop();
            lastSleepUpdateTime = sleepWatch.ElapsedMilliseconds;
            if (lastSleepUpdateTime > sleep)
            {
                sleepWatch.Restart();
            }
            else
            {
                // Resume stopWatch
                sleepWatch.Start();
                return false;
            }

            try
            {
                Dictionary<UnifiedBase, bool> results = new Dictionary<UnifiedBase, bool>(connectedDevices.Count);

                foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                {
                    foreach (UnifiedBase device in connectedDevices)
                    {
                        if (device.DeviceColorMap.TryGetValue(key.Key, out Color currentColor) && currentColor != key.Value)
                        {
                            // Apply and strip Alpha
                            var color = Color.FromArgb(255, ColorUtils.MultiplyColorByScalar(key.Value, key.Value.A / 255.0D));

                            // Update current color
                            device.DeviceColorMap[key.Key] = color;

                            // Set color
                            results[device] = device.SetColor(key.Key, color.R, color.G, color.B);
                        }
                    }
                }

                // Check results of connected devices
                foreach (var result in results)
                {
                    if (!result.Value)
                    {
                        Global.logger.Error($"[UnifiedHID] error when updating device {result.Key.PrettyName}. Restarting...");

                        // Try to restart device
                        if (result.Key.Disconnect() && !result.Key.Connect())
                        {
                            Global.logger.Error($"[UnifiedHID] unable to restart device {result.Key.PrettyName}. Removed from connected device!");
                            // Remove device from connected list
                            connectedDevices.Remove(result.Key);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Global.logger.Error("[UnifiedHID] error when updating device: " + ex);
                return false;
            }
        }
    }
}
