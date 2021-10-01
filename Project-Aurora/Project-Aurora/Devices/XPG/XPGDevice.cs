using Aurora;
using Aurora.Devices;
using Aurora.Devices.ScriptedDevice;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using System.Runtime.InteropServices; // DllImport
using System.Diagnostics; // Stopwatch
using System.ComponentModel; // DoWorkEventArgs

namespace Aurora.Devices.XPG
{

    class XPGDevice : IDevice
    {
        // XPG device is implemented as a script for ScriptedDevice, but we
        // also want the script to call Shutdown() when UpdateDevice() fails.
        // This lets us easily handle the peripheral being disconnected and
        // reconnected. However, we can't override the interface method
        // from ScriptedDevice, use aggregation instead.
        private Devices.ScriptedDevice.ScriptedDevice device;

        public XPGDevice()
        {
            device = new Devices.ScriptedDevice.ScriptedDevice(new XPGDeviceScript());
        }

        public Settings.VariableRegistry RegisteredVariables => device.RegisteredVariables;
        public string DeviceName => "XPG";
        public string DeviceDetails => device.DeviceDetails;
        public string DeviceUpdatePerformance => device.DeviceUpdatePerformance;
        public bool Initialize() => device.Initialize();
        public void Shutdown() => device.Shutdown();
        public void Reset() => device.Reset();
        public bool Reconnect() => device.Reconnect();
        public bool IsInitialized => device.IsInitialized;
        public bool IsConnected() => device.IsConnected();
        public bool IsKeyboardConnected() => device.IsKeyboardConnected();
        public bool IsPeripheralConnected() => device.IsPeripheralConnected();

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (IsInitialized)
            {
                bool success = device.UpdateDevice(keyColors, e, forced);
                if (!success) {
                    device.Shutdown();
                }
                return success;
            }
            return false;
        }
        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            if (IsInitialized)
            {
                bool success = device.UpdateDevice(colorComposition, e, forced);
                if (!success) {
                    device.Shutdown();
                }
                return success;
            }
            return false;
        }

    }


    internal class XPGDeviceScript
    {
        public string devicename = "XPG script";
        public bool enabled = true;

        private Stopwatch updateStopwatch = new Stopwatch();


        [DllImport("libxpgp_aurora", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Initialize")]
        private static extern int cInitialize();
        [DllImport("libxpgp_aurora", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Reset")]
        private static extern void cReset();
        [DllImport("libxpgp_aurora", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Shutdown")]
        private static extern void cShutdown();
        [DllImport("libxpgp_aurora", CallingConvention = CallingConvention.Cdecl, EntryPoint = "UpdateDevice")]
        private static extern int cUpdateDevice(byte[] array, int bytesize);


        public bool Initialize()
        {
            int error = cInitialize();
            if (error > 0) // a proper error occured
            {
                throw new Exception("Error initializing XPGDeviceScript");
            }
            return error == 0; // true if we have a device
        }

        public void Reset()
        {
            updateStopwatch.Reset();
            cReset();
        }

        public void Shutdown()
        {
            updateStopwatch.Reset();
            cShutdown();
        }

        private const int colorcount = 108; // highest value we use is FN_Key (107)
        private const int bytesize = 4 * colorcount;
        private byte[] currentColors = new byte[bytesize];
        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced)
        {
            byte[] newColors = new byte[bytesize]; // zero-init
            if (keyColors.ContainsKey(DeviceKeys.Peripheral))
            {
                Color color = keyColors[DeviceKeys.Peripheral];
                for (int k = 0; k < colorcount; k++)
                {
                    newColors[4 * k + 0] = color.R;
                    newColors[4 * k + 1] = color.G;
                    newColors[4 * k + 2] = color.B;
                    newColors[4 * k + 3] = color.A;
                }
            }
            foreach (KeyValuePair<DeviceKeys, Color> kvp in keyColors)
            {
                int k = (int)kvp.Key;
                if (0 <= k && k < colorcount)
                {
                    Color color = kvp.Value;
                    newColors[4 * k + 0] = color.R;
                    newColors[4 * k + 1] = color.G;
                    newColors[4 * k + 2] = color.B;
                    newColors[4 * k + 3] = color.A;
                }
            }
            // Stopwatch is stopped. This is the first update after connecting.
            // 'this.currentColors' doesn't hold any previous state.
            if (!updateStopwatch.IsRunning)
            {
                forced = true;
            }
            // The device resets itself after 5s. Force an update before then.
            if (updateStopwatch.ElapsedMilliseconds >= 4703)
            {
                forced = true;
            }
            // Do the thing!
            if (forced || !Enumerable.SequenceEqual(this.currentColors, newColors))
            {
                int error = cUpdateDevice(newColors, bytesize);
                if (error != 0) // device has probably been physically disconnected
                {
                    return false;
                }
                updateStopwatch.Restart();
                this.currentColors = newColors;
            }
            return true;
        }
    }

}
