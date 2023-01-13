using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Aurora.Utils;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationLine : AnimationFrame
    {
        [Newtonsoft.Json.JsonProperty]
        private PointF _start_point;
        [Newtonsoft.Json.JsonProperty]
        private PointF _end_point;
        [Newtonsoft.Json.JsonProperty]
        private Color _end_color;

        public PointF StartPoint => _start_point;
        public PointF EndPoint => _end_point;
        public Color EndColor => _end_color;

        public AnimationFrame SetStartPoint(PointF startPoint)
        {
            _start_point = startPoint;
            _invalidated = true;

            return this;
        }

        public AnimationFrame SetEndPoint(PointF endPoint)
        {
            _end_point = endPoint;
            _invalidated = true;

            return this;
        }

        public AnimationFrame SetEndColor(Color endColor)
        {
            _end_color = endColor;
            _invalidated = true;

            return this;
        }

        public AnimationLine()
        {
            _start_point = new PointF(0, 0);
            _end_point = new PointF(30, 30);
            _color = ColorUtils.GenerateRandomColor();
            _end_color = ColorUtils.GenerateRandomColor();
            _width = 1;
            _duration = 1.0f;
        }

        public AnimationLine(PointF start_point, PointF end_point, Color color, int width = 1, float duration = 0.0f)
        {
            _start_point = start_point;
            _end_point = end_point;
            _color = color;
            _end_color = color;
            _width = width;
            _duration = duration;
        }

        public AnimationLine(Point start_point, Point end_point, Color color, int width = 1, float duration = 0.0f)
        {
            _start_point = start_point;
            _end_point = end_point;
            _color = color;
            _end_color = color;
            _width = width;
            _duration = duration;
        }

        public AnimationLine(float start_x, float start_y, float end_x, float end_y, Color color, int width = 1, float duration = 0.0f)
        {
            _start_point = new PointF(start_x, start_y);
            _end_point = new PointF(end_x, end_y); ;
            _color = color;
            _end_color = color;
            _width = width;
            _duration = duration;
        }

        public AnimationLine(PointF start_point, PointF end_point, Color start_color, Color end_color, int width = 1, float duration = 0.0f)
        {
            _start_point = start_point;
            _end_point = end_point;
            _color = start_color;
            _end_color = end_color;
            _width = width;
            _duration = duration;
        }

        public AnimationLine(Point start_point, Point end_point, Color start_color, Color end_color, int width = 1, float duration = 0.0f)
        {
            _start_point = start_point;
            _end_point = end_point;
            _color = start_color;
            _end_color = end_color;
            _width = width;
            _duration = duration;
        }

        public AnimationLine(float start_x, float start_y, float end_x, float end_y, Color start_color, Color end_color, int width = 1, float duration = 0.0f)
        {
            _start_point = new PointF(start_x, start_y);
            _end_point = new PointF(end_x, end_y); ;
            _color = start_color;
            _end_color = end_color;
            _width = width;
            _duration = duration;
        }

        public override void Draw(Graphics g)
        {
            if (Math.Abs((int)(_start_point.X - _end_point.X)) - Math.Abs((int)(_start_point.Y - _end_point.Y)) == 0 )
                return;

            if (_invalidated)
            {
                _pen = new Pen(new LinearGradientBrush(_start_point, _end_point, _color, _end_color));
                _pen.Width = _width * Scale * (12f / (int)Global.Configuration.BitmapAccuracy);
                _pen.Alignment = PenAlignment.Center;

                VirtUpdate();

                _invalidated = false;
            }

            g.ResetTransform();
            g.Transform = _transformationMatrix;
            g.DrawLine(_pen, _start_point, _end_point);
        }

        public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            if (!(otherAnim is AnimationLine))
            {
                throw new FormatException("Cannot blend with another type");
            }

            amount = GetTransitionValue(amount);

            PointF newstart = new PointF(CalculateNewValue(_start_point.X, (otherAnim as AnimationLine)._start_point.X, amount),
                CalculateNewValue(_start_point.Y, (otherAnim as AnimationLine)._start_point.Y, amount)
                );

            PointF newend = new PointF(CalculateNewValue(_end_point.X, (otherAnim as AnimationLine)._end_point.X, amount),
                CalculateNewValue(_end_point.Y, (otherAnim as AnimationLine)._end_point.Y, amount)
                );

            int newwidth = CalculateNewValue(_width, otherAnim._width, amount);
            float newAngle = CalculateNewValue(_angle, otherAnim._angle, amount);

            return new AnimationLine(newstart, newend, ColorUtils.BlendColors(_color, otherAnim._color, amount), ColorUtils.BlendColors(_end_color, (otherAnim as AnimationLine)._end_color, amount), newwidth).SetAngle(newAngle);
        }

        public override AnimationFrame GetCopy()
        {
            PointF newstart = new PointF(_start_point.X, _start_point.Y);
            PointF newend = new PointF(_end_point.X, _end_point.Y);

            return new AnimationLine(newstart, newend, Color.FromArgb(_color.A, _color.R, _color.G, _color.B), Color.FromArgb(_end_color.A, _end_color.R, _end_color.G, _end_color.B), _width, _duration).SetAngle(_angle).SetTransitionType(_transitionType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AnimationLine)obj);
        }

        public bool Equals(AnimationLine p)
        {
            return _color.Equals(p._color) &&
                _end_color.Equals(p._end_color) &&
                _start_point.Equals(p._start_point) &&
                _end_point.Equals(p._end_point) &&
                _width.Equals(p._width) &&
                _angle.Equals(p._angle);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _color.GetHashCode();
                hash = hash * 23 + _end_color.GetHashCode();
                hash = hash * 23 + _start_point.GetHashCode();
                hash = hash * 23 + _end_point.GetHashCode();
                hash = hash * 23 + _width.GetHashCode();
                hash = hash * 23 + _duration.GetHashCode();
                hash = hash * 23 + _angle.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"AnimationLine [ Start Color: {_color.ToString()} End Color: { _color.ToString()} Start Point: {_start_point.ToString()} End Point: {_end_point.ToString()} Width: {_width} Duration: {_duration} Angle: {_angle} ]";
        }

    }
}
