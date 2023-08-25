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

namespace Aurora.Devices.Dualshock;

internal class DS4Container
{
    public readonly DS4Device Device;
    private readonly ConnectionType _connectionType;
    private readonly Color _restoreColor;

    public int Battery { get; private set; }
    public double Latency { get; private set; }
    public bool Charging { get; private set; }

    public Color SendColor;
    public DS4HapticState State;

    public DS4Container(DS4Device device, Color restoreColor)
    {
        Device = device;
        _connectionType = Device.getConnectionType();
        Device.Report += OnDeviceReport;
        Device.StartUpdate();
        _restoreColor = restoreColor;
    }

    private void OnDeviceReport(object? sender, EventArgs e)
    {
        Battery = Device.Battery;
        Latency = Device.Latency;
        Charging = Device.Charging;

        if (ColorsEqual(SendColor, State.LightBarColor))
            return;

        State.LightBarExplicitlyOff = SendColor.R == 0 && SendColor.G == 0 && SendColor.B == 0;
        State.LightBarColor = new DS4Color(SendColor);
        Device.pushHapticState(State);
    }

    public void Disconnect(bool stop)
    {
        SendColor = _restoreColor;
        Thread.Sleep(10);//HACK: this is terrible, but it works reliably so i'll leave it like this :(
        //this is needed so that OnDeviceReport has time to send the color once the device sends a report.
        Device.Report -= OnDeviceReport;
        if (stop)
        {
            Device.DisconnectBT();
            Device.DisconnectDongle();
        }
        Device.StopUpdate();
    }

    public string GetDeviceDetails()
    {
        var connectionString = _connectionType switch
        {
            ConnectionType.BT => "Bluetooth",
            ConnectionType.SONYWA => "DS4 Wireless adapter",
            ConnectionType.USB => "USB",
            _ => "",
        };

        return $"over {connectionString} {(Charging ? "⚡" : "")}🔋{Battery}% Latency: {Latency:0.00}ms";
    }

    private bool ColorsEqual(Color clr, DS4Color ds4Clr)
    {
        return clr.R == ds4Clr.red &&
               clr.G == ds4Clr.green &&
               clr.B == ds4Clr.blue;
    }
}

public class DualshockDevice : DefaultDevice
{
    public static DualshockDevice? Instance;
        
    public override string DeviceName => "Sony DualShock 4(PS4)";

    protected override string DeviceInfo =>
        string.Join(", ", devices.Select((dev, i) => $"#{i + 1} {dev.GetDeviceDetails()}"));

    public int Battery => devices.FirstOrDefault()?.Battery ?? -1;
    public double Latency => devices.FirstOrDefault()?.Latency ?? -1;
    public bool Charging => devices.FirstOrDefault()?.Charging ?? false;

    private readonly List<DS4Container> devices = new();
    private DeviceKeys _key;

    public DualshockDevice()
    {
        Instance = this;
    }

    protected override Task<bool> DoInitialize()
    {
        if (IsInitialized)
            return Task.FromResult(true);

        _key = Global.Configuration.VarRegistry.GetVariable<DeviceKeys>($"{DeviceName}_devicekey");
        DS4Devices.findControllers();

        var restore = Global.Configuration.VarRegistry.GetVariable<RealColor>($"{DeviceName}_restore_dualshock").GetDrawingColor();

        foreach (var controller in DS4Devices.getDS4Controllers())
            devices.Add(new DS4Container(controller, restore));

        return Task.FromResult(IsInitialized = devices.Count > 0);
    }

    protected override Task Shutdown()
    {
        if (!IsInitialized)
        {
            return Task.CompletedTask;
        }

        foreach (var dev in devices)
            dev.Disconnect(Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_disconnect_when_stop"));

        DS4Devices.stopControllers();
        devices.Clear();
        IsInitialized = false;
        return Task.CompletedTask;
    }

    protected override async Task<bool> UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
    {
        if (!keyColors.TryGetValue(_key, out var clr)) return false;
        foreach (var dev in devices)
        {
            dev.SendColor = ColorUtils.CorrectWithAlpha(clr);
            if (!dev.Device.isDisconnectingStatus()) continue;
            await Reset().ConfigureAwait(false);
            return false;
        }

        return true;

    }

    protected override void RegisterVariables(VariableRegistry variableRegistry)
    {
        variableRegistry.Register($"{DeviceName}_restore_dualshock", new RealColor(Color.FromArgb(0, 0, 255)), "Restore Color");
        variableRegistry.Register($"{DeviceName}_devicekey", DeviceKeys.Peripheral, "Key to Use", DeviceKeys.MOUSEPADLIGHT15, DeviceKeys.Peripheral_Logo);
        variableRegistry.Register($"{DeviceName}_disconnect_when_stop", false, "Disconnect when Stopping");
        variableRegistry.Register($"{DeviceName}_auto_init", false, "Initialize automatically when plugged in");
    }
}