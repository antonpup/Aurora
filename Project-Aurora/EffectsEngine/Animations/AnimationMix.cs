using System.Collections.Generic;
using System.Drawing;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationMix
    {
        private Dictionary<string, AnimationTrack> _tracks;

        /// <summary>
        /// When true, will remove Animation tracks that no longer have any animations.
        /// </summary>
        private bool _automatically_remove_complete;

        public AnimationMix()
        {
            _tracks = new Dictionary<string, AnimationTrack>();
            _automatically_remove_complete = false;
        }

        public AnimationMix SetAutoRemove(bool value)
        {
            _automatically_remove_complete = value;

            return this;
        }

        public AnimationMix AddTrack(AnimationTrack track)
        {
            if (_tracks.ContainsKey(track.GetName()))
                _tracks[track.GetName()] = track;
            else
                _tracks.Add(track.GetName(), track);

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

        public void Draw(Graphics g, float time)
        {
            Dictionary<string, AnimationTrack> _local = new Dictionary<string, AnimationTrack>(_tracks);

            foreach (KeyValuePair<string, AnimationTrack> track in _local)
            {
                if (track.Value.ContainsAnimationAt(time))
                    track.Value.GetFrame(time).Draw(g);
                else
                {
                    if (_automatically_remove_complete)
                        RemoveTrack(track.Key);
                }
            }
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
