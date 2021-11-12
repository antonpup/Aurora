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

    class XPGDevice : DefaultDevice
    {
        public override string DeviceName => "XPG";

        private Stopwatch updateStopwatch = new Stopwatch();


        [DllImport("libxpgp_aurora", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Initialize")]
        private static extern int cInitialize();
        [DllImport("libxpgp_aurora", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Reset")]
        private static extern void cReset();
        [DllImport("libxpgp_aurora", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Shutdown")]
        private static extern void cShutdown();
        [DllImport("libxpgp_aurora", CallingConvention = CallingConvention.Cdecl, EntryPoint = "UpdateDevice")]
        private static extern int cUpdateDevice(byte[] array, int bytesize);

        private bool crashed = false;
        private bool initialized = false;
        public override bool IsInitialized => initialized && !crashed;

        protected override string DeviceInfo => crashed ? "Error" : "OK";

        public override bool Initialize()
        {
            if (crashed)
            {
                return false;
            }
            if (!initialized)
            {
                int error = cInitialize();
                if (error > 0) // a proper error occured
                {
                    crashed = true; // unlikely to work on the second try either
                }
                if (error == 0) // no error + device found
                {
                    initialized = true;
                }
            }
            return IsInitialized;
        }

        public override void Reset()
        {
            updateStopwatch.Reset();
            if (IsInitialized)
            {
                cReset();
            }
        }

        public override void Shutdown()
        {
            Reset();
            cShutdown();
            initialized = false;
        }

        public override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (IsInitialized)
            {
                bool success = TryUpdateDevice(keyColors, forced);
                if (!success)
                {
                    this.Shutdown();
                }
                return success;
            }
            return false;
        }

        private const int colorcount = 108; // highest value we use is FN_Key (107)
        private const int bytesize = 4 * colorcount;
        private byte[] currentColors = new byte[bytesize];
        private byte[] spareBuffer = new byte[bytesize];
        private bool TryUpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced)
        {
            byte[] newColors = this.spareBuffer;
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
            else
            {
                // start with current colors to support partial updates
                Array.Copy(this.currentColors, newColors, bytesize);
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
                this.spareBuffer = this.currentColors;
                this.currentColors = newColors;
            }
            return true;
        }
    }

}
