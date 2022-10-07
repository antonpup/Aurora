using System.Collections.Generic;
using System.Linq;
using Windows.Media.Control;
using WindowsMediaController;

namespace Aurora.Utils
{
    public static class MediaMonitor
    {
        public static bool MediaPlaying;
        public static bool HasMedia;
        public static bool HasNextMedia;
        public static bool HasPreviousMedia;
        
        private static readonly MediaManager MediaManager = new();

        private static readonly HashSet<MediaManager.MediaSession> MediaSessions = new(new MediaSessionComparer());

        static MediaMonitor()
        {
            MediaManager.OnAnySessionOpened += MediaManager_OnSessionOpened;
            MediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
            MediaManager.OnAnySessionClosed += MediaManager_OnAnySessionClosed;

            MediaManager.Start();
            
            HasMedia = MediaManager.CurrentMediaSessions.Count > 0;
            if (!HasMedia) return;

            foreach (var (_, mediaSession) in MediaManager.CurrentMediaSessions)
            {
                MediaSessions.Add(mediaSession);
            }

            UpdateButtons();
        }

        private static void MediaManager_OnSessionOpened(MediaManager.MediaSession mediaSession)
        {
            HasMedia = true;
            MediaSessions.Add(mediaSession);
            UpdateButtons();
        }

        private static void MediaManager_OnAnySessionClosed(MediaManager.MediaSession mediaSession)
        {
            mediaSession.OnPlaybackStateChanged -= MediaManager_OnAnyPlaybackStateChanged;
            MediaSessions.Remove(mediaSession);

            UpdateButtons();
        }

        private static void MediaManager_OnAnyPlaybackStateChanged(MediaManager.MediaSession mediaSession,
            GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo)
        {
            MediaSessions.Add(mediaSession);
            UpdateButtons();
        }

        private static void UpdateButtons()
        {
            HasMedia = MediaSessions.Count > 0;
            HasNextMedia = MediaSessions.Any(
                    value => value.ControlSession.GetPlaybackInfo().Controls.IsNextEnabled);
            HasPreviousMedia = MediaSessions.Any(value =>
                value.ControlSession.GetPlaybackInfo().Controls.IsPreviousEnabled);
            MediaPlaying = MediaManager.CurrentMediaSessions.Any(pair =>
                pair.Value.ControlSession.GetPlaybackInfo().PlaybackStatus ==
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing);
        }
    }

    internal class MediaSessionComparer : IEqualityComparer<MediaManager.MediaSession>
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