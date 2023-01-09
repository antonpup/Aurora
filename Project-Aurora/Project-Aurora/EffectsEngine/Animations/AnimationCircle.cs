using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationCircle : AnimationFrame
    {
        [Newtonsoft.Json.JsonProperty]
        internal float _radius;

        public float Radius => _radius;
        public PointF Center => _dimension.Location;

        public AnimationFrame SetRadius(float radius)
        {
            _radius = radius;
            _dimension = new RectangleF(_dimension.X - _radius, _dimension.Y - _radius, 2.0f * _radius, 2.0f * _radius);
            _invalidated = true;

            return this;
        }

        public AnimationFrame SetCenter(PointF center)
        {
            _dimension = new RectangleF(center.X - _radius, center.Y - _radius, 2.0f * _radius, 2.0f * _radius);
            _invalidated = true;

            return this;
        }

        public AnimationCircle()
        {
            _radius = 0;
            _dimension = new RectangleF(- _radius, - _radius, 2.0f * _radius, 2.0f * _radius);
            _color = Utils.ColorUtils.GenerateRandomColor();
            _width = 1;
            _duration = 0.0f;
        }

        public AnimationCircle(AnimationFrame frame, float radius) : base(frame)
        {
            _radius = radius;
        }

        public AnimationCircle(AnimationCircle animationCircle) : base(animationCircle)
        {
            _radius = animationCircle.Radius;
        }

        public AnimationCircle(Rectangle dimension, Color color, int width = 1, float duration = 0.0f) : base(dimension, color, width, duration)
        {
            _radius = dimension.Width / 2.0f;
        }

        public AnimationCircle(RectangleF dimension, Color color, int width = 1, float duration = 0.0f) : base(dimension, color, width, duration)
        {
            _radius = dimension.Width / 2.0f;
        }

        public AnimationCircle(PointF center, float radius, Color color, int width = 1, float duration = 0.0f)
        {
            _radius = radius;
            _dimension = new RectangleF(center.X - _radius, center.Y - _radius, 2.0f * _radius, 2.0f * _radius);
            _color = color;
            _width = width;
            _duration = duration;
        }

        public AnimationCircle(float x, float y, float radius, Color color, int width = 1, float duration = 0.0f)
        {
            _radius = radius;
            _dimension = new RectangleF(x - _radius, y - _radius, 2.0f * _radius, 2.0f * _radius);
            _color = color;
            _width = width;
            _duration = duration;
        }

        public override void Draw(Graphics g)
        {
            if (_invalidated)
            {
                _pen = new Pen(_color);
                _pen.Width = _width * Scale * (12 / (int)Global.Configuration.BitmapAccuracy);
                _pen.Alignment = PenAlignment.Center;
                _pen.ScaleTransform(Scale, Scale);

                VirtUpdate();
                _invalidated = false;
            }

            if(_dimension.Width > 1 && _dimension.Height > 1)
            {
                g.ResetTransform();
                g.Transform = _transformationMatrix;
                g.DrawEllipse(_pen, _dimension);
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

            float newRadius = CalculateNewValue(_radius, otherCircle._radius, amount);

            return new AnimationCircle(newFrame, newRadius);
        }

        public override AnimationFrame GetCopy()
        {
            return new AnimationCircle(this);
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
