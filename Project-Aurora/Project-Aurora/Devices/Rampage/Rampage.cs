using System;
using System.Collections.Generic;
using System.ComponentModel;
using Aurora.Utils;
using System.Drawing;
using System.Threading;
using Aurora.Settings;

namespace Aurora.Devices.Rampage
{
    public class Rampage : DefaultDevice
    {
        public override string DeviceName => "Rampage";
        protected override string DeviceInfo => IsInitialized ? GetDeviceNames() : base.DeviceInfo;

        private RampageMouse mouse;

        public override bool Initialize()
        {
            mouse = RampageMouse.Initialize();

            return IsInitialized = mouse != null;
        }

        public override void Shutdown()
        {
            mouse?.Disconnect();
            mouse = null;
            IsInitialized = false;
        }

        protected override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false) {
            if (mouse!=null)
            {
                UpdateMouse(keyColors);

                var sleep = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_sleep");
                if (sleep > 0)
                    Thread.Sleep(sleep);
            }
            return true;
        }
        
        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_sleep", 30, "Sleep for", 1000, 0);
        }

        private void UpdateMouse(Dictionary<DeviceKeys, Color> keyColors)
        {
            keyColors.TryGetValue(DeviceKeys.Peripheral, out var peripheralColor);
            mouse.SetColor(ColorUtils.CorrectWithAlpha(peripheralColor));
            foreach (var keyValuePair in RampageKeyMap.MouseLightMap)
            {
                if (keyColors.TryGetValue(keyValuePair.Value, out var color))
                {
                    var fromArgb = Color.FromArgb(
                        Math.Max(color.A - 128, 0),
                        Math.Max(color.R - 128, 0),
                        Math.Max(color.G - 128, 0),
                        Math.Max(color.B - 128, 0));
                    mouse.SetKeyColor(keyValuePair.Key, ColorUtils.CorrectWithAlpha(fromArgb));
                }
            }

            mouse.Update();
        }

        private string GetDeviceNames()
        {
            return (mouse != null ? " Mouse" : "");
        }
    }
}