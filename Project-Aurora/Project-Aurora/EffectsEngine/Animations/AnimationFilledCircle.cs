using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationFilledCircle : AnimationCircle
    {

        public AnimationFilledCircle() : base()
        {
        }
        public AnimationFilledCircle(AnimationCircle circleFrame) : base(circleFrame)
        {
        }

        public AnimationFilledCircle(PointF center, float radius, Color color, int width = 1, float duration = 0.0f) : base(center, radius, color, width, duration)
        {
        }

        public AnimationFilledCircle(float x, float y, float radius, Color color, int width = 1, float duration = 0.0f) : base(x, y, radius, color, width, duration)
        {
        }

        public override void Draw(Graphics g)
        {
            if (_brush == null || _invalidated)
            {
                _brush = new SolidBrush(_color);
                _pen = new Pen(_color);
                _pen.Width = _width * Scale * (12 / (int)Global.Configuration.BitmapAccuracy);
                _pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;
                _pen.ScaleTransform(Scale, Scale);

                VirtUpdate();
                _invalidated = false;
            }

            g.ResetTransform();
            g.FillEllipse(_brush, _dimension);
        }

        public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            if (!(otherAnim is AnimationFilledCircle))
            {
                throw new FormatException("Cannot blend with another type");
            }
            AnimationFilledCircle otherCircle = (AnimationFilledCircle)otherAnim;

            amount = GetTransitionValue(amount);

            AnimationCircle newCircle = (AnimationCircle) base.BlendWith(otherAnim, amount);

            return new AnimationFilledCircle(newCircle);
        }

        public override AnimationFrame GetCopy()
        {
            return new AnimationFilledCircle(this);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AnimationFilledCircle)obj);
        }

        public bool Equals(AnimationFilledCircle p)
        {
            return _color.Equals(p._color) &&
                _dimension.Equals(p._dimension) &&
                _width.Equals(p._width) &&
                _duration.Equals(p._duration) &&
                _angle.Equals(p._angle);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _color.GetHashCode();
                hash = hash * 23 + _dimension.GetHashCode();
                hash = hash * 23 + _width.GetHashCode();
                hash = hash * 23 + _duration.GetHashCode();
                hash = hash * 23 + _angle.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"AnimationFilledCircle [ Color: {_color.ToString()} Dimensions: {_dimension.ToString()} Duration: {_duration} Angle: {_angle} ]";
        }
    }
}
