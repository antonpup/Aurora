using Aurora.Settings;
using Aurora.Utils;
using DS4Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Aurora.Devices.Dualshock
{
    internal class DS4Container
    {
        public readonly DS4Device device;
        public readonly ConnectionType connectionType;
        public readonly Color RestoreColor;

        public int Battery { get; private set; }
        public double Latency { get; private set; }
        public bool Charging { get; private set; }

        public Color sendColor;
        public DS4HapticState state;

        public DS4Container(DS4Device _device, Color restoreColor)
        {
            device = _device;
            connectionType = device.getConnectionType();
            device.Report += OnDeviceReport;
            device.StartUpdate();
            RestoreColor = restoreColor;
        }

        private void OnDeviceReport(object sender, EventArgs e)
        {
            Battery = device.Battery;
            Latency = device.Latency;
            Charging = device.Charging;

            if (ColorsEqual(sendColor, state.LightBarColor))
                return;

            state.LightBarExplicitlyOff = sendColor.R == 0 && sendColor.G == 0 && sendColor.B == 0;
            state.LightBarColor = new DS4Color(sendColor);
            device.pushHapticState(state);
        }

        public void Disconnect(bool stop)
        {
            sendColor = RestoreColor;
            Thread.Sleep(10);//HACK: this is terrible, but it works reliably so i'll leave it like this :(
            //this is needed so that OnDeviceReport has time to send the color once the device sends a report.
            device.Report -= OnDeviceReport;
            if (stop)
            {
                device.DisconnectBT();
                device.DisconnectDongle();
            }
            device.StopUpdate();
        }

        public string GetDeviceDetails()
        {
            var connectionString = connectionType switch
            {
                ConnectionType.BT => "Bluetooth",
                ConnectionType.SONYWA => "DS4 Wireless adapter",
                ConnectionType.USB => "USB",
                _ => "",
            };

            return $"over {connectionString} {(Charging ? "⚡" : "")}🔋{Battery}% Latency: {Latency:0.00}ms";
        }

        private bool ColorsEqual(Color clr, DS4Color ds4clr)
        {
            return clr.R == ds4clr.red &&
                    clr.G == ds4clr.green &&
                    clr.B == ds4clr.blue;
        }
    }

    public class DualshockDevice : DefaultDevice
    {
        public override string DeviceName => "Sony DualShock 4(PS4)";

        protected override string DeviceInfo =>
            string.Join(", ", devices.Select((dev, i) => $"#{i + 1} {dev.GetDeviceDetails()}"));

        public int Battery => devices.FirstOrDefault()?.Battery ?? -1;
        public double Latency => devices.FirstOrDefault()?.Latency ?? -1;
        public bool Charging => devices.FirstOrDefault()?.Charging ?? false;

        private readonly List<DS4Container> devices = new List<DS4Container>();
        private DeviceKeys key;
        private bool isDisconnecting;

        public DualshockDevice()
        {
            //dummy call, we just need hidsharp to scan for devices once
            HidSharp.DeviceList.Local.GetAllDevices();
            HidSharp.DeviceList.Local.Changed += DeviceListChanged;
        }

        private void DeviceListChanged(object sender, HidSharp.DeviceListChangedEventArgs e)
        {
            if ((Global.Configuration?.DevicesDisabled?.Contains(typeof(DualshockDevice)) ?? false) || 
               (!Global.Configuration?.VarRegistry?.GetVariable<bool>($"{DeviceName}_auto_init") ?? false))
            {
                return;
            }
                

            if (isDisconnecting)
                return;

            LogInfo("Detected device list changed, rescanning for controllers...");
            DS4Devices.findControllers();
            if (DS4Devices.getDS4Controllers().Count() != devices.Count)
                Reset();
        }

        public override bool Initialize()
        {
            if (IsInitialized)
                return true;

            key = Global.Configuration.VarRegistry.GetVariable<DeviceKeys>($"{DeviceName}_devicekey");
            DS4Devices.findControllers();

            var restore = Global.Configuration.VarRegistry.GetVariable<RealColor>($"{DeviceName}_restore_dualshock").GetDrawingColor();

            foreach (var controller in DS4Devices.getDS4Controllers())
                devices.Add(new DS4Container(controller, restore));

            return IsInitialized = devices.Count > 0;
        }

        public override void Shutdown()
        {
            if (!IsInitialized)
                return;

            isDisconnecting = true;
            foreach (var dev in devices)
                dev.Disconnect(Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_disconnect_when_stop"));

            DS4Devices.stopControllers();
            devices.Clear();
            IsInitialized = false;
            isDisconnecting = false;
        }

        public override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (keyColors.TryGetValue(key, out var clr))
            {
                foreach (var dev in devices)
                {
                    dev.sendColor = ColorUtils.CorrectWithAlpha(clr);
                    if (dev.device.isDisconnectingStatus())
                    {
                        Reset();
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_restore_dualshock", new RealColor(Color.FromArgb(0, 0, 255)), "Restore Color");
            variableRegistry.Register($"{DeviceName}_devicekey", DeviceKeys.Peripheral, "Key to Use", DeviceKeys.MOUSEPADLIGHT15, DeviceKeys.Peripheral_Logo);
            variableRegistry.Register($"{DeviceName}_disconnect_when_stop", false, "Disconnect when Stopping");
            variableRegistry.Register($"{DeviceName}_auto_init", false, "Initialize automatically when plugged in");
        }
    }
}
