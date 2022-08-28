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

        private static readonly List<string> MediaSessions = new();

        static MediaMonitor()
        {
            MediaManager.OnAnySessionOpened += MediaManager_OnSessionOpened;
            MediaManager.OnAnySessionClosed += MediaManager_OnAnySessionClosed;
            MediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;

            MediaManager.Start();
            
            HasMedia = MediaManager.CurrentMediaSessions.Count > 0;

            if (MediaManager.CurrentMediaSessions.Count == 0) return;
            UpdateButtons();
        }

        private static void MediaManager_OnSessionOpened(MediaManager.MediaSession mediasession)
        {
            HasMedia = true;
            
            MediaSessions.Add(mediasession.Id);

            mediasession.OnSessionClosed += MediaManager_OnAnySessionClosed;
        }

        private static void MediaManager_OnAnySessionClosed(MediaManager.MediaSession mediasession)
        {
            mediasession.OnSessionClosed -= MediaManager_OnAnySessionClosed;
            MediaSessions.Remove(mediasession.Id);

            if (MediaSessions.Count == 0)
            {
                HasMedia = false;
                MediaPlaying = false;
                HasNextMedia = false;
                HasPreviousMedia = false;
            }
            else
            {
                UpdateButtons();
            }
        }

        private static void MediaManager_OnAnyPlaybackStateChanged(MediaManager.MediaSession mediasession, GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackinfo)
        {
            UpdateButtons();
        }

        private static void UpdateButtons()
        {
            HasNextMedia = MediaManager.CurrentMediaSessions.Any(
                    pair => pair.Value.ControlSession.GetPlaybackInfo().Controls.IsNextEnabled);
            HasPreviousMedia = MediaManager.CurrentMediaSessions.Any(pair =>
                    pair.Value.ControlSession.GetPlaybackInfo().Controls.IsPreviousEnabled);
            MediaPlaying = MediaManager.CurrentMediaSessions.Any(pair =>
                pair.Value.ControlSession.GetPlaybackInfo().PlaybackStatus ==
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing);
        }
    }
}