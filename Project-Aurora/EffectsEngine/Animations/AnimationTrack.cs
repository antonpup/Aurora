using System.Collections.Generic;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationTrack
    {
        private Dictionary<float, AnimationFrame> _animations;
        private float _animationDuration;
        private string _track_name;
        private float _shift;

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
            if (time > _animationDuration)
                _animationDuration = time;

            _animations[time] = animframe;

            return this;
        }

        public AnimationFrame GetFrame(float time)
        {
            if(!ContainsAnimationAt(time))
                return new AnimationFrame();

            time = NormalizeTime(time);

            if (time > _animationDuration || _animations.Count == 0)
                return new AnimationFrame();

            float closest_lower = 0.0f;
            float closest_higher = _animationDuration;

            foreach (KeyValuePair<float, AnimationFrame> kvp in _animations)
            {
                if (kvp.Key == time)
                    return kvp.Value;

                if (kvp.Key > time && kvp.Key < closest_higher)
                    closest_higher = kvp.Key;

                if (kvp.Key < time && kvp.Key > closest_lower)
                    closest_lower = kvp.Key;
            }

            return _animations[closest_lower].BlendWith(_animations[closest_higher], ((double)(time - closest_lower) / (double)(closest_higher - closest_lower)));
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
