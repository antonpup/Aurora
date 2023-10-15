using System.Drawing;
using AurorDeviceManager.Devices.RGBNet;
using Common.Devices;
using AurorDeviceManager.Devices.ScriptedDevice;
using Common;
using Common.Data;
using Microsoft.Win32;

namespace AurorDeviceManager.Devices;

public sealed class DeviceManager : IDisposable
{
    private bool _suspended;
    private bool _resumed;
    private bool _disposed;
    private readonly MemorySharedStruct<DeviceManagerInfo> _deviceInformations;

    private List<DeviceContainer> DeviceContainers { get; } = new();

    private IEnumerable<DeviceContainer> InitializedDeviceContainers =>
        DeviceContainers.Where(d => d.Device.IsInitialized);

    public DeviceManager()
    {
        const string devicesPath = "Devices";
        IEnumerable<IDeviceLoader> deviceLoaders = new IDeviceLoader[]
        {
            new AssemblyDeviceLoader(),
            new ScriptDeviceLoader(Path.Combine(Global.ExecutingDirectory, "Scripts", devicesPath)),
            new ScriptDeviceLoader(Path.Combine(Global.AppDataDirectory, "Scripts", devicesPath)),
            new DllDeviceLoader(Path.Combine(Global.ExecutingDirectory, "Plugins", devicesPath)),
            new DllDeviceLoader(Path.Combine(Global.AppDataDirectory, "Plugins", devicesPath))
        };

        foreach (var deviceLoader in deviceLoaders)
        {
            foreach (var device in deviceLoader.LoadDevices().Where(d => d != null))
            {
                DeviceContainers.Add(new DeviceContainer(device!));
            }

            deviceLoader.Dispose();
        }

        _deviceInformations = new MemorySharedStruct<DeviceManagerInfo>(Constants.DeviceInformations, CreateDeviceManagerInfo());

        UpdateSharedDeviceInfos();
        _deviceInformations.UpdateRequested += (_, _) =>
        {
            UpdateSharedDeviceInfos();
        };

        SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
    }

    private void UpdateSharedDeviceInfos()
    {
        Global.Logger.Information("Updating device infos");
        var a = CreateDeviceManagerInfo();
        _deviceInformations.WriteObject(a);
    }

    private DeviceManagerInfo CreateDeviceManagerInfo()
    {
        var devices = string.Join('~', DeviceContainers
            .Select(dc => dc.Device)
            .Select(d => d.DeviceName));

        return new DeviceManagerInfo(devices);
    }

    public void RegisterVariables()
    {
        foreach (var device in DeviceContainers)
        {
            device.UpdateVariables();
        }
    }

    public async Task InitializeDevices()
    {
        if (_suspended)
            return;

        var initializeTasks = DeviceContainers
            .Where(dc => dc.Device is { isDoingWork: false })
            .Where(dc => dc.Device.IsInitialized ^ DeviceEnabled(dc))
            .Select(deviceContainer => deviceContainer.Device.IsInitialized
                ? deviceContainer.DisableDevice()
                : deviceContainer.EnableDevice());

        await Task.WhenAll(initializeTasks);
    }

    private static bool DeviceEnabled(DeviceContainer dc)
    {
        return Global.Configuration.EnabledDevices.Contains(dc.Device.DeviceName);
    }

    public Task ShutdownDevices()
    {
        var shutdownTasks = InitializedDeviceContainers.Select(dc => dc.DisableDevice());

        return Task.WhenAll(shutdownTasks);
    }

    public void UpdateDevices(Dictionary<DeviceKeys, Color> composition)
    {
        if (_disposed)
            return;
        foreach (var dc in InitializedDeviceContainers)
        {
            dc.UpdateDevice(composition);
        }
    }

    #region SystemEvents

    private async void SystemEvents_PowerModeChanged(object? sender, PowerModeChangedEventArgs e)
    {
        switch (e.Mode)
        {
            case PowerModes.Suspend:
                Global.Logger.Information("Suspending Devices");
                _suspended = true;
                await Task.Run(async () => await ShutdownDevices());
                break;
            case PowerModes.Resume:
                Global.Logger.Information("Resuming Devices -- PowerModes.Resume");
                Thread.Sleep(TimeSpan.FromSeconds(5));
                _resumed = true;
                _suspended = false;
                await Task.Run(async () => await InitializeDevices());
                break;
        }
    }

    #endregion

    public void Dispose()
    {
        _disposed = true;
        DeviceContainers.Clear();
    }

    public void BlinkDevice(string deviceId)
    {      
        var rgbNetDevices = InitializedDeviceContainers
            .Select(container => container.Device)
            .Where(d => d is RgbNetDevice)
            .Cast<RgbNetDevice>();
        var devicesToBlink = rgbNetDevices.SelectMany(d => d.DeviceList)
            .Where(rgbDevice => rgbDevice.DeviceInfo.DeviceName == deviceId);
        
        
    }
}