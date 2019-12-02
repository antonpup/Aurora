using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Aurora.Settings;
using Aurora.Utils;

namespace Aurora.Devices.SteelSeries
{
    public partial class SteelSeriesDevice : Device
    {
        Stopwatch watch = new Stopwatch();
        object lock_obj = new object();

        public VariableRegistry GetRegisteredVariables()
        {
            return new VariableRegistry();
        }

        public string GetDeviceName() => "SteelSeries";

        public string GetDeviceDetails() => IsInitialized() ? GetDeviceName() + ": Connected" : GetDeviceName() + ": Not initialized";

        public string GetDeviceUpdatePerformance()
        {
            return (IsInitialized() ? watch.ElapsedMilliseconds + " ms" : "");
        }

        public bool Initialize()
        {
            lock (lock_obj)
            {
                try
                {
                    if (Global.Configuration.steelseries_first_time)
                    {
                        System.Windows.Application.Current.Dispatcher?.Invoke(() =>
                        {
                            SteelSeriesInstallInstructions instructions = new SteelSeriesInstallInstructions();
                            instructions.ShowDialog();
                        });
                        Global.Configuration.steelseries_first_time = false;
                        ConfigManager.Save(Global.Configuration);
                    }
                    loadCoreProps();
                    baseObject.Add("game", "PROJECTAURORA");
                    baseColorObject.Add("game", baseObject["game"]);
                    return true;
                }
                catch (Exception e)
                {
                    Global.logger.Error("SteelSeries SDK could not be initialized: " + e);
                    return false;
                }
            }
        }

        public void Shutdown()
        {
            lock (lock_obj)
            {
                pingTaskTokenSource.Cancel();
                loadedLisp = false;
            }
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public bool IsInitialized() => loadedLisp;

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool IsKeyboardConnected()
        {
            throw new NotImplementedException();
        }

        public bool IsPeripheralConnected()
        {
            throw new NotImplementedException();
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            foreach (var (key, color) in keyColors)
            {
                if (TryGetHid(key, out var hid))
                {
                    setKeyboardLed(hid, color);
                }
            }
            return true;
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();
            var keyColors = colorComposition.keyColors.ToDictionary(t => t.Key, t => ColorUtils.MultiplyColorByScalar(t.Value, t.Value.A / 255D));
            var mousePad = keyColors.Where(t => t.Key >= DeviceKeys.MOUSEPADLIGHT1 && t.Key <= DeviceKeys.MOUSEPADLIGHT12).Select(t => t.Value).ToArray();
            var mouse = keyColors.Where(t => t.Key == DeviceKeys.Peripheral_Logo || t.Key == DeviceKeys.Peripheral_ScrollWheel || t.Key >= DeviceKeys.ADDITIONALLIGHT11 && t.Key <= DeviceKeys.ADDITIONALLIGHT16).Select(t => t.Value).ToArray();
            var monitor = keyColors.Where(t => t.Key >= DeviceKeys.MONITORLIGHT1 && t.Key <= DeviceKeys.MONITORLIGHT103).Select(t => t.Value).ToArray();
            if (!Global.Configuration.devices_disable_mouse || !Global.Configuration.devices_disable_headset)
            {
                if (mouse.Length <= 1)
                    setMouse(keyColors[DeviceKeys.Peripheral_Logo]);
                else
                {
                    setLogo(keyColors[DeviceKeys.Peripheral_Logo]);
                    setWheel(keyColors[DeviceKeys.Peripheral_ScrollWheel]);
                    if (mouse.Length == 8)
                        setEightZone(mouse);
                }
                if (mousePad.Length == 2)
                    setTwoZone(mousePad);
                else
                    setTwelveZone(mousePad);
                if (monitor.Length == 103)
                    setHundredThreeZone(monitor);
            }
            if (!Global.Configuration.devices_disable_keyboard)
            {
                UpdateDevice(keyColors, e, forced);
            }
            sendLighting();
            watch.Stop();
            return true;
        }
    }
}