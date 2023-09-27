using System.Collections.Generic;
using System.Drawing;
using Aurora.Settings;
using Common;
using Common.Devices;
using JetBrains.Annotations;

namespace Aurora.Devices;

/// <summary>
/// An interface for a device class.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature | ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithInheritors)]
public interface IDevice
{
    /// <summary>
    /// Gets the name of the device.
    /// </summary>
    string DeviceName { get; }

    /// <summary>
    /// Gets specific details about the device instance.
    /// </summary>
    /// <returns>Details about the device instance</returns>
    string DeviceDetails { get; }

    /// <summary>
    /// Gets the device update performance.
    /// </summary>
    /// <returns>Details about device's update performance</returns>
    string DeviceUpdatePerformance { get; }

    /// <summary>
    /// Indicates that initialization is in progress for this device instance.
    /// </summary>
    bool isDoingWork { get; }

    /// <summary>
    /// Gets the initialization status of this device instance.
    /// </summary>
    /// <returns>A boolean value representing the initialization status of this device</returns>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets registered variables by this device.
    /// </summary>
    /// <returns>Registered Variables</returns>
    VariableRegistry RegisteredVariables { get; }

    IEnumerable<string> GetDevices();
    
    DeviceTooltips Tooltips { get; }
}