using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Devices
{
    public abstract class DefaultDevice : IDevice
    {
        private readonly Stopwatch watch = new Stopwatch();
        private long lastUpdateTime;

        public abstract string DeviceName { get; }

        protected virtual string DeviceInfo => "";

        public string DeviceDetails => IsInitialized
            ? $"Initialized{(string.IsNullOrWhiteSpace(DeviceInfo) ? "" : ": " + DeviceInfo)}"
            : "Not Initialized";

        public string DeviceUpdatePerformance => IsInitialized
            ? lastUpdateTime + " ms"
            : "";

        public virtual bool IsInitialized { get; protected set; }

        public abstract bool Initialize();
        public abstract void Shutdown();

        public virtual void Reset()
        {
            Shutdown();
            Initialize();
        }

        public abstract bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false);

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            bool update_result = UpdateDevice(colorComposition.keyColors, e, forced);

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return update_result;
        }

        #region Variables
        private VariableRegistry variableRegistry;
        public VariableRegistry RegisteredVariables
        {
            get
            {
                if (variableRegistry is null)
                {
                    variableRegistry = new VariableRegistry();
                    RegisterVariables(variableRegistry);
                }
                return variableRegistry;
            }
        }
        protected virtual void RegisterVariables(VariableRegistry variableRegistry) { }

        protected void LogInfo(string s) => Global.logger.Info($"[Device][{DeviceName}] {s}");

        protected void LogError(string s) => Global.logger.Error($"[Device][{DeviceName}] {s}");
        #endregion
    }
}
