using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationFilledGradientRectangle : AnimationFilledRectangle
    {
        [Newtonsoft.Json.JsonProperty]
        internal EffectBrush _gradientBrush;

        public EffectBrush GradientBrush { get { return _gradientBrush; } }

        public AnimationFilledGradientRectangle() : base()
        {
        }

        public AnimationFilledGradientRectangle(RectangleF dimension, EffectBrush brush, float duration = 0.0f) : base(dimension, Color.Transparent, duration)
        {
            _gradientBrush = brush;
        }

        public AnimationFilledGradientRectangle(PointF center, float rect_width, float rect_height, EffectBrush brush, float duration = 0.0f) : base(center, rect_width, rect_height, Color.Transparent, duration)
        {
            _gradientBrush = brush;
        }

        public AnimationFilledGradientRectangle(float x, float y, float rect_width, float rect_height, EffectBrush brush, float duration = 0.0f) : base(x, y, rect_width, rect_height, Color.Transparent, duration)
        {
            _gradientBrush = brush;
        }

        public override void Draw(Graphics g, float scale = 1.0f, PointF offset = default(PointF))
        {
            RectangleF _scaledDimension = new RectangleF(_dimension.X * scale, _dimension.Y * scale, _dimension.Width * scale, _dimension.Height * scale);
            _scaledDimension.Offset(offset);

            PointF rotatePoint = new PointF(_scaledDimension.X, _scaledDimension.Y);

            EffectBrush _newbrush = new EffectBrush(_gradientBrush);
            _newbrush.start = new PointF(_newbrush.start.X * scale, _newbrush.start.Y * scale);
            _newbrush.end = new PointF(_newbrush.end.X * scale, _newbrush.end.Y * scale);

            Matrix rotationMatrix = new Matrix();
            rotationMatrix.RotateAt(-_angle, rotatePoint, MatrixOrder.Append);
            rotationMatrix.Translate(-_scaledDimension.Width / 2f, -_scaledDimension.Height / 2f);

            Matrix originalMatrix = g.Transform;
            g.Transform = rotationMatrix;
            g.FillRectangle(_newbrush.GetDrawingBrush(), _scaledDimension);
            g.Transform = originalMatrix;
        }

        public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            if (!(otherAnim is AnimationFilledGradientRectangle))
            {
                throw new FormatException("Cannot blend with another type");
            }

            amount = GetTransitionValue(amount);

            RectangleF newrect = new RectangleF((float)CalculateNewValue(_dimension.X, otherAnim._dimension.X, amount),
                (float)CalculateNewValue(_dimension.Y, otherAnim._dimension.Y, amount),
                (float)CalculateNewValue(_dimension.Width, otherAnim._dimension.Width, amount),
                (float)CalculateNewValue(_dimension.Height, otherAnim._dimension.Height, amount)
                );

            float newAngle = (float)CalculateNewValue(_angle, otherAnim._angle, amount);

            return new AnimationFilledGradientRectangle(newrect, _gradientBrush.BlendEffectBrush((otherAnim as AnimationFilledGradientRectangle)._gradientBrush, amount)).SetAngle(newAngle);
        }

        public override AnimationFrame GetCopy()
        {
            RectangleF newrect = new RectangleF(_dimension.X,
                _dimension.Y,
                _dimension.Width,
                _dimension.Height
                );

            return new AnimationFilledGradientRectangle(newrect, new EffectBrush(_gradientBrush), _duration).SetAngle(_angle).SetTransitionType(_transitionType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AnimationFilledGradientRectangle)obj);
        }

        public bool Equals(AnimationFilledGradientRectangle p)
        {
            return _color.Equals(p._color) &&
                _dimension.Equals(p._dimension) &&
                _width.Equals(p._width) &&
                _duration.Equals(p._duration) &&
                _angle.Equals(p._angle) &&
                _gradientBrush.Equals(p._gradientBrush);
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
                hash = hash * 23 + _gradientBrush.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"AnimationFilledGradientRectangle [ Color: {_color.ToString()} Dimensions: {_dimension.ToString()} Duration: {_duration} Angle: {_angle} ]";
        }
    }
}
