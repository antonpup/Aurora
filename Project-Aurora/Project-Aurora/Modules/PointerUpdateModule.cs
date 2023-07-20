using Aurora.Utils;
using Lombok.NET;

namespace Aurora.Modules;

public sealed partial class PointerUpdateModule : IAuroraModule
{
    public override void Initialize()
    {
        if (!Global.Configuration.GetPointerUpdates) return;
        Global.logger.Info("Fetching latest pointers");
        PointerUpdateUtils.FetchDevPointers("master");
    }


    [Async]
    public override void Dispose()
    {
        //noop
    }
}