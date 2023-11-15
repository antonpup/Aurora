using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Devices;
using Aurora.Modules.Blacklist.Model;
using Aurora.Modules.OnlineConfigs;
using Aurora.Modules.OnlineConfigs.Model;
using Aurora.Modules.ProcessMonitor;
using Aurora.Utils.IpApi;
using ICSharpCode.SharpZipLib.Zip;
using Lombok.NET;
using Microsoft.Win32;

namespace Aurora.Modules;

public sealed partial class OnlineSettings : AuroraModule
{
    public static Dictionary<string, DeviceTooltips> DeviceTooltips = new();
    
    private readonly Task<DeviceManager> _deviceManager;
    private Dictionary<string, ShutdownProcess> _shutdownProcesses = new();
    private readonly TaskCompletionSource _layoutUpdateTaskSource = new();

    public Task LayoutsUpdate => _layoutUpdateTaskSource.Task;

    public OnlineSettings(Task<DeviceManager> deviceManager)
    {
        _deviceManager = deviceManager;
    }

    protected override async Task Initialize()
    {
        var localSettings = await OnlineConfigsRepository.GetOnlineSettingsLocal();
        var localSettingsDate = localSettings.OnlineSettingsTime;
        if (localSettingsDate > DateTimeOffset.MinValue)
        {
            _layoutUpdateTaskSource.TrySetResult();
        }
        
        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

        await DownloadAndExtract();
        _layoutUpdateTaskSource.TrySetResult();
        //TODO update layouts
        await Refresh();

        RunningProcessMonitor.Instance.RunningProcessesChanged += OnRunningProcessesChanged;

        if (Global.Configuration.Lat == 0 && Global.Configuration.Lon == 0)
        {
            try
            {
                var ipData = await IpApiClient.GetIpData();
                Global.Configuration.Lat = ipData.Lat;
                Global.Configuration.Lon = ipData.Lon;
            }
            catch (Exception e)
            {
                Global.logger.Error(e, "Failed getting geographic data");
            }
        }
    }

    private async Task DownloadAndExtract()
    {
        try
        {
            await WaitGithubAccess(TimeSpan.FromSeconds(60));
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Skipped Online Settings update because of internet problem");
            return;
        }

        DateTimeOffset commitDate;
        try
        {
            var settingsMeta = await OnlineConfigsRepository.GetOnlineSettingsOnline();
            commitDate = settingsMeta.OnlineSettingsTime;
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Error fetching online settings");
            return;
        }

        var localSettings = await OnlineConfigsRepository.GetOnlineSettingsLocal();
        var localSettingsDate = localSettings.OnlineSettingsTime;

        if (commitDate <= localSettingsDate)
        {
            return;
        }

        Global.logger.Information("Updating Online Settings");

        try
        {
            await ExtractSettings();
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Error extracting online settings");
        }
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

    private async Task ExtractSettings()
    {
        const string zipUrl = "https://github.com/Aurora-RGB/Online-Settings/archive/refs/heads/master.zip";

        using var webClient = new WebClient();
        using var zipStream = new MemoryStream(webClient.DownloadData(zipUrl));
        await using var zipInputStream = new ZipInputStream(zipStream);
        while (zipInputStream.GetNextEntry() is { } entry)
        {
            if (!entry.IsFile)
                continue;

            var entryName = entry.Name;
            var fullPath = Path.Combine(".", entryName).Replace("\\Online-Settings-master", "");

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            await using var entryFileStream = File.Create(fullPath);
            await zipInputStream.CopyToAsync(entryFileStream);
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

        await DownloadAndExtract();
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
        DeviceTooltips = await OnlineConfigsRepository.GetDeviceTooltips();
        foreach (var device in (await _deviceManager).DeviceContainers.Select(dc => dc.Device))
        {
            if (DeviceTooltips.TryGetValue(device.DeviceName, out var tooltips))
            {
                device.Tooltips = tooltips;
            }
        }
    }

    async Task WaitGithubAccess(TimeSpan timeout)
    {
        var cancelSource = new CancellationTokenSource();

        var resolveTask = WaitUntilResolve("github.com", cancelSource.Token);
        var delayTask = Task.Delay(timeout);

        var completedTask = await Task.WhenAny(resolveTask, delayTask);

        if (completedTask == delayTask)
        {
            cancelSource.Cancel();
            throw new Exception("Failed");
        }
    }

    private async Task WaitUntilResolve(string domain, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var ips = await Dns.GetHostAddressesAsync(domain, cancellationToken);
                if (ips.Length > 0)
                {
                    return;
                }
            }
            catch
            {
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    [Async]
    public override void Dispose()
    {
        RunningProcessMonitor.Instance.RunningProcessesChanged -= OnRunningProcessesChanged;
    }
}