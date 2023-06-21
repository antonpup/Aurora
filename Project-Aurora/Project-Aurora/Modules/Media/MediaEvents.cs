using System;
using Windows.Media.Control;
using WindowsMediaController;

namespace Aurora.Modules.Media;

public class FocusedMediaChangedEventArgs : EventArgs
{
    public MediaManager.MediaSession? MediaSession { get; }
    public GlobalSystemMediaTransportControlsSessionPlaybackInfo? PlaybackInfo { get; }

    public FocusedMediaChangedEventArgs(MediaManager.MediaSession? mediaSession,
        GlobalSystemMediaTransportControlsSessionPlaybackInfo? playbackInfo)
    {
        MediaSession = mediaSession;
        PlaybackInfo = playbackInfo;
    }
}