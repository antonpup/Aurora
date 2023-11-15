using System.ComponentModel;
using System.Drawing;
using Common.Devices;

namespace AuroraDeviceManager.Devices.ScriptedDevice;

public class ScriptedDevice : DefaultDevice
{
    private bool _crashed;
    private readonly dynamic _script;

    private readonly string _deviceName;
    private bool _isInitialized;

    public ScriptedDevice(dynamic script)
    {
        if (
            script != null &&
            script.devicename != null &&
            (script.enabled != null && script.enabled) &&
            script.GetType().GetMethod("Initialize") != null &&
            script.GetType().GetMethod("Shutdown") != null &&
            script.GetType().GetMethod("Reset") != null &&
            script.GetType().GetMethod("UpdateDevice") != null
        )
        {
            _deviceName = script.devicename;
            _script = script;
        }
        else
        {
            throw new InvalidOperationException("Script does not meet all the requirements");
        }
    }

    public override string DeviceDetails
    {
        get
        {
            if (_crashed)
                return "Error!";

            return _isInitialized ? "Connected" : "Not initialized";
        }
    }

    public override string DeviceName => _deviceName;

    protected override Task<bool> DoInitialize()
    {
        if (_isInitialized) return Task.FromResult(_isInitialized && !_crashed);
        try
        {
            _isInitialized = _script.Initialize();
        }
        catch (Exception exc)
        {
            Global.Logger.Error(exc, "Device script for {DeviceName} encountered an error during Initialization", _deviceName);
            _crashed = true;
            _isInitialized = false;

            return Task.FromResult(false);
        }

        return Task.FromResult(_isInitialized && !_crashed);
    }

    public override Task Reset()
    {
        if (!_isInitialized) return Task.CompletedTask;
        try
        {
            _script.Reset();
        }
        catch (Exception exc)
        {
            Global.Logger.Error(exc, "Device script for {DeviceName} encountered an error during Reset", _deviceName);
            _crashed = true;
            _isInitialized = false;
        }

        return Task.CompletedTask;
    }

    protected override async Task Shutdown()
    {
        if (!_isInitialized) return;
        try
        {
            await this.Reset().ConfigureAwait(false);
            _script.Shutdown();
            _isInitialized = false;
        }
        catch (Exception exc)
        {
            Global.Logger.Error(exc, "Device script for {DeviceName} encountered an error during Shutdown", _deviceName);
            _crashed = true;
            _isInitialized = false;
        }
    }

    protected override Task<bool> UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
    {
        if (!_isInitialized) return Task.FromResult(false);
        try
        {
            return Task.FromResult(_script.UpdateDevice(keyColors, forced));
        }
        catch (Exception exc)
        {
            Global.Logger.Error(exc, 
                "Device script for {DeviceName} encountered an error during UpdateDevice", _deviceName);
            _crashed = true;
            _isInitialized = false;

            return Task.FromResult(false);
        }
    }
}
