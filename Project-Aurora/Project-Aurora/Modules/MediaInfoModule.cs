using Aurora.Utils;
using Lombok.NET;

namespace Aurora.Modules;

public sealed partial class MediaInfoModule : IAuroraModule
{
    private MediaMonitor _mediaMonitor;
    
    [Async]
    public void Initialize()
    {
        _mediaMonitor = new MediaMonitor();
    }


    [Async]
    public void Dispose()
    {
        _mediaMonitor?.Dispose();
        _mediaMonitor = null;
    }
}