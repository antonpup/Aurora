using System;
using System.Threading.Tasks;
using System.Windows;
using Aurora.Modules.Media;
using Lombok.NET;

namespace Aurora.Modules;

public sealed partial class MediaInfoModule : AuroraModule
{
    private MediaMonitor? _mediaMonitor;

    protected override async Task Initialize()
    {
        if (!Global.Configuration.EnableMediaInfo)
        {
            return;
        }
        try
        {
            _mediaMonitor = new MediaMonitor();
        }
        catch (Exception e)
        {
            MessageBox.Show("Media Info module could not be loaded.\nMedia playback data will not be detected.",
                "Aurora - Warning");
            Global.logger.Error("MediaInfo error", e);
        }
    }

    [Async]
    public override void Dispose()
    {
        _mediaMonitor?.Dispose();
        _mediaMonitor = null;
    }
}