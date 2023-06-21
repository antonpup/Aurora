using System;
using Windows.Media.Control;
using WindowsMediaController;

namespace Aurora.Modules.Media;

public sealed class MediaMonitor : IDisposable
{
    public static bool MediaPlaying { get; private set; }
    public static bool HasMedia { get; private set; }
    public static bool HasNextMedia { get; private set; }
    public static bool HasPreviousMedia { get; private set; }

    private readonly MediaWatcher _mediaManager = new();

    private MediaManager.MediaSession? _currentSession;

    public MediaMonitor()
    {
        _mediaManager.FocusedMediaChanged += MediaManagerOnFocusedMediaChanged;
        _mediaManager.StartListening();
    }

    private void MediaManagerOnFocusedMediaChanged(object? sender, FocusedMediaChangedEventArgs e)
    {
        UpdateButtons(e.MediaSession, e.PlaybackInfo);
    }

    private void UpdateButtons(MediaManager.MediaSession? mediaSession, GlobalSystemMediaTransportControlsSessionPlaybackInfo? playbackInfo)
    {
        if (_currentSession != null)
        {
            _currentSession.OnPlaybackStateChanged -= MediaSession_OnPlaybackStateChanged;
        }
        _currentSession = mediaSession;
        
        HasMedia = _currentSession != null;
        HasNextMedia = playbackInfo?.Controls.IsNextEnabled ?? false;
        HasPreviousMedia = playbackInfo?.Controls.IsPreviousEnabled ?? false;
        MediaPlaying = playbackInfo?.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

        if (_currentSession != null)
        {
            _currentSession.OnPlaybackStateChanged += MediaSession_OnPlaybackStateChanged;
        }
    }

    private void MediaSession_OnPlaybackStateChanged(MediaManager.MediaSession mediaSession,
        GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo)
    {
        HasNextMedia = playbackInfo.Controls.IsNextEnabled;
        HasPreviousMedia = playbackInfo.Controls.IsPreviousEnabled;
        MediaPlaying = playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
    }

    public void Dispose()
    {
        _mediaManager.StopListening();
        if (_currentSession == null) return;
        _currentSession.OnPlaybackStateChanged -= MediaSession_OnPlaybackStateChanged;
        _currentSession = null;
    }
}