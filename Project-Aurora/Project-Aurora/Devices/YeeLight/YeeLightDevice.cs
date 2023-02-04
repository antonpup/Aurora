using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Aurora.Devices.YeeLight;

public class YeeLightDevice : DefaultDevice
{
    public override string DeviceName => "YeeLight";

    private readonly Stopwatch _updateDelayStopWatch = new();
    private readonly List<YeeLightAPI.YeeLightDevice> _lights = new();

    private IYeeLightState _yeeLightState;
    private YeelightConnectionListener _connectionListener;

    protected override string DeviceInfo => String.Join(
        ", ",
        _lights.Select(light => light.GetLightIPAddressAndPort().ipAddress + ":" +
                                light.GetLightIPAddressAndPort().port +
                                (light.IsMusicMode() ? "(m)" : ""))
    );

    public override bool Initialize()
    {
        if (IsInitialized) return IsInitialized;
        try
        {
            var ipListString = Global.Configuration.VarRegistry.GetVariable<string>($"{DeviceName}_IP");
            var autoDiscover = Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_auto_discovery");
            YeeLightConnector.PopulateDevices(_lights, autoDiscover ? null : ipListString);

            InitiateState();
            _updateDelayStopWatch.Start();
            IsInitialized = true;
            _connectionListener = autoDiscover ? new YeelightConnectionListener() :
                new YeelightConnectionListener(ipListString.Split(','));
            _connectionListener.DeviceDetected += DeviceDetected;
            _connectionListener.StartListeningConnections();
        }
        catch (Exception exc)
        {
            LogError($"Encountered an error while initializing. Exception: {exc}");
            IsInitialized = false;

            return false;
        }

        return IsInitialized;
    }

    private void DeviceDetected(object sender, DeviceDetectedEventArgs e)
    {
        YeeLightConnector.ConnectNewDevice(_lights, e.IpAddress);
    }

    private void InitiateState()
    {
        _yeeLightState = YeeLightStateBuilder.Build(_lights, Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_white_delay"));
    }

    public override void Shutdown()
    {
        foreach (var light in _lights.Where(x => x.IsConnected()))
        {
            light.CloseConnection();
        }

        _lights.Clear();

        IsInitialized = false;

        if (_updateDelayStopWatch.IsRunning)
        {
            _updateDelayStopWatch.Stop();
        }
    }

    protected override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
    {
        try
        {
            return TryUpdate(keyColors);
        }catch(Exception exc)
        {
            Reset();
            return TryUpdate(keyColors);
        }
    }

    private bool TryUpdate(IReadOnlyDictionary<DeviceKeys, Color> keyColors)
    {
        var sendDelay = Math.Max(5, Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_send_delay"));
        if (_updateDelayStopWatch.ElapsedMilliseconds <= sendDelay)
            return false;

        var targetKey = Global.Configuration.VarRegistry.GetVariable<DeviceKeys>($"{DeviceName}_devicekey");
        if (!keyColors.TryGetValue(targetKey, out var targetColor))
            return false;

        _yeeLightState = _yeeLightState.Update(targetColor);
        _updateDelayStopWatch.Restart();
            
        return true;
    }

    protected override void RegisterVariables(VariableRegistry variableRegistry)
    {
        var devKeysEnumAsEnumerable = Enum.GetValues(typeof(DeviceKeys)).Cast<DeviceKeys>();

        var keysEnumAsEnumerable = devKeysEnumAsEnumerable as DeviceKeys[] ?? devKeysEnumAsEnumerable.ToArray();
        variableRegistry.Register($"{DeviceName}_devicekey", DeviceKeys.Peripheral_Logo, "Key to Use",
            keysEnumAsEnumerable.Max(), keysEnumAsEnumerable.Min());
        variableRegistry.Register($"{DeviceName}_send_delay", 35, "Send delay (ms)");
        variableRegistry.Register($"{DeviceName}_IP", "", "YeeLight IP(s)",
            null, null, "Comma separated IPv4 or IPv6 addresses.");
        variableRegistry.Register($"{DeviceName}_auto_discovery", false, "Auto-discovery",
            null, null, "Enable this and empty out the IP field to auto-discover lights.");
        variableRegistry.Register($"{DeviceName}_white_delay", 10, "White mode delay(ticks)",
            null, null, "How many ticks should happen before white mode is activated.");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
            
        _connectionListener.StopListeningConnections();
        _connectionListener.DeviceDetected -= DeviceDetected;
    }
}