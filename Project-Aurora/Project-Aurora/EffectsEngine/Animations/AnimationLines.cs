using System;
using System.Collections.Generic;
using System.Drawing;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationLines : AnimationFrame
    {
        [Newtonsoft.Json.JsonProperty]
        private List<AnimationLine> _lines;

        public AnimationLines(AnimationLine[] lines, float duration = 0.0f)
        {
            _lines = new List<AnimationLine>(lines);
            _duration = duration;
        }

        public override void Draw(Graphics g, float scale = 1.0f, PointF offset = default(PointF))
        {
            foreach( AnimationLine line in _lines)
                line.Draw(g, scale, offset);
        }

        public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            if (!(otherAnim is AnimationLines))
            {
                throw new FormatException("Cannot blend with another type");
            }

            if(this._lines.Count != (otherAnim as AnimationLines)._lines.Count)
            {
                throw new NotImplementedException();
            }

            amount = GetTransitionValue(amount);

            List<AnimationLine> newlines = new List<AnimationLine>();

            for (int line_i = 0; line_i < this._lines.Count; line_i++)
                newlines.Add(this._lines[line_i].BlendWith((otherAnim as AnimationLines)._lines[line_i], amount) as AnimationLine);

            return new AnimationLines(newlines.ToArray());
        }

        public override AnimationFrame GetCopy()
        {
            List<AnimationLine> newlines = new List<AnimationLine>(_lines.Count);

            foreach(var line in _lines)
            {
                newlines.Add(line.GetCopy() as AnimationLine);
            }

            return new AnimationLines(newlines.ToArray(), _duration).SetAngle(_angle).SetTransitionType(_transitionType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AnimationLines)obj);
        }

        public bool Equals(AnimationLines p)
        {
            return _lines.Equals(p._lines) &&
                _duration.Equals(p._duration);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _lines.GetHashCode();
                hash = hash * 23 + _duration.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return "AnimationLines [ Lines: " + _lines.Count + " ]";
        }
    }
}
