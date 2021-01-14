using Aurora.Settings;
using Aurora.Utils;
using LedCSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Devices.Logitech
{
    public class LogitechDevice : DefaultDevice
    {
        public override string DeviceName => "Logitech";

        private readonly byte[] logitechBitmap = new byte[LogitechGSDK.LOGI_LED_BITMAP_SIZE];

        public override bool Initialize()
        {
            var ghubRunning = Global.LightingStateManager.RunningProcessMonitor.IsProcessRunning("lghub.exe");
            var lgsRunning = Global.LightingStateManager.RunningProcessMonitor.IsProcessRunning("lcore.exe");

            if (!ghubRunning && !lgsRunning)
                return IsInitialized = false;

            LogInfo($"Trying to initialize Logitech SDK");

            if (LogitechGSDK.LogiLedInit() && LogitechGSDK.LogiLedSaveCurrentLighting())
            {
                if (Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_set_default"))
                {
                    var color = Global.Configuration.VarRegistry.GetVariable<RealColor>($"{DeviceName}_default_color").GetDrawingColor();
                    LogitechGSDK.LogiLedSetLighting(color.R, color.G, color.B);
                }

                return IsInitialized = true;
            }

            return IsInitialized = false;
        }

        public override void Shutdown()
        {
            LogitechGSDK.LogiLedRestoreLighting();
            LogitechGSDK.LogiLedShutdown();
            IsInitialized = false;
        }

        public override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!IsInitialized)
                return false;

            foreach (var key in keyColors)
            {
                if (LedMaps.BitmapMap.TryGetValue(key.Key, out var index))
                {
                    logitechBitmap[index] = key.Value.B;
                    logitechBitmap[index + 1] = key.Value.G;
                    logitechBitmap[index + 2] = key.Value.R;
                    logitechBitmap[index + 3] = key.Value.A;
                }
                if (!Global.Configuration.DevicesDisableKeyboard && LedMaps.KeyMap.TryGetValue(key.Key, out var logiKey))
                    IsInitialized &= LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(logiKey, key.Value.R, key.Value.G, key.Value.B);
                if (LedMaps.PeripheralMap.TryGetValue(key.Key, out var peripheral))
                {
                    if ((peripheral.type == DeviceType.Headset && !Global.Configuration.DevicesDisableHeadset)
                    || (peripheral.type == DeviceType.Mouse && !Global.Configuration.DevicesDisableMouse))
                    {
                        LogitechGSDK.LogiLedSetLightingForTargetZone(peripheral.type, peripheral.zone, key.Value.R, key.Value.G, key.Value.B);
                    }
                }

                //TargetZone returns false if the targer device does not have the zone with the specified index
                //so we'll not use it to check the connection is still active.
                //The other methods only seem to return false if the connection to LGS / GHUB fails
            }

            if (!Global.Configuration.DevicesDisableKeyboard)
                IsInitialized &= LogitechGSDK.LogiLedSetLightingFromBitmap(logitechBitmap);

            return IsInitialized;
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_set_default", false, "Set Default Color");
            variableRegistry.Register($"{DeviceName}_default_color", new Utils.RealColor(Color.FromArgb(255, 255, 255, 255)), "Default Color");
        }
    }
}
