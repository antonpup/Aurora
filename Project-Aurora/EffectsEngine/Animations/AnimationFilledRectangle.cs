using System;
using System.Drawing;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationFilledRectangle : AnimationFrame
    {
        Rectangle _dimension_int;

        public AnimationFilledRectangle(Rectangle dimension, Color color, float duration = 0.0f) : base(dimension, color, 1, duration)
        {
            _dimension_int = dimension;
        }

        public AnimationFilledRectangle(PointF center, float rect_width, float rect_height, Color color, float duration = 0.0f)
        {
            _dimension_int = new Rectangle((int)(center.X - rect_width * 0.5f), (int)(center.Y - rect_height * 0.5f), (int)rect_width, (int)rect_height);
            _color = color;
            _duration = duration;
        }

        public AnimationFilledRectangle(float x, float y, float rect_width, float rect_height, Color color, float duration = 0.0f)
        {
            _dimension_int = new Rectangle((int)(x - rect_width * 0.5f), (int)(y - rect_height * 0.5f), (int)rect_width, (int)rect_height);
            _color = color;
            _duration = duration;
        }

        public override void Draw(Graphics g)
        {
            if (_brush == null)
            {
                _brush = new SolidBrush(_color);
            }

            g.FillRectangle(_brush, _dimension_int);
        }

        public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            if (!(otherAnim is AnimationFilledRectangle))
            {
                throw new FormatException("Cannot blend with another type");
            }

            Rectangle newrect = new Rectangle((int)(_dimension_int.X * (1.0 - amount) + (otherAnim as AnimationFilledRectangle)._dimension_int.X * (amount)),
                (int)(_dimension_int.Y * (1.0 - amount) + (otherAnim as AnimationFilledRectangle)._dimension_int.Y * (amount)),
                (int)(_dimension_int.Width * (1.0 - amount) + (otherAnim as AnimationFilledRectangle)._dimension_int.Width * (amount)),
                (int)(_dimension_int.Height * (1.0 - amount) + (otherAnim as AnimationFilledRectangle)._dimension_int.Height * (amount))
                );

            return new AnimationFilledRectangle(newrect, Utils.ColorUtils.BlendColors(_color, otherAnim._color, amount));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AnimationFilledRectangle)obj);
        }

        public bool Equals(AnimationFilledRectangle p)
        {
            return _color.Equals(p._color) &&
                _dimension_int.Equals(p._dimension_int) &&
                _width.Equals(p._width) &&
                _duration.Equals(p._duration);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _color.GetHashCode();
                hash = hash * 23 + _dimension_int.GetHashCode();
                hash = hash * 23 + _width.GetHashCode();
                hash = hash * 23 + _duration.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return "AnimationFilledRectangle [ Color: " + _color.ToString() + " Dimensions: " + _dimension_int.ToString() + "]";
        }
    }
}
