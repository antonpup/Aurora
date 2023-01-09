using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationTrack
    {
        [JsonProperty]
        private readonly ConcurrentDictionary<float, AnimationFrame> _animations;
        [JsonProperty]
        private float _animationDuration;
        [JsonProperty]
        private string _track_name;
        [JsonProperty]
        private float _shift;

        private bool _SupportedTypeIdentified;
        private Type _SupportedAnimationType = typeof(AnimationFrame);

        [JsonIgnore]
        private readonly Dictionary<int, AnimationFrame> _blendCache = new();
        
        public Type SupportedAnimationType
        {
            get
            {
                if (!_SupportedTypeIdentified)
                {
                    if (_animations.Count > 0)
                    {
                        _SupportedAnimationType = _animations.Values.ToArray()[0].GetType();
                        _SupportedTypeIdentified = true;
                    }
                }

                return _SupportedAnimationType;
            }
        }

        public float AnimationDuration => _animationDuration;

        public AnimationTrack(string trackName, float animationDuration, float shift = 0.0f)
        {
            _animations = new ConcurrentDictionary<float, AnimationFrame>();
            _track_name = trackName;
            _animationDuration = animationDuration;
            _shift = shift;
        }

        public void SetScale(float scale)
        {
            foreach(var frame in _animations.Values)
            {
                frame.Scale = scale;
            }
            _blendCache.Clear();
        }

        public AnimationTrack SetName(string name)
        {
            _track_name = name;

            return this;
        }

        public string GetName()
        {
            return _track_name;
        }

        public AnimationTrack SetShift(float shift)
        {
            _shift = shift;

            return this;
        }

        public float GetShift()
        {
            return _shift;
        }

        private float NormalizeTime(float time)
        {
            //Shift
            return time - _shift;
        }

        public bool ContainsAnimationAt(float time)
        {
            time = NormalizeTime(time);

            return time <= _animationDuration && !_animations.IsEmpty;
        }

        public AnimationTrack SetFrame(float time, AnimationFrame animframe)
        {
            //One can retype the animation track by removing all frames
            if (_animations.IsEmpty)
            {
                _SupportedAnimationType = animframe.GetType();
                _SupportedTypeIdentified = true;
            }

            if (_SupportedAnimationType == animframe.GetType())
            {
                if (!_animations.IsEmpty)
                {
                    Tuple<float, float> closeValues = GetCloseValues(time);

                    if (closeValues.Item1 + _animations[closeValues.Item1]._duration > time && time > closeValues.Item1)
                        _animations[closeValues.Item1].SetDuration(time - closeValues.Item1);
                }

                _animations[time] = animframe;
            }

            UpdateDuration();

            return this;
        }

        public AnimationTrack RemoveFrame(float time)
        {
            foreach (KeyValuePair<float, AnimationFrame> kvp in _animations)
            {
                if (kvp.Key == time)
                {
                    _animations.TryRemove(kvp.Key, out _);
                    break;
                }
            }

            UpdateDuration();
            return this;
        }

        public AnimationTrack Clear()
        {
            _animations.Clear();
            _blendCache.Clear();

            UpdateDuration();
            return this;
        }
        public AnimationFrame GetFrame(float time)
        {
            if (!ContainsAnimationAt(time))
                return (AnimationFrame)Activator.CreateInstance(SupportedAnimationType);

            time = NormalizeTime(time);

            if (time > _animationDuration || _animations.Count == 0)
                return (AnimationFrame)Activator.CreateInstance(SupportedAnimationType);

            Tuple<float, float> closeValues = GetCloseValues(time);

            if (!_animations.ContainsKey(closeValues.Item1) || closeValues.Item1 > time)
                return new AnimationFrame();

            //The time value is exact
            if (Math.Abs(closeValues.Item1 - closeValues.Item2) < 0.1)
                return _animations[closeValues.Item1];
            if (closeValues.Item1 + _animations[closeValues.Item1]._duration > time)
                return _animations[closeValues.Item1];
            if (_animations.ContainsKey(closeValues.Item1) && _animations.ContainsKey(closeValues.Item2))
            {
                int roundedTime = (int)Math.Round(time * 100);
                AnimationFrame blend;
                if (_blendCache.TryGetValue(roundedTime, out blend))
                {
                    return blend;
                }
                var blendAmount = (time - (closeValues.Item1 + _animations[closeValues.Item1]._duration)) /
                                  (double)(closeValues.Item2 - (closeValues.Item1 + _animations[closeValues.Item1]._duration));
                blend = _animations[closeValues.Item1].BlendWith(_animations[closeValues.Item2], blendAmount);
                _blendCache.Add(roundedTime, blend);
                return blend;
            }

            return (AnimationFrame)Activator.CreateInstance(SupportedAnimationType);
        }

        public ConcurrentDictionary<float, AnimationFrame> GetAnimations()
        {
            return _animations;
        }

        private Tuple<float, float> GetCloseValues(float time)
        {
            float closest_lower = _animations.Keys.Min();
            float closest_higher = _animationDuration;

            foreach (KeyValuePair<float, AnimationFrame> kvp in _animations)
            {
                if (kvp.Key == time)
                    return new Tuple<float, float>(time, time);

                if (kvp.Key > time && kvp.Key < closest_higher)
                    closest_higher = kvp.Key;

                if (kvp.Key < time && kvp.Key > closest_lower)
                    closest_lower = kvp.Key;
            }

            return new Tuple<float, float>(closest_lower, closest_higher);
        }

        private void UpdateDuration()
        {
            _blendCache.Clear();
            if (_animations.Count > 0)
            {
                float max = _animations.Keys.Max();
                _animationDuration = max + _animations[max].Duration;
            }
            else
                _animationDuration = 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AnimationTrack)obj);
        }

        public bool Equals(AnimationTrack p)
        {
            return _track_name.Equals(p._track_name) &&
                _animationDuration == p._animationDuration &&
                _shift == p._shift &&
                _animations.Equals(p._animations);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _track_name.GetHashCode();
                hash = hash * 23 + _animationDuration.GetHashCode();
                hash = hash * 23 + _shift.GetHashCode();
                hash = hash * 23 + _animations.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return "AnimationTrack: " + _track_name + " { Frames: " + _animations.Count + " Duration: " + _animationDuration + " sec. Shift: " + _shift + " sec. }";
        }
    }
}
