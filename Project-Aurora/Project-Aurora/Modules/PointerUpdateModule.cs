using System.Threading.Tasks;
using Aurora.Utils;
using Lombok.NET;

namespace Aurora.Modules;

public sealed partial class PointerUpdateModule : AuroraModule
{
    protected override async Task Initialize()
    {
        if (!Global.Configuration.GetPointerUpdates) return;
        Global.logger.Information("Fetching latest pointers");
        PointerUpdateUtils.FetchDevPointers("master");
    }

    [Async]
    public override void Dispose()
    {
        //noop
    }
}