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
using Aurora.Modules.ProcessMonitor;
using ICSharpCode.SharpZipLib.Zip;
using Lombok.NET;
using Microsoft.Win32;
using Octokit;

namespace Aurora.Modules;

public sealed partial class OnlineSettings : AuroraModule
{
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
        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

        await DownloadAndExtract();
        _layoutUpdateTaskSource.TrySetResult();
        await Refresh();

        RunningProcessMonitor.Instance.RunningProcessesChanged += OnRunningProcessesChanged;
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

        var settingsMeta = await OnlineConfigsRepository.GetOnlineSettingsOnline();
        var commitDate = settingsMeta.OnlineSettingsTime;

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
        var deviceTooltips = await OnlineConfigsRepository.GetDeviceTooltips();
        foreach (var device in (await _deviceManager).DeviceContainers.Select(dc => dc.Device))
        {
            if (deviceTooltips.TryGetValue(device.DeviceName, out var tooltips))
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