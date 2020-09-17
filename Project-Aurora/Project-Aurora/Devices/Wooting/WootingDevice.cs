using Aurora.Settings;
using Aurora.Utils;
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
        public override string DeviceName => "Wooting";
        protected override string DeviceInfo => _deviceInfo;
        private string _deviceInfo = "";
        private DisconnectedCallback cb;
        public override bool Initialize()
        {
            if (IsInitialized)
                return IsInitialized;

            try
            {
                if (RGBControl.IsConnected())
                {
                    IsInitialized = true;
                    _deviceInfo = RGBControl.GetDeviceInfo().Model;
                    cb = new DisconnectedCallback(OnDisconnect);
                    RGBControl.SetDisconnectedCallback(cb);
                }
                else
                {
                    IsInitialized = false;
                    _deviceInfo = "";
                }
            }
            catch (Exception exc)
            {
                LogError("There was an error initializing Wooting SDK.\r\n" + exc.Message);

                IsInitialized = false;
            }

            return IsInitialized;
        }

        public override void Shutdown()
        {
            if (!IsInitialized)
                return;

            RGBControl.Close();
            _deviceInfo = "";
            IsInitialized = false;
        }

        public override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!IsInitialized)
                return false;

            double rScalar = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_scalar_r") / 100.0;
            double gScalar = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_scalar_g") / 100.0;
            double bScalar = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_scalar_b") / 100.0;

            try
            {
                foreach (var key in keyColors)
                {
                    if (WootingKeyMap.KeyMap.TryGetValue(key.Key, out var wootKey))
                    {
                        var clr = ColorUtils.CorrectWithAlpha(key.Value);
                        RGBControl.SetKey(wootKey, (byte)(clr.R * rScalar),
                                                   (byte)(clr.G * gScalar),
                                                   (byte)(clr.B * bScalar));
                    }
                }
                RGBControl.UpdateKeyboard();
                return true;
            }
            catch (Exception exc)
            {
                LogError("Failed to Update Device" + exc.ToString());
                return false;
            }
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_scalar_r", 100, "Red Scalar", 100, 0);
            variableRegistry.Register($"{DeviceName}_scalar_g", 100, "Green Scalar", 100, 0);
            variableRegistry.Register($"{DeviceName}_scalar_b", 100, "Blue Scalar", 100, 0, "In percent");
        }

        private void OnDisconnect()
        {
            IsInitialized = false;
        }
    }
}
