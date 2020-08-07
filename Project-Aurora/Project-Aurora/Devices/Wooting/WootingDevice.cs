using Aurora.Settings;
using Aurora.Utils;
using CoolerMaster;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wooting;

namespace Aurora.Devices.Wooting
{
    public class WootingDevice : DefaultDevice
    {
        private readonly object action_lock = new object();

        public override string DeviceName => "Wooting";

        public override bool Initialize()
        {
            if (IsInitialized)
                return IsInitialized;

            lock (action_lock)
            {
                try
                {
                    IsInitialized = RGBControl.IsConnected();
                }
                catch (Exception exc)
                {
                    Global.logger.Error("There was an error initializing Wooting SDK.\r\n" + exc.Message);

                    IsInitialized = false;
                }

                return IsInitialized;
            }
        }

        public override void Shutdown()
        {
            if (!IsInitialized)
                return;

            lock (action_lock)
            {
                RGBControl.Reset();
                IsInitialized = false;   
            }
        }

        public override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!IsInitialized)
                return false;

            try
            {
                //Do this to prevent setting lighting again after the keyboard has been shutdown and reset
                lock (action_lock)
                {
                    foreach (var key in keyColors)
                    {
                        if(WootingKeyMap.KeyMap.TryGetValue(key.Key, out var wootKey))
                        {
                            var clr = ColorUtils.CorrectWithAlpha(key.Value);
                            RGBControl.SetKey(wootKey, (byte)(clr.R * Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_scalar_r") / 100),
                                                       (byte)(clr.G * Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_scalar_g") / 100),
                                                       (byte)(clr.B * Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_scalar_b") / 100));
                        }
                    }
                    RGBControl.UpdateKeyboard();
                }
                return true;
            }
            catch (Exception exc)
            {
                Global.logger.Error("Failed to Update Device" + exc.ToString());
                return false;
            }
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_scalar_r", 100, "Red Scalar", 100, 0);
            variableRegistry.Register($"{DeviceName}_scalar_g", 100, "Green Scalar", 100, 0);
            variableRegistry.Register($"{DeviceName}_scalar_b", 100, "Blue Scalar", 100, 0, "In percent");
        }
    }
}
