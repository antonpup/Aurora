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
            if (Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_override_dll"))
                LogitechGSDK.GHUB = Global.Configuration.VarRegistry.GetVariable<LGDLL>($"{DeviceName}_override_dll_option") == LGDLL.GHUB;
            else
                LogitechGSDK.GHUB = Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "LGHUB"));

            Global.logger.Info("Trying to initialize Logitech using the dll for " + (LogitechGSDK.GHUB ? "GHUB" : "LGS"));

            if (LogitechGSDK.LogiLedInit() && LogitechGSDK.LogiLedSaveCurrentLighting())
            {
                LogitechGSDK.LogiLedSetLighting(Global.Configuration.VarRegistry.GetVariable<RealColor>($"{DeviceName}_default_color").GetDrawingColor());
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
                if (LedMaps.KeyMap.TryGetValue(key.Key, out var logiKey))
                    IsInitialized &= LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(logiKey, key.Value);
                if (LedMaps.HidCodeMap.TryGetValue(key.Key, out var hidId))
                    IsInitialized &= LogitechGSDK.LogiLedSetLightingForKeyWithHidCode(hidId, key.Value);
                if (LedMaps.PeripheralMap.TryGetValue(key.Key, out var peripheral))
                    IsInitialized &= LogitechGSDK.LogiLedSetLightingForTargetZone(peripheral.type, peripheral.zone, key.Value);
            }

            IsInitialized &= LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB);
            IsInitialized &= LogitechGSDK.LogiLedSetLightingFromBitmap(logitechBitmap);

            return IsInitialized;
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_set_default", false, "Set Default Color");
            variableRegistry.Register($"{DeviceName}_default_color", new Utils.RealColor(Color.FromArgb(255, 255, 255, 255)), "Default Color");
            variableRegistry.Register($"{DeviceName}_override_dll", false, "Override DLL", null, null, "Requires restart to take effect");
            variableRegistry.Register($"{DeviceName}_override_dll_option", LGDLL.GHUB, "Override DLL Selection", null, null, "Requires restart to take effect");
        }
    }
}
