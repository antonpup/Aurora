using System;
using System.Collections.Generic;
using System.Drawing;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationMix
    {
        [Newtonsoft.Json.JsonProperty]
        private Dictionary<string, AnimationTrack> _tracks;

        /// <summary>
        /// When true, will remove Animation tracks that no longer have any animations.
        /// </summary>
        [Newtonsoft.Json.JsonProperty]
        private bool _automatically_remove_complete;

        public AnimationMix()
        {
            _tracks = new Dictionary<string, AnimationTrack>();
            _automatically_remove_complete = false;
        }

        public AnimationMix(AnimationTrack[] tracks)
        {
            _tracks = new Dictionary<string, AnimationTrack>();
            _automatically_remove_complete = false;

            foreach (var track in tracks)
                AddTrack(track);
        }

        public AnimationMix SetAutoRemove(bool value)
        {
            _automatically_remove_complete = value;

            return this;
        }

        public AnimationMix AddTrack(AnimationTrack track)
        {
            if (track != null)
            {
                if (_tracks.ContainsKey(track.GetName()))
                    _tracks[track.GetName()] = track;
                else
                    _tracks.Add(track.GetName(), track);
            }

            return this;
        }

        public AnimationMix AddTrack(string track_name, AnimationTrack track)
        {
            if (_tracks.ContainsKey(track_name))
                _tracks[track_name] = track;
            else
                _tracks.Add(track_name, track);

            return this;
        }

        public AnimationMix RemoveTrack(string track_name)
        {
            if (_tracks.ContainsKey(track_name))
                _tracks.Remove(track_name);

            return this;
        }

        public AnimationTrack GetTrack(string track_name)
        {
            if (_tracks.ContainsKey(track_name))
                return _tracks[track_name];
            else
                return null;
        }

        public bool ContainsTrack(string track_name)
        {
            return _tracks.ContainsKey(track_name);
        }

        public float GetDuration()
        {
            Dictionary<string, AnimationTrack> _local = new Dictionary<string, AnimationTrack>(_tracks);

            float return_val = 0.0f;

            foreach (KeyValuePair<string, AnimationTrack> track in _local)
            {
                if (track.Value.AnimationDuration > return_val)
                    return_val = track.Value.AnimationDuration;
            }

            return return_val;
        }

        public Dictionary<string, AnimationTrack> GetTracks()
        {
            return new Dictionary<string, AnimationTrack>(_tracks);
        }

        public bool AnyActiveTracksAt(float time)
        {
            Dictionary<string, AnimationTrack> _local = new Dictionary<string, AnimationTrack>(_tracks);

            bool return_val = false;

            foreach (KeyValuePair<string, AnimationTrack> track in _local)
            {
                if (track.Value.ContainsAnimationAt(time))
                    return_val = true;
            }

            return return_val;
        }

        public void Draw(Graphics g, float time, float scale = 1.0f, PointF offset = default(PointF))
        {
            Dictionary<string, AnimationTrack> _local = new Dictionary<string, AnimationTrack>(_tracks);

            foreach (KeyValuePair<string, AnimationTrack> track in _local)
            {
                if (track.Value.ContainsAnimationAt(time))
                {
                    try
                    {
                        track.Value.GetFrame(time).Draw(g, scale, offset);
                    }
                    catch (Exception exc)
                    {
                        System.Console.WriteLine();
                    }

                }
                else
                {
                    if (_automatically_remove_complete)
                        RemoveTrack(track.Key);
                }
            }
        }

        public AnimationMix Clear()
        {
            _tracks.Clear();

            return this;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AnimationMix)obj);
        }

        public bool Equals(AnimationMix p)
        {
            return _tracks.Equals(p._tracks) &&
                _automatically_remove_complete == p._automatically_remove_complete;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _tracks.GetHashCode();
                hash = hash * 23 + _automatically_remove_complete.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return "AnimationMix: [ Count: " + _tracks.Count + " ]";
        }

    }
}
