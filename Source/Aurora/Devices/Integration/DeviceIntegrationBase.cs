using System.Collections.Generic;
using Aurora.Settings;
using Aurora.Utils;

namespace Aurora.Devices.Integration
{
    public enum IntegrationState
    {
        Initialised,
        Paused,
        Inactive
    }
    
    public interface DeviceIntegration : IInitialize
    {
        //TODO: Might want to change this up to be cleaner with DeviceManager. Could potentially have it bind to event when Enabled is changed and use that to auto disable
        /// <summary>
        /// Gets a bool determining if the Integration is Enabled
        /// </summary>
        bool IsEnabled { get; }
        
        /// <summary>
        /// The unique IntegrationID
        /// </summary>
        short IntegrationID { get; }
        
        /// <summary>
        /// [OPTIONAL] The name of this integration
        /// </summary>
        string IntegrationName { get; }
        
        /// <summary>
        /// The unique BrandID for the Brand of devices this Integration is supporting
        /// Can be used to only allow one integration for a particular brand to be used at once
        /// </summary>
        short BrandID { get; }
        
        /// <summary>
        /// Name of the Brand of devices this integration supports
        /// </summary>
        string BrandName { get; }
        
        /// <summary>
        /// Array of Device types that this integration can process
        /// </summary>
        byte[] SupportedDevices { get; }

        /// <summary>
        /// Gets specific details about the device instance.
        /// </summary>
        /// <returns>Details about the device instance</returns>
        string DeviceDetails { get; }

        /// <summary>
        /// Gets a bool determining if the relevant items are installed for it to be able to be Initialised
        /// i.e. LGS installed
        /// </summary>
        bool IsInstalled { get; }
        
        /// <summary>
        /// Get a bool determining if necessary services are running
        /// i.e. LGS process running
        /// </summary>
        bool IsAvailable { get; }
        
        /// <summary>
        /// Attempts to initialise the integration to begin processing lighting
        /// </summary>
        /// <returns>A bool determining if the integration was successfully initialised</returns>
        bool Initialise();
        
        /// <summary>
        /// Gets the current state of the Integration
        /// </summary>
        IntegrationState CurrentState { get; }

        /// <summary>
        /// Instructs the Integration to attempt a temporary sleep of the integration,
        /// returning lighting control to the original owner
        /// </summary>
        /// <returns>A bool determining if the operation was successful</returns>
        bool Sleep();
        
        /// <summary>
        /// Attempts to wake up the integration from a sleep operation to control lighting of the devices again
        /// </summary>
        /// <returns>A bool determining if the operation was successful</returns>
        bool Wake();
        
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