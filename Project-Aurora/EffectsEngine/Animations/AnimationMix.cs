using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationMix
    {
        private Dictionary<string, AnimationTrack> _tracks;

        public AnimationMix()
        {
            _tracks = new Dictionary<string, AnimationTrack>();
        }

        public void AddTrack(AnimationTrack track)
        {
            if (_tracks.ContainsKey(track.GetName()))
                _tracks[track.GetName()] = track;
            else
                _tracks.Add(track.GetName(), track);
        }

        public void AddTrack(string track_name, AnimationTrack track)
        {
            if (_tracks.ContainsKey(track_name))
                _tracks[track_name] = track;
            else
                _tracks.Add(track_name, track);
        }

        public void RemoveTrack(string track_name)
        {
            if (_tracks.ContainsKey(track_name))
                _tracks.Remove(track_name);
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
            foreach(KeyValuePair<string, AnimationTrack> track in _tracks)
            {
                track.Value.GetFrame(time).Draw(g);
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
            return _tracks.Equals(p._tracks);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _tracks.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return "AnimationMix: [ Count: " + _tracks.Count + " ]";
        }

    }
}
