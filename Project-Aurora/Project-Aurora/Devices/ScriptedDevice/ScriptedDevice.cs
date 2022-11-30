using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

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

    public override bool Initialize()
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

                return false;
            }
        }

        return isInitialized && !crashed;
    }

    public override void Reset()
    {
        if (!isInitialized) return;
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
    }

    public override void Shutdown()
    {
        if (!isInitialized) return;
        try
        {
            Reset();
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

    protected override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
    {
        if (!isInitialized) return false;
        try
        {
            return script.UpdateDevice(keyColors, forced);
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, 
                "Device script for {DeviceName} encountered an error during UpdateDevice", devicename);
            crashed = true;
            isInitialized = false;

            return false;
        }
    }
}