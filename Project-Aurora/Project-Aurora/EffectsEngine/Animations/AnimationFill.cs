using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationFill : AnimationFilledRectangle
    {
        public AnimationFill() : base()
        {
        }

        public AnimationFill(Color color, float duration = 0.0f) : base(new Rectangle(), color, duration)
        {
        }

        public override void Draw(Graphics g, float scale = 1.0f, PointF offset = default(PointF))
        {
            // Offset has no effect on this. I think.
            if (_brush == null || _invalidated)
            {
                _brush = new SolidBrush(_color);
                _invalidated = false;
            }
            
            g.FillRectangle(_brush, g.VisibleClipBounds);
        }

        public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            if (!(otherAnim is AnimationFill))
            {
                throw new FormatException("Cannot blend with another type");
            }

            if (_transitionType == AnimationFrameTransitionType.None)
                return new AnimationFill(Color.Transparent);

            amount = GetTransitionValue(amount);

            return new AnimationFill(Utils.ColorUtils.BlendColors(_color, otherAnim._color, amount));
        }

        public override AnimationFrame GetCopy()
        {
            return new AnimationFill(Color.FromArgb(_color.A, _color.R, _color.G, _color.B), _duration).SetTransitionType(_transitionType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AnimationFill)obj);
        }

        public bool Equals(AnimationFill p)
        {
            return _color.Equals(p._color) &&
                _duration.Equals(p._duration);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _color.GetHashCode();
                hash = hash * 23 + _duration.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"AnimationFill [ Color: {_color.ToString()} Duration: {_duration} ]";
        }
    }
}
