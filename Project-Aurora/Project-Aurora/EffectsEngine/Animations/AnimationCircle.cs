using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationCircle : AnimationFrame
    {
        [Newtonsoft.Json.JsonProperty]
        internal float _radius = 0.0f;

        public float Radius { get { return _radius; } }
        public PointF Center { get { return _center; } }


        public AnimationFrame SetRadius(float radius)
        {
            _radius = radius;
            _dimension = new RectangleF(_center.X - _radius, _center.Y - _radius, 2.0f * _radius, 2.0f * _radius);
            _center = new PointF(_dimension.X + _radius, _dimension.Y + _radius);
            _invalidated = true;

            return this;
        }

        public AnimationFrame SetCenter(PointF center)
        {
            _center = center;
            _dimension = new RectangleF(_center.X - _radius, _center.Y - _radius, 2.0f * _radius, 2.0f * _radius);
            _invalidated = true;

            return this;
        }

        public AnimationCircle()
        {
            _radius = 0;
            _center = new PointF(0, 0);
            _dimension = new RectangleF(_center.X - _radius, _center.Y - _radius, 2.0f * _radius, 2.0f * _radius);
            _color = Utils.ColorUtils.GenerateRandomColor();
            _width = 1;
            _duration = 0.0f;
        }

        public AnimationCircle(AnimationFrame frame, float radius) : base(frame)
        {
            _radius = radius;
            _center = new PointF(_dimension.X + _radius, _dimension.Y + _radius);
        }

        public AnimationCircle(AnimationCircle animationCircle) : base(animationCircle)
        {
            _radius = animationCircle.Radius;
            _center = animationCircle.Center;
        }

        public AnimationCircle(Rectangle dimension, Color color, int width = 1, float duration = 0.0f) : base(dimension, color, width, duration)
        {
            _radius = dimension.Width / 2.0f;
            _center = new PointF(dimension.X + _radius, dimension.Y + _radius);
        }

        public AnimationCircle(RectangleF dimension, Color color, int width = 1, float duration = 0.0f) : base(dimension, color, width, duration)
        {
            _radius = dimension.Width / 2.0f;
            _center = new PointF(dimension.X + _radius, dimension.Y + _radius);
        }

        public AnimationCircle(PointF center, float radius, Color color, int width = 1, float duration = 0.0f)
        {
            _radius = radius;
            _center = center;
            _dimension = new RectangleF(_center.X - _radius, _center.Y - _radius, 2.0f * _radius, 2.0f * _radius);
            _color = color;
            _width = width;
            _duration = duration;
        }

        public AnimationCircle(float x, float y, float radius, Color color, int width = 1, float duration = 0.0f)
        {
            _radius = radius;
            _center = new PointF(x, y);
            _dimension = new RectangleF(_center.X - _radius, _center.Y - _radius, 2.0f * _radius, 2.0f * _radius);
            _color = color;
            _width = width;
            _duration = duration;
        }

        public override void Draw(Graphics g)
        {
            if (_pen == null || _invalidated)
            {
                _pen = new Pen(_color);
                _pen.Width = _width;
                _pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;
                _pen.ScaleTransform(Scale, Scale);

                virtUpdate();
                _invalidated = false;
            }

            if(_scaledDimension.Width > 1 && _scaledDimension.Height > 1)
            {
                g.ResetTransform();
                g.DrawEllipse(_pen, _scaledDimension);
            }
        }

        public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            if (!(otherAnim is AnimationCircle))
            {
                throw new FormatException("Cannot blend with another type");
            }
            AnimationCircle otherCircle = (AnimationCircle)otherAnim;

            amount = GetTransitionValue(amount);

            AnimationFrame newFrame = base.BlendWith(otherCircle, amount);

            float newRadius = (float)CalculateNewValue(_radius, otherCircle._radius, amount);

            return new AnimationCircle(newFrame, newRadius);
        }

        public override AnimationFrame GetCopy()
        {
            RectangleF newrect = new RectangleF(_dimension.X,
                _dimension.Y,
                _dimension.Width,
                _dimension.Height
                );

            return new AnimationCircle(newrect, Color.FromArgb(_color.A, _color.R, _color.G, _color.B), _width, _duration).SetAngle(_angle).SetTransitionType(_transitionType);
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
            return $"AnimationCircle [ Color: {_color.ToString()} Dimensions: {_dimension.ToString()} Width: {_width} Duration: {_duration} Angle: {_angle} ]";
        }
    }
}
