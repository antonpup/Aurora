using Aurora.Utils;
using Lombok.NET;

namespace Aurora.Modules;

public partial class PointerUpdateModule : IAuroraModule
{
    [Async]
    public void Initialize()
    {
        if (!Global.Configuration.GetPointerUpdates) return;
        Global.logger.Info("Fetching latest pointers");
        PointerUpdateUtils.FetchDevPointers("master");
    }


    [Async]
    public void Dispose()
    {
        //noop
    }
}