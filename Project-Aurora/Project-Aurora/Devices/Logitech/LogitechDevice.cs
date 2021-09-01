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
        private Color speakers;
        private Color mousepad;
        private readonly Color[] mouse = new Color[2];
        private readonly Color[] headset = new Color[2];
        private DeviceKeys genericKey;

        public override bool Initialize()
        {
            genericKey = Global.Configuration.VarRegistry.GetVariable<DeviceKeys>($"{DeviceName}_devicekey");
            var ghubRunning = Global.LightingStateManager.RunningProcessMonitor.IsProcessRunning("lghub.exe");
            var lgsRunning = Global.LightingStateManager.RunningProcessMonitor.IsProcessRunning("lcore.exe");

            if (!ghubRunning && !lgsRunning)
                return IsInitialized = false;

            if (Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_override_dll"))
                LogitechGSDK.GHUB = Global.Configuration.VarRegistry.GetVariable<LGDLL>($"{DeviceName}_override_dll_option") == LGDLL.GHUB;
            else
                LogitechGSDK.GHUB = ghubRunning;

            LogInfo($"Trying to initialize Logitech using the dll for {(LogitechGSDK.GHUB ? "GHUB" : "LGS")}");

            if (LogitechGSDK.LogiLedInit() && LogitechGSDK.LogiLedSaveCurrentLighting())
            {
                //logitech says to wait a bit of time between Init() and SetLighting()
                //This didnt seem to be needed in the past but I feel like 100ms might 
                //fix some weird issues without any noticeable disadvantages
                Thread.Sleep(100);
                if (Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_set_default"))
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

            //reset keys to peripheral_logo here so if we dont find any better color for them,
            //at least the leds wont turn off :)
            if (keyColors.TryGetValue(genericKey, out var periph))
            {
                speakers = periph;
                mousepad = periph;
                mouse[0] = periph;
                mouse[1] = periph;
                headset[0] = periph;
                headset[1] = periph;
            }

            foreach (var key in keyColors)
            {
                #region keyboard
                if (LedMaps.BitmapMap.TryGetValue(key.Key, out var index))
                {
                    logitechBitmap[index] = key.Value.B;
                    logitechBitmap[index + 1] = key.Value.G;
                    logitechBitmap[index + 2] = key.Value.R;
                    logitechBitmap[index + 3] = key.Value.A;
                }

                if (!Global.Configuration.DevicesDisableKeyboard && LedMaps.KeyMap.TryGetValue(key.Key, out var logiKey))
                    IsInitialized &= LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(logiKey, key.Value);
                #endregion

                #region peripherals
                if (LedMaps.PeripheralMap.TryGetValue(key.Key, out var peripheral))
                {
                    switch (peripheral.type)
                    {
                        case DeviceType.Mouse:
                            mouse[peripheral.zone] = key.Value;
                            break;
                        case DeviceType.Mousemat:
                            mousepad = key.Value;
                            break;
                        case DeviceType.Headset:
                            headset[peripheral.zone] = key.Value;
                            break;
                        case DeviceType.Speaker:
                            speakers = key.Value;
                            break;
                    }
                }
                #endregion
            }

            if (!Global.Configuration.DevicesDisableMouse)
            {
                for (int i = 0; i < 2; i++)
                {
                    LogitechGSDK.LogiLedSetLightingForTargetZone(DeviceType.Mouse, i, mouse[i]);
                }

                LogitechGSDK.LogiLedSetLightingForTargetZone(DeviceType.Mousemat, 0, mousepad);
            }
            if (!Global.Configuration.DevicesDisableHeadset)
            {
                for (int i = 0; i < headset.Length; i++)
                {
                    LogitechGSDK.LogiLedSetLightingForTargetZone(DeviceType.Headset, i, headset[i]);
                }

                for (int i = 0; i < 4; i++)//speakers have 4 leds
                {
                    LogitechGSDK.LogiLedSetLightingForTargetZone(DeviceType.Speaker, i, speakers);
                }
            }

            if (!Global.Configuration.DevicesDisableKeyboard)
            {
                IsInitialized &= LogitechGSDK.LogiLedSetLightingFromBitmap(logitechBitmap);
            }

            return IsInitialized;
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_set_default", false, "Set Default Color");
            variableRegistry.Register($"{DeviceName}_default_color", new Utils.RealColor(Color.FromArgb(255, 255, 255, 255)), "Default Color");
            variableRegistry.Register($"{DeviceName}_override_dll", false, "Override DLL", null, null, "Requires restart to take effect");
            variableRegistry.Register($"{DeviceName}_override_dll_option", LGDLL.GHUB, "Override DLL Selection", null, null, "Requires restart to take effect");
            variableRegistry.Register($"{DeviceName}_devicekey", DeviceKeys.Peripheral_Logo, "Key to Use", DeviceKeys.MOUSEPADLIGHT15, DeviceKeys.Peripheral);
        }
    }
}
