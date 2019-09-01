using Aurora.Devices.Layout;
using Aurora.Settings;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using LEDINT = System.Int16;

namespace Aurora.Devices
{

    /// <summary>
    /// Struct representing color settings being sent to devices
    /// </summary>
    public class DeviceColorComposition
    {
        public readonly object bitmapLock = new object();
        public Dictionary<LEDINT, System.Drawing.Color> deviceColours;
        public Bitmap keyBitmap;
    }

    public class DeviceSettings : SettingsBase
    {
        private bool isEnabled = true;
        public bool IsEnabled { get { return isEnabled; } set { UpdateVar(ref isEnabled, value); } }
    }

    public class FirstTimeDeviceSettings : DeviceSettings
    {
        private bool firstTime = false;
        public bool FirstTime { get { return firstTime; } set { UpdateVar(ref firstTime, value); } }
    }

    public abstract class Device<T> : ObjectSettings<T>, IDevice where T : DeviceSettings
    {
        private long lastUpdateTime = 0;
        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        public bool Initialized { get; protected set; } = false;
        public bool Enabled { get => this.Settings.IsEnabled; set => this.Settings.IsEnabled = value; }

        public Device() : base("Devices/")
        {
            this.LoadSettings();
        }
        public abstract string GetDeviceDetails();
        public abstract string GetDeviceName();
        public string GetDeviceUpdatePerformance()
        {
            return (Initialized ? lastUpdateTime + " ms" : "");
        }
        public abstract bool Initialize();
        public abstract bool IsConnected();
        public abstract bool IsKeyboardConnected();
        public abstract bool IsPeripheralConnected();
        public abstract bool Reconnect();
        public abstract void Reset();
        public abstract void Shutdown();
        public abstract bool PerformUpdateDevice(Color GlobalColor, List<DeviceLayout> devices, DoWorkEventArgs e, bool forced = false);
        public bool UpdateDevice(Color GlobalColor, List<DeviceLayout> devices, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();
            bool res = PerformUpdateDevice(GlobalColor, devices, e, forced);
            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;
            return res;
        }

    }

    /// <summary>
    /// An interface for a device class.
    /// </summary>
    public interface IDevice
    {
        bool Enabled { get; set; }


        /// <summary>
        /// Gets the device name.
        /// </summary>
        /// <returns>Device name</returns>
        string GetDeviceName();

        /// <summary>
        /// Gets specific details about the device instance.
        /// </summary>
        /// <returns>Details about the device instance</returns>
        string GetDeviceDetails();

        /// <summary>
        /// Gets the device update performance.
        /// </summary>
        /// <returns>Details about device's update performance</returns>
        string GetDeviceUpdatePerformance();

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
        /// Attempts to reconnect the device. [NOT IMPLEMENTED]
        /// </summary>
        /// <returns>A boolean value representing the success of this call</returns>
        bool Reconnect();

        /// <summary>
        /// Gets the initialization status of this device instance.
        /// </summary>
        /// <returns>A boolean value representing the initialization status of this device</returns>
        bool Initialized { get; }

        /// <summary>
        /// Gets the connection status of this device instance. [NOT IMPLEMENTED]
        /// </summary>
        /// <returns>A boolean value representing the connection status of this device</returns>
        bool IsConnected();

        /// <summary>
        /// Gets the keyboard connection status for this device instance.
        /// </summary>
        /// <returns>A boolean value representing the keyboard connection status of this device</returns>
        bool IsKeyboardConnected();

        /// <summary>
        /// Gets the peripheral connection status for this device instance.
        /// </summary>
        /// <returns>A boolean value representing the peripheral connection status of this device</returns>
        bool IsPeripheralConnected();

        /// <summary>
        /// Updates the device with a specified color composition.
        /// </summary>
        /// <param name="colorComposition">A struct containing a dictionary of colors as well as the resulting bitmap</param>
        /// <param name="forced">A boolean value indicating whether or not to forcefully update this device</param>
        /// <returns></returns>
        bool UpdateDevice(Color GlobalColor, List<DeviceLayout> devices, DoWorkEventArgs e, bool forced = false);
    }
}
