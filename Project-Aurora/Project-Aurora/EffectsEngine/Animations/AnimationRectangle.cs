using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationRectangle : AnimationFrame
    {
        public AnimationRectangle()
        {
            _dimension = new Rectangle((int)(0), (int)(0), (int)0, (int)0);
            _color = Utils.ColorUtils.GenerateRandomColor();
            _width = 1;
            _duration = 0.0f;
        }

        public AnimationRectangle(RectangleF dimension, Color color, int width = 1, float duration = 0.0f) : base(dimension, color, width, duration)
        {
        }

        public AnimationRectangle(PointF center, float rect_width, float rect_height, Color color, int width = 1, float duration = 0.0f)
        {
            _dimension = new RectangleF(center.X - rect_width * 0.5f, center.Y - rect_height * 0.5f, rect_width, rect_height);
            _color = color;
            _width = width;
            _duration = duration;
        }

        public AnimationRectangle(float x, float y, float rect_width, float rect_height, Color color, int width = 1, float duration = 0.0f)
        {
            _dimension = new RectangleF(x, y, rect_width, rect_height);
            _color = color;
            _width = width;
            _duration = duration;
        }

        public override void Draw(Graphics g, float scale = 1.0f, PointF offset = default(PointF))
        {
            if (_pen == null || _invalidated)
            {
                _pen = new Pen(_color);
                _pen.Width = _width;
                _pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;

                _invalidated = false;
            }

            _pen.ScaleTransform(scale, scale);
            RectangleF _scaledDimension = new RectangleF(_dimension.X * scale, _dimension.Y * scale, _dimension.Width * scale, _dimension.Height * scale);
            _scaledDimension.Offset(offset);

            PointF rotatePoint = new PointF(_scaledDimension.X, _scaledDimension.Y);

            Matrix transformationMatrix = new Matrix();
            transformationMatrix.RotateAt(-_angle, rotatePoint, MatrixOrder.Append);
            transformationMatrix.Translate(-_scaledDimension.Width / 2f, -_scaledDimension.Height / 2f);

            Matrix originalMatrix = g.Transform;
            g.Transform = transformationMatrix;
            g.DrawRectangle(_pen, _scaledDimension.X, _scaledDimension.Y, _scaledDimension.Width, _scaledDimension.Height);
            g.Transform = originalMatrix;

        }

        public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            if (!(otherAnim is AnimationRectangle))
            {
                throw new FormatException("Cannot blend with another type");
            }

            amount = GetTransitionValue(amount);

            RectangleF newrect = new RectangleF((float)CalculateNewValue(_dimension.X, otherAnim._dimension.X, amount),
                (float)CalculateNewValue(_dimension.Y, otherAnim._dimension.Y, amount),
                (float)CalculateNewValue(_dimension.Width, otherAnim._dimension.Width, amount),
                (float)CalculateNewValue(_dimension.Height, otherAnim._dimension.Height, amount)
                );

            int newwidth = (int)CalculateNewValue(_width, otherAnim._width, amount);
            float newAngle = (float)CalculateNewValue(_angle, otherAnim._angle, amount);

            return new AnimationRectangle(newrect, Utils.ColorUtils.BlendColors(_color, otherAnim._color, amount), newwidth).SetAngle(newAngle);
        }

        public override AnimationFrame GetCopy()
        {
            RectangleF newrect = new RectangleF(_dimension.X,
                _dimension.Y,
                _dimension.Width,
                _dimension.Height
                );

            return new AnimationRectangle(newrect, Color.FromArgb(_color.A, _color.R, _color.G, _color.B), _width, _duration).SetAngle(_angle).SetTransitionType(_transitionType);
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
