using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationTrack
    {
        [Newtonsoft.Json.JsonProperty]
        private Dictionary<float, AnimationFrame> _animations;
        [Newtonsoft.Json.JsonProperty]
        private float _animationDuration;
        [Newtonsoft.Json.JsonProperty]
        private string _track_name;
        [Newtonsoft.Json.JsonProperty]
        private float _shift;

        private bool _SupportedTypeIdentified = false;
        private Type _SupportedAnimationType = typeof(AnimationFrame);
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

        public float AnimationDuration { get { return _animationDuration; } }

        public AnimationTrack(string track_name, float animationDuration, float shift = 0.0f)
        {
            _animations = new Dictionary<float, AnimationFrame>();
            _track_name = track_name;
            _animationDuration = animationDuration;
            _shift = shift;
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

            if (time > _animationDuration || _animations.Count == 0)
                return false;
            else
                return true;
        }

        public AnimationTrack SetFrame(float time, AnimationFrame animframe)
        {
            //One can retype the animation track by removing all frames
            if (_animations.Count == 0)
            {
                _SupportedAnimationType = animframe.GetType();
                _SupportedTypeIdentified = true;
            }

            if (_SupportedAnimationType == animframe.GetType())
            {
                if (_animations.Count > 0)
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
                    _animations.Remove(kvp.Key);
                    break;
                }
            }

            UpdateDuration();
            return this;
        }

        public AnimationTrack RemoveFrame(AnimationFrame frame)
        {
            foreach (KeyValuePair<float, AnimationFrame> kvp in _animations)
            {
                if (kvp.Value.Equals(frame))
                {
                    _animations.Remove(kvp.Key);
                    break;
                }
            }

            UpdateDuration();
            return this;
        }

        public AnimationTrack Clear()
        {
            _animations.Clear();

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
            if (closeValues.Item1 == closeValues.Item2)
                return _animations[closeValues.Item1];
            else
            {
                if (closeValues.Item1 + _animations[closeValues.Item1]._duration > time)
                    return _animations[closeValues.Item1];
                else
                {
                    if (_animations.ContainsKey(closeValues.Item1) && _animations.ContainsKey(closeValues.Item2))
                        return _animations[closeValues.Item1].BlendWith(_animations[closeValues.Item2], ((double)(time - (closeValues.Item1 + _animations[closeValues.Item1]._duration)) / (double)(closeValues.Item2 - (closeValues.Item1 + _animations[closeValues.Item1]._duration))));
                    else
                        return (AnimationFrame)Activator.CreateInstance(SupportedAnimationType);
                }
            }
        }

        public Dictionary<float, AnimationFrame> GetAnimations()
        {
            return new Dictionary<float, AnimationFrame>(_animations);
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
