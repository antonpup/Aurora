using System.ComponentModel;
using Common.Devices;

namespace AuroraDeviceManager.Devices;

public class FailedDevice : IDevice
{
    public string DeviceName { get; }
    public string DeviceDetails => "Plugin failed to compile";
    public string DeviceUpdatePerformance => "n/a";
    public bool IsDoingWork => false;
    public bool IsInitialized => false;
    public VariableRegistry RegisteredVariables { get; } = new();

    public FailedDevice(Exception e, string fileName)
    {
        DeviceName = fileName;
    }

    public Task<bool> Initialize()
    {
        return Task.FromResult(false);
    }

    public Task ShutdownDevice()
    {
        return Task.CompletedTask;
    }

    public Task Reset()
    {
        return Task.CompletedTask;
    }

    public Task<bool> UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
    {
        return Task.FromResult(false);
    }

    public string? GetDevices()
    {
        return null;
    }
}