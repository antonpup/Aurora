using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Aurora.Utils;
using Newtonsoft.Json;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationMix
    {
        [Newtonsoft.Json.JsonProperty]
        [JsonConverter(typeof(ConcurrentDictionaryJsonConverterAdapter<string, AnimationTrack>))]
        private readonly ConcurrentDictionary<string, AnimationTrack> _tracks = new();

        /// <summary>
        /// When true, will remove Animation tracks that no longer have any animations.
        /// </summary>
        [Newtonsoft.Json.JsonProperty] private bool _automatically_remove_complete;

        public AnimationMix()
        {
        }

        public AnimationMix(AnimationTrack[] tracks)
        {
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
                    _tracks.TryAdd(track.GetName(), track);
            }

            return this;
        }

        public AnimationMix RemoveTrack(string track_name)
        {
            _tracks.TryRemove(track_name, out _);

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
            float current_duration, return_val = 0.0f;

            foreach (KeyValuePair<string, AnimationTrack> track in _tracks)
            {
                current_duration = track.Value.GetShift() + track.Value.AnimationDuration;
                if (current_duration > return_val)
                    return_val = current_duration;
            }

            return return_val;
        }

        public void SetScale(float scale)
        {
            foreach (KeyValuePair<string, AnimationTrack> track in _tracks)
            {
                track.Value.SetScale(scale);
            }
        }

        public ConcurrentDictionary<string, AnimationTrack> GetTracks()
        {
            return _tracks;
        }

        public bool AnyActiveTracksAt(float time)
        {
            foreach (KeyValuePair<string, AnimationTrack> track in _tracks)
            {
                if (track.Value.ContainsAnimationAt(time))
                    return true;
            }

            return false;
        }

        public void Draw(Graphics g, float time, PointF offset = default(PointF))
        {
            foreach (KeyValuePair<string, AnimationTrack> track in _tracks)
            {
                if (track.Value.ContainsAnimationAt(time))
                {
                    AnimationFrame frame = track.Value.GetFrame(time);
                    try
                    {
                        frame.SetOffset(offset);
                        frame.Draw(g);
                    }
                    catch (Exception exc)
                    {
                        System.Console.WriteLine("Animation mix draw error: " + exc.Message);
                    }
                }
                else if (_automatically_remove_complete)
                {
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
            return Equals((AnimationMix) obj);
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