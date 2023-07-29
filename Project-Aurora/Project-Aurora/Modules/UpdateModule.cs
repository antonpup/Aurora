using System.Threading.Tasks;
using Aurora.Utils;

namespace Aurora.Modules;

public sealed class UpdateModule : AuroraModule
{
    public bool IgnoreUpdate;
    
    protected override async Task Initialize()
    {
        if (Global.Configuration.UpdatesCheckOnStartUp && !IgnoreUpdate)
        {
            await DesktopUtils.CheckUpdate();
        }
    }

    public override Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
    }
}