using System.Collections.Generic;
using Aurora.Devices.Integration;
using Aurora.Plugins;
using Aurora.Settings;
using Aurora.Utils;

namespace Aurora.Devices
{
    public class DeviceManager : IInitialize, IPluginConsumer
    {
        public List<DeviceIntegration> Devices;

        public bool Initialized { get; private set; }
        
        public bool Initialize()
        {
            if (this.Initialized)
                return true;
            
            
            return (this.Initialized = true);
        }
        
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public void Visit(PluginBase plugin)
        {
            plugin.Process(this);
        }
    }
}