using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationRectangle : AnimationFrame
    {
        internal Rectangle _dimension_int;

        public Rectangle Dimension_int { get { return _dimension_int; } }

        public AnimationFrame SetDimensionInt(Rectangle dimension_int)
        {
            _dimension_int = dimension_int;
            _invalidated = true;

            return this;
        }

        public AnimationRectangle()
        {
            _dimension_int = new Rectangle((int)(0), (int)(0), (int)0, (int)0);
            _color = Utils.ColorUtils.GenerateRandomColor();
            _width = 1;
            _duration = 0.0f;
        }

        public AnimationRectangle(Rectangle dimension, Color color, int width = 1, float duration = 0.0f) : base(dimension, color, width, duration)
        {
            _dimension_int = dimension;
        }

        public AnimationRectangle(PointF center, float rect_width, float rect_height, Color color, int width = 1, float duration = 0.0f)
        {
            _dimension_int = new Rectangle((int)(center.X - rect_width * 0.5f), (int)(center.Y - rect_height * 0.5f), (int)rect_width, (int)rect_height);
            _color = color;
            _width = width;
            _duration = duration;
        }

        public AnimationRectangle(float x, float y, float rect_width, float rect_height, Color color, int width = 1, float duration = 0.0f)
        {
            _dimension_int = new Rectangle((int)(x - rect_width * 0.5f), (int)(y - rect_height * 0.5f), (int)rect_width, (int)rect_height);
            _color = color;
            _width = width;
            _duration = duration;
        }

        public override void Draw(Graphics g, float scale = 1.0f)
        {
            if (_pen == null || _invalidated)
            {
                _pen = new Pen(_color);
                _pen.Width = _width;
                _pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;

                _invalidated = false;
            }

            _pen.ScaleTransform(scale, scale);
            Rectangle _scaledDimension = new Rectangle((int)(_dimension_int.X * scale), (int)(_dimension_int.Y * scale), (int)(_dimension_int.Width * scale), (int)(_dimension_int.Height * scale));

            PointF rotatePoint = new PointF(_scaledDimension.X + (_scaledDimension.Width / 2.0f), _scaledDimension.Y + (_scaledDimension.Height / 2.0f));

            Matrix rotationMatrix = new Matrix();
            rotationMatrix.RotateAt(-_angle, rotatePoint, MatrixOrder.Append);

            Matrix originalMatrix = g.Transform;
            g.Transform = rotationMatrix;
            g.DrawRectangle(_pen, _scaledDimension);
            g.Transform = originalMatrix;

        }

        public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            if (!(otherAnim is AnimationRectangle))
            {
                throw new FormatException("Cannot blend with another type");
            }

            amount = GetTransitionValue(amount);

            Rectangle newrect = new Rectangle((int)CalculateNewValue(_dimension_int.X, (otherAnim as AnimationRectangle)._dimension_int.X, amount),
                (int)CalculateNewValue(_dimension_int.Y, (otherAnim as AnimationRectangle)._dimension_int.Y, amount),
                (int)CalculateNewValue(_dimension_int.Width, (otherAnim as AnimationRectangle)._dimension_int.Width, amount),
                (int)CalculateNewValue(_dimension_int.Height, (otherAnim as AnimationRectangle)._dimension_int.Height, amount)
                );

            int newwidth = (int)CalculateNewValue(_width, otherAnim._width, amount);
            float newAngle = (float)CalculateNewValue(_angle, otherAnim._angle, amount);

            return new AnimationRectangle(newrect, Utils.ColorUtils.BlendColors(_color, otherAnim._color, amount), newwidth).SetAngle(newAngle);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AnimationRectangle)obj);
        }

        public bool Equals(AnimationRectangle p)
        {
            return _color.Equals(p._color) &&
                _dimension_int.Equals(p._dimension_int) &&
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
                hash = hash * 23 + _dimension_int.GetHashCode();
                hash = hash * 23 + _width.GetHashCode();
                hash = hash * 23 + _duration.GetHashCode();
                hash = hash * 23 + _angle.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"AnimationRectangle [ Color: {_color.ToString()} Dimensions: {_dimension_int.ToString()} Width: {_width} Duration: {_duration} Angle: {_angle} ]";
        }
    }
}
