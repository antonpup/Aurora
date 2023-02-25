using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;

namespace Aurora.Devices.ScriptedDevice;

public class ScriptedDevice : DefaultDevice
{
    private bool crashed;
    private readonly dynamic script;

    private string devicename;
    private bool isInitialized;

    private long lastUpdateTime = 0;

    public ScriptedDevice(dynamic script)
    {
        if (
            (script != null) &&
            (script.devicename != null) &&
            (script.enabled != null && script.enabled) &&
            (script.GetType().GetMethod("Initialize") != null) &&
            (script.GetType().GetMethod("Shutdown") != null) &&
            (script.GetType().GetMethod("Reset") != null) &&
            (script.GetType().GetMethod("UpdateDevice") != null)
        )
        {
            devicename = script.devicename;
            this.script = script;
        }
        else
        {
            throw new Exception("Provided script, does not meet all the requirements");
        }
    }

    public override string DeviceDetails
    {
        get
        {
            if (crashed)
                return "Error!";

            return isInitialized ? "Connected" : "Not initialized";
        }
    }

    public override string DeviceName => devicename;

    protected override Task<bool> DoInitialize()
    {
        if (!isInitialized)
        {
            try
            {
                isInitialized = script.Initialize();
            }
            catch (Exception exc)
            {
                Global.logger.Error(exc, "Device script for {DeviceName} encountered an error during Initialization", devicename);
                crashed = true;
                isInitialized = false;

                return Task.FromResult(false);
            }
        }

        return Task.FromResult(isInitialized && !crashed);
    }

    public override Task Reset()
    {
        if (!isInitialized)
        {
            return Task.CompletedTask;
        }

        try
        {
            script.Reset();
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Device script for {DeviceName} encountered an error during Reset", devicename);
            crashed = true;
            isInitialized = false;
        }

        return Task.CompletedTask;
    }

    public override async Task Shutdown()
    {
        if (!isInitialized)
        {
            return;
        }

        try
        {
            await this.Reset();
            script.Shutdown();
            isInitialized = false;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Device script for {DeviceName} encountered an error during Shutdown", devicename);
            crashed = true;
            isInitialized = false;
        }
    }

    protected override Task<bool> UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
    {
        if (!isInitialized) return Task.FromResult(false);

        try
        {
            return Task.FromResult(script.UpdateDevice(keyColors, forced));
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc,
                "Device script for {DeviceName} encountered an error during UpdateDevice", devicename);
            crashed = true;
            isInitialized = false;

            return Task.FromResult(false);
        }
    }
}
