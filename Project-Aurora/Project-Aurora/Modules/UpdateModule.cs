using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Common.Utils;
using Microsoft.Win32;

namespace Aurora.Modules;

public sealed class UpdateModule : AuroraModule
{
    public bool IgnoreUpdate;
    
    protected override async Task Initialize()
    {
        if (Global.Configuration.UpdatesCheckOnStartUp && !IgnoreUpdate)
        {
            await CheckUpdate();
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }
    }

    private async void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if (e.Reason != SessionSwitchReason.SessionUnlock)
        {
            return;
        }
        await CheckUpdate();
    }

    public static async Task CheckUpdate()
    {
        var updaterPath = Path.Combine(Global.ExecutingDirectory, "Aurora-Updater.exe");

        if (!File.Exists(updaterPath)) return;
        await DesktopUtils.WaitSessionUnlock();
        try
        {
            var updaterProc = new ProcessStartInfo
            {
                FileName = updaterPath,
                Arguments = "-silent"
            };
            var process = Process.Start(updaterProc);
#if DEBUG
            await process!.WaitForExitAsync();
#endif
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Could not start Aurora Updater");
        }
    }

    public override Task DisposeAsync()
    {
        SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
    }
}