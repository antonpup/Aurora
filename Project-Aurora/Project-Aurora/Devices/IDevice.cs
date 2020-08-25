using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Settings;

namespace Aurora.Devices
{
    /// <summary>
    /// Struct representing color settings being sent to devices
    /// </summary>
    public class DeviceColorComposition
    {
        public readonly object bitmapLock = new object();
        public Dictionary<DeviceKeys, Color> keyColors;
        public Bitmap keyBitmap;
    }

    /// <summary>
    /// An interface for a device class.
    /// </summary>
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
        /// Gets the initialization status of this device instance.
        /// </summary>
        /// <returns>A boolean value representing the initialization status of this device</returns>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets registered variables by this device.
        /// </summary>
        /// <returns>Registered Variables</returns>
        VariableRegistry RegisteredVariables { get; }

        /// <summary>
        /// Attempts to initialize the device instance.
        /// </summary>
        /// <returns>A boolean value representing the success of this call</returns>
        bool Initialize();

        /// <summary>
        /// Shuts down the device instance.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Resets the device instance.
        /// </summary>
        void Reset();

        /// <summary>
        /// Updates the device with a specified color arrangement.
        /// </summary>
        /// <param name="keyColors">A dictionary of DeviceKeys their corresponding Colors</param>
        /// <param name="forced">A boolean value indicating whether or not to forcefully update this device</param>
        /// <returns></returns>
        bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false);

        /// <summary>
        /// Updates the device with a specified color composition.
        /// </summary>
        /// <param name="colorComposition">A struct containing a dictionary of colors as well as the resulting bitmap</param>
        /// <param name="forced">A boolean value indicating whether or not to forcefully update this device</param>
        /// <returns></returns>
        bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false);
    }
}
