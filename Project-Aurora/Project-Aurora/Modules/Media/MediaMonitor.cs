using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Media.Control;
using WindowsMediaController;

namespace Aurora.Modules.Media;

public sealed class MediaMonitor : IDisposable
{
    public static bool MediaPlaying { get; private set; }
    public static bool HasMedia { get; private set; }
    public static bool HasNextMedia { get; private set; }
    public static bool HasPreviousMedia { get; private set; }

    private readonly MediaManager _mediaManager = new();

    private readonly HashSet<MediaManager.MediaSession> _mediaSessions = new(new MediaSessionComparer());

    public MediaMonitor()
    {
        _mediaManager.OnAnySessionOpened += MediaManager_OnSessionOpened;
        _mediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
        _mediaManager.OnAnySessionClosed += MediaManager_OnAnySessionClosed;

        _mediaManager.Start();
    }

    private void MediaManager_OnSessionOpened(MediaManager.MediaSession mediaSession)
    {
        _mediaSessions.Add(mediaSession);
        UpdateButtons();
    }

    private void MediaManager_OnAnySessionClosed(MediaManager.MediaSession mediaSession)
    {
        _mediaSessions.Remove(mediaSession);
        UpdateButtons();
    }

    private void MediaManager_OnAnyPlaybackStateChanged(MediaManager.MediaSession mediaSession,
        GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo)
    {
        if (playbackInfo.PlaybackStatus != GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed)
        {
            _mediaSessions.Add(mediaSession);
        }
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        HasMedia = _mediaSessions.Count > 0;
        HasNextMedia = _mediaSessions.Any(
            value => value.ControlSession.GetPlaybackInfo().Controls.IsNextEnabled);
        HasPreviousMedia = _mediaSessions.Any(value =>
            value.ControlSession.GetPlaybackInfo().Controls.IsPreviousEnabled);
        MediaPlaying = _mediaManager.CurrentMediaSessions.Any(pair =>
            pair.Value.ControlSession.GetPlaybackInfo().PlaybackStatus ==
            GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing);
    }

    public void Dispose()
    {
        _mediaManager.OnAnySessionOpened -= MediaManager_OnSessionOpened;
        _mediaManager.OnAnyPlaybackStateChanged -= MediaManager_OnAnyPlaybackStateChanged;
        _mediaManager.OnAnySessionClosed -= MediaManager_OnAnySessionClosed;
        _mediaManager.Dispose();
    }

    private sealed class MediaSessionComparer : IEqualityComparer<MediaManager.MediaSession>
    {
        public bool Equals(MediaManager.MediaSession x, MediaManager.MediaSession y)
        {
            return x?.Id == y?.Id;
        }

        public int GetHashCode(MediaManager.MediaSession obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}