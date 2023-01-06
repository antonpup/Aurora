using System;
using System.Windows;
using Aurora.Modules.Media;
using Lombok.NET;

namespace Aurora.Modules;

public sealed partial class MediaInfoModule : IAuroraModule
{
    private MediaMonitor _mediaMonitor;
    
    [Async]
    public void Initialize()
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
            MessageBox.Show("Media Info module could not be loaded.\nMedia playback data will not be detected.", "Aurora - Error");
            Global.logger.Error("MediaInfo error", e);
        }
    }


    [Async]
    public void Dispose()
    {
        _mediaMonitor?.Dispose();
        _mediaMonitor = null;
    }
}