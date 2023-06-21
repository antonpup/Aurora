using System;
using System.Threading.Tasks;
using Windows.Media.Control;
using WindowsMediaController;

namespace Aurora.Modules.Media;

/// <summary>
/// Wrapping class that it's only dependency is Dubya.WindowsMediaController
/// </summary>
public class MediaWatcher
{
    private readonly MediaManager _mediaManager = new();

    private MediaManager.MediaSession? _currentSession;

    public event EventHandler<FocusedMediaChangedEventArgs>? FocusedMediaChanged;

    public async Task StartListening()
    {
        _mediaManager.OnAnySessionOpened += MediaManager_OnSessionOpened;
        _mediaManager.OnAnySessionClosed += MediaManager_OnAnySessionClosed;
        _mediaManager.OnFocusedSessionChanged += MediaManagerOnOnFocusedSessionChanged;
        _mediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;  //listen for media arts

        await _mediaManager.StartAsync();
    }

    public void StopListening()
    {
        if (_currentSession != null)
        {
            _currentSession.OnPlaybackStateChanged -= MediaSession_OnPlaybackStateChanged;
        }
        _currentSession = null;

        _mediaManager.OnAnyMediaPropertyChanged -= MediaManager_OnAnyMediaPropertyChanged;
        _mediaManager.OnFocusedSessionChanged -= MediaManagerOnOnFocusedSessionChanged;
        _mediaManager.OnAnySessionClosed -= MediaManager_OnAnySessionClosed;
        _mediaManager.OnAnySessionOpened -= MediaManager_OnSessionOpened;
        _mediaManager.Dispose();
    }

    private void MediaManagerOnOnFocusedSessionChanged(MediaManager.MediaSession? mediaSession)
    {
        if (_currentSession != null)
        {
            _currentSession.OnPlaybackStateChanged -= MediaSession_OnPlaybackStateChanged;
        }

        if (mediaSession != null)
        {
            _currentSession = mediaSession;
            _currentSession.OnPlaybackStateChanged += MediaSession_OnPlaybackStateChanged;
        }
        FocusedMediaChanged?.Invoke(this, new FocusedMediaChangedEventArgs(mediaSession, mediaSession?.ControlSession.GetPlaybackInfo()));
    }

    private void MediaManager_OnSessionOpened(MediaManager.MediaSession mediaSession)
    {
        _mediaManager.ForceUpdate();
    }

    private void MediaManager_OnAnySessionClosed(MediaManager.MediaSession mediaSession)
    {
        _mediaManager.ForceUpdate();
    }

    private void MediaManager_OnAnyMediaPropertyChanged(MediaManager.MediaSession mediaSession,
        GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties)
    {
        if (mediaSession.ControlSession == null)
        {
            MediaManager_OnAnySessionClosed(mediaSession);
        }
    }

    private void MediaSession_OnPlaybackStateChanged(MediaManager.MediaSession mediaSession,
        GlobalSystemMediaTransportControlsSessionPlaybackInfo? playbackInfo)
    {
        if (playbackInfo == null || mediaSession.ControlSession == null ||
            playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed ||
            playbackInfo.Controls == null)
        {
            MediaManager_OnAnySessionClosed(mediaSession);
            return;
        }

        FocusedMediaChanged?.Invoke(this, new FocusedMediaChangedEventArgs(mediaSession, playbackInfo));
    }
}