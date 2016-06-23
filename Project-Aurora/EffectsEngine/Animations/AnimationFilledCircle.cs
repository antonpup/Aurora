using System;
using System.Drawing;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationFilledCircle : AnimationFrame
    {

        public AnimationFilledCircle(Rectangle dimension, Color color) : base(dimension, color)
        {
        }

        public AnimationFilledCircle(RectangleF dimension, Color color) : base(dimension, color)
        {
        }

        public AnimationFilledCircle(PointF center, float radius, Color color, int width = 1)
        {
            _dimension = new RectangleF(center.X - radius, center.Y - radius, 2.0f * radius, 2.0f * radius);
            _color = color;
            _width = width;
        }

        public AnimationFilledCircle(float x, float y, float radius, Color color, int width = 1)
        {
            _dimension = new RectangleF(x - radius, y - radius, 2.0f * radius, 2.0f * radius);
            _color = color;
            _width = width;
        }

        public override void Draw(Graphics g)
        {
            _brush = new SolidBrush(_color);

            g.FillEllipse(_brush, _dimension);

            _brush.Dispose();
        }

        public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            if (!(otherAnim is AnimationFilledCircle))
            {
                throw new FormatException("Cannot blend with another type");
            }

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
                _width.Equals(p._width);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _color.GetHashCode();
                hash = hash * 23 + _dimension.GetHashCode();
                hash = hash * 23 + _width.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return "AnimationFilledCircle [ Color: " + _color.ToString() + " Dimensions: " + _dimension.ToString() + "]";
        }
    }
}
