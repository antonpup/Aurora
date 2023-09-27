using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Common.Utils;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationRectangle : AnimationFrame
    {
        public AnimationRectangle()
        {
            _dimension = new RectangleF(25, 10, 50, 20);
            _color = CommonColorUtils.GenerateRandomColor();
            _width = 1;
            _duration = 2.0f;
        }

        public AnimationRectangle(AnimationFrame animationFrame) : base(animationFrame)
        {
        }

        public AnimationRectangle(RectangleF dimension, Color color, int width = 1, float duration = 0.0f)
        {
            _dimension = dimension;
            _color = color;
            _width = width;
            _duration = duration;
        }

        public AnimationRectangle(float x, float y, float rectWidth, float rectHeight, Color color, int width = 1, float duration = 0.0f)
        {
            _dimension = new RectangleF(x, y, rectWidth, rectHeight);
            _color = color;
            _width = width;
            _duration = duration;
        }

        public override void Draw(Graphics g)
        {
            if (_invalidated)
            {
                _pen = new Pen(_color);
                _pen.Width = _width;
                _pen.Alignment = PenAlignment.Inset;

                VirtUpdate();
                _invalidated = false;
            }
            
            g.ResetTransform();
            g.Transform = _transformationMatrix;
            float drawX = _dimension.X - _dimension.Width/2;
            float drawY = _dimension.Y - _dimension.Height/2;
            g.DrawRectangle(_pen, drawX, drawY, _dimension.Width, _dimension.Height);
        }

        public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            if (!(otherAnim is AnimationRectangle))
            {
                throw new FormatException("Cannot blend with another type");
            }
            AnimationRectangle otherRectangle = (AnimationRectangle)otherAnim;

            amount = GetTransitionValue(amount);

            AnimationFrame newFrame = base.BlendWith(otherRectangle, amount);

            return new AnimationRectangle(newFrame);
        }

        public override AnimationFrame GetCopy()
        {
            return new AnimationRectangle(this);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((AnimationRectangle)obj);
        }

        public bool Equals(AnimationRectangle p)
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
            return $"AnimationRectangle [ Color: {_color.ToString()} Dimensions: {_dimension.ToString()} Width: {_width} Duration: {_duration} Angle: {_angle} ]";
        }
    }
}
