using System;
using System.Drawing;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationCircle : AnimationFrame
    {

        public AnimationCircle(Rectangle dimension, Color color, int width = 1) : base(dimension, color, width)
        {
        }

        public AnimationCircle(RectangleF dimension, Color color, int width = 1) : base(dimension, color, width)
        {
        }

        public AnimationCircle(PointF center, float radius, Color color, int width = 1)
        {
            _dimension = new RectangleF(center.X - radius, center.Y - radius, 2.0f * radius, 2.0f * radius);
            _color = color;
            _width = width;
        }

        public AnimationCircle(float x, float y, float radius, Color color, int width = 1)
        {
            _dimension = new RectangleF(x - radius, y - radius, 2.0f * radius, 2.0f * radius);
            _color = color;
            _width = width;
        }

        public override void Draw(Graphics g)
        {
            Pen pen = new Pen(_color);
            pen.Width = _width;
            pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;

            g.DrawEllipse(pen, _dimension);

            pen.Dispose();
        }

        public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            if(!(otherAnim is AnimationCircle))
            {
                throw new FormatException("Cannot blend with another type");
            }

            RectangleF newrect = new RectangleF((float)(_dimension.X * (1.0 - amount) + otherAnim._dimension.X * (amount)),
                (float)(_dimension.Y * (1.0 - amount) + otherAnim._dimension.Y * (amount)),
                (float)(_dimension.Width * (1.0 - amount) + otherAnim._dimension.Width * (amount)),
                (float)(_dimension.Height * (1.0 - amount) + otherAnim._dimension.Height * (amount))
                );

            int newwidth = (int)((_width * (1.0 - amount)) + (otherAnim._width * (amount)));

            return new AnimationCircle(newrect, Utils.ColorUtils.BlendColors(_color, otherAnim._color, amount), newwidth);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AnimationCircle)obj);
        }

        public bool Equals(AnimationCircle p)
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
            return "AnimationCircle [ Color: " + _color.ToString() + " Dimensions: " + _dimension.ToString() + " Width: " + _width + "]";
        }
    }
}
