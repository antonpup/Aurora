using System;
using System.Drawing;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationFilledCircle : AnimationCircle
    {

        public AnimationFilledCircle() : base()
        {
        }

        public AnimationFilledCircle(Rectangle dimension, Color color, float duration = 0.0f) : base(dimension, color, 1, duration)
        {
        }

        public AnimationFilledCircle(RectangleF dimension, Color color, float duration = 0.0f) : base(dimension, color, 1, duration)
        {
        }

        public AnimationFilledCircle(PointF center, float radius, Color color, int width = 1, float duration = 0.0f) : base(center, radius, color, width, duration)
        {
        }

        public AnimationFilledCircle(float x, float y, float radius, Color color, int width = 1, float duration = 0.0f) : base(x, y, radius, color, width, duration)
        {
        }

        public override void Draw(Graphics g, float scale = 1.0f)
        {
            if (_brush == null || _invalidated)
            {
                _brush = new SolidBrush(_color);
                _invalidated = false;
            }

            RectangleF _scaledDimension = new RectangleF(_dimension.X * scale, _dimension.Y * scale, _dimension.Width * scale, _dimension.Height * scale);

            g.FillEllipse(_brush, _scaledDimension);
        }

        public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            if (!(otherAnim is AnimationFilledCircle))
            {
                throw new FormatException("Cannot blend with another type");
            }

            amount = GetTransitionValue(amount);

            RectangleF newrect = new RectangleF((float)(_dimension.X * (1.0 - amount) + otherAnim._dimension.X * (amount)),
                (float)(_dimension.Y * (1.0 - amount) + otherAnim._dimension.Y * (amount)),
                (float)(_dimension.Width * (1.0 - amount) + otherAnim._dimension.Width * (amount)),
                (float)(_dimension.Height * (1.0 - amount) + otherAnim._dimension.Height * (amount))
                );


            return new AnimationFilledCircle(newrect, Utils.ColorUtils.BlendColors(_color, otherAnim._color, amount));
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
                _duration.Equals(p._duration);
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
                return hash;
            }
        }

        public override string ToString()
        {
            return "AnimationFilledCircle [ Color: " + _color.ToString() + " Dimensions: " + _dimension.ToString() + "]";
        }
    }
}
