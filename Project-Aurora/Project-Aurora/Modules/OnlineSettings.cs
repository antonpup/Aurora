using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aurora.Devices;
using Aurora.Modules.Blacklist.Model;
using Aurora.Modules.OnlineConfigs;
using Aurora.Modules.ProcessMonitor;
using Lombok.NET;
using Microsoft.Win32;

namespace Aurora.Modules;

public sealed partial class OnlineSettings : AuroraModule
{
    private readonly Task<DeviceManager> _deviceManager;
    private Dictionary<string, ShutdownProcess> _shutdownProcesses = new();

    public OnlineSettings(Task<DeviceManager> deviceManager)
    {
        _deviceManager = deviceManager;
    }

    protected override async Task Initialize()
    {
        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

        await Refresh();

        RunningProcessMonitor.Instance.RunningProcessesChanged += OnRunningProcessesChanged;
    }

    private async Task Refresh()
    {
        try
        {
            await UpdateConflicts();
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Failed to update conflicts");
        }
        try
        {
            await UpdateDeviceInfos();
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Failed to update device infos");
        }
    }

    private async Task UpdateConflicts()
    {
        var conflictingProcesses = await OnlineConfigsRepository.GetConflictingProcesses();
        if (!Global.Configuration.EnableShutdownOnConflict || conflictingProcesses.ShutdownAurora == null)
        {
            return;
        }

        _shutdownProcesses = conflictingProcesses.ShutdownAurora.ToDictionary(p => p.ProcessName.ToLowerInvariant());
    }

    private async void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if (e.Reason != SessionSwitchReason.SessionUnlock)
        {
            return;
        }
        await Refresh();
    }

    private void OnRunningProcessesChanged(object? sender, RunningProcessChanged e)
    {
        if (!_shutdownProcesses.TryGetValue(e.ProcessName, out var shutdownProcess)) return;
        Global.logger.Fatal("Shutting down Aurora because of a conflicted process {Process}. Reason: {Reason}",
            shutdownProcess.ProcessName, shutdownProcess.Reason);
        App.ForceShutdownApp(-1);
    }

    private async Task UpdateDeviceInfos()
    {
        var deviceTooltips = await OnlineConfigsRepository.GetDeviceTooltips();
        foreach (var device in (await _deviceManager).DeviceContainers.Select(dc => dc.Device))
        {
            if (deviceTooltips.TryGetValue(device.DeviceName, out var tooltips))
            {
                device.Tooltips = tooltips;
            }
        }
    }

    [Async]
    public override void Dispose()
    {
        RunningProcessMonitor.Instance.RunningProcessesChanged -= OnRunningProcessesChanged;
    }
}