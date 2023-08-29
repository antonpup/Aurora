using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aurora.Modules.Blacklist;
using Aurora.Modules.Blacklist.Model;
using Aurora.Modules.ProcessMonitor;
using Lombok.NET;

namespace Aurora.Modules;

public sealed partial class BlacklistMonitor : AuroraModule
{
    private Dictionary<string, ShutdownProcess> _shutdownProcesses = new();

    protected override async Task Initialize()
    {
        var conflictingProcesses = await BlacklistSettingsRepository.GetConflictingProcesses();
        if (!Global.Configuration.EnableShutdownOnConflict || conflictingProcesses.ShutdownAurora == null)
        {
            return;
        }
        
        _shutdownProcesses = conflictingProcesses.ShutdownAurora.ToDictionary(p => p.ProcessName.ToLowerInvariant());

        RunningProcessMonitor.Instance.RunningProcessesChanged += OnRunningProcessesChanged;
    }

    private void OnRunningProcessesChanged(object? sender, RunningProcessChanged e)
    {
        if (!_shutdownProcesses.TryGetValue(e.ProcessName, out var shutdownProcess)) return;
        Global.logger.Fatal("Shutting down Aurora because of a conflicted process {Process}. Reason: {Reason}",
            shutdownProcess.ProcessName, shutdownProcess.Reason);
        App.ForceShutdownApp(-1);
    }

    [Async]
    public override void Dispose()
    {
        RunningProcessMonitor.Instance.RunningProcessesChanged -= OnRunningProcessesChanged;
    }
}