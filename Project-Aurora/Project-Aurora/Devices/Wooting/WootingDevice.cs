using Aurora.Settings;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

        protected override Task<bool> DoInitialize()
        {
            if (IsInitialized)
                return Task.FromResult(true);

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
                LogError("There was an error initializing Wooting SDK", exc);

                IsInitialized = false;
            }

            return Task.FromResult(IsInitialized);
        }

        public override Task Shutdown()
        {
            if (!IsInitialized)
                return Task.CompletedTask;

            RGBControl.Close();
            _deviceInfo = "";
            IsInitialized = false;
            return Task.CompletedTask;
        }

        protected override Task<bool> UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!IsInitialized)
                return Task.FromResult(false);

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
                return Task.FromResult(true);
            }
            catch (Exception exc)
            {
                LogError("Failed to Update Device", exc);
                return Task.FromResult(false);
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
