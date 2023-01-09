using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationFilledRectangle : AnimationRectangle
    {
        public AnimationFilledRectangle()
        {
        }

        public AnimationFilledRectangle(AnimationFrame animationFrame) : base(animationFrame)
        {
        }

        public AnimationFilledRectangle(RectangleF dimension, Color color, float duration = 0.0f) : base(dimension, color, 1, duration)
        {
        }

        public AnimationFilledRectangle(float x, float y, float rect_width, float rect_height, Color color, float duration = 0.0f) : base(x, y, rect_width, rect_height, color, 1, duration)
        {
        }

        public override void Draw(Graphics g)
        {
            if (_invalidated)
            {
                _brush = new SolidBrush(_color);

                VirtUpdate();
                _invalidated = false;
            }

            g.ResetTransform();
            g.Transform = _transformationMatrix;
            float drawX = _dimension.X - _dimension.Width/2;
            float drawY = _dimension.Y - _dimension.Height/2;
            g.FillRectangle(_brush, drawX, drawY, _dimension.Width, _dimension.Height);
        }

        public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            if (!(otherAnim is AnimationFilledRectangle))
            {
                throw new FormatException("Cannot blend with another type");
            }
            AnimationFilledRectangle otherCircle = (AnimationFilledRectangle)otherAnim;

            amount = GetTransitionValue(amount);

            AnimationFrame newFrame = base.BlendWith(otherCircle, amount);

            return new AnimationFilledRectangle(newFrame);
        }

        public override AnimationFrame GetCopy()
        {
            return new AnimationFilledRectangle(this);
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
            return $"AnimationFilledRectangle [ Color: {_color.ToString()} Dimensions: {_dimension.ToString()} Duration: {_duration} Angle: {_angle} ]";
        }
    }
}
