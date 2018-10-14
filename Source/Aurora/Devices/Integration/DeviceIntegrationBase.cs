using Aurora.Settings;
using Aurora.Utils;

namespace Aurora.Devices.Integration
{
    public interface DeviceIntegration : IInitialize
    {
        string IntegrationID { get; }
        
        bool IsEnabled { get; }
        
        /// <summary>
        /// Gets the device name.
        /// </summary>
        /// <returns>Device name</returns>
        string DeviceName { get; }

        /// <summary>
        /// Gets specific details about the device instance.
        /// </summary>
        /// <returns>Details about the device instance</returns>
        string DeviceDetails { get; }

        bool IsAvailable { get; }
        
        /// <summary>
        /// Shuts down the device instance.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Resets the device instance.
        /// </summary>
        void Reset();
        
        
        //render placeholder
        void Render();
    }
    
    public abstract class DeviceIntegrationBase<T> : ObjectSettings<T> where T : DeviceIntegrationSettings, DeviceIntegration
    {
        bool IsEnabled => this.Settings.Enabled;
    }
}