using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Aurora.Utils;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationEllipse : AnimationCircle
    {
        [Newtonsoft.Json.JsonProperty]
        private float _radius_x;
        [Newtonsoft.Json.JsonProperty]
        private float _radius_y;

        public AnimationEllipse()
        {
            _radius_x = 0;
            _radius_y = 0;
            _dimension = new RectangleF(- _radius_x, - _radius_y, 2.0f * _radius_x, 2.0f * _radius_y);
            _color = ColorUtils.GenerateRandomColor();
            _width = 1;
            _duration = 0.0f;
        }

        public AnimationEllipse(AnimationFrame frame, float radiusX, float radiusY) : base(frame, radiusX)
        {
            _radius_x = radiusX;
            _radius_y = radiusY;
        }

        public AnimationEllipse(AnimationEllipse animationEllipse) : base(animationEllipse)
        {
            _radius_x = animationEllipse._radius_x;
            _radius_y = animationEllipse._radius_y;
        }

        public AnimationEllipse(Rectangle dimension, Color color, int width = 1, float duration = 0.0f) : base(dimension, color, width, duration)
        {
            _radius_x = dimension.Width / 2.0f;
            _radius_y = dimension.Height / 2.0f;
        }

        public AnimationEllipse(RectangleF dimension, Color color, int width = 1, float duration = 0.0f) : base(dimension, color, width, duration)
        {
            _radius_x = dimension.Width / 2.0f;
            _radius_y = dimension.Height / 2.0f;
        }

        public AnimationEllipse(PointF center, float xAxis, float yAxis, Color color, int width = 1, float duration = 0.0f)
        {
            _radius_x = xAxis;
            _radius_y = yAxis;
            _dimension = new RectangleF(center.X , center.Y, 2.0f * _radius_x, 2.0f * _radius_y);
            _color = color;
            _width = width;
            _duration = duration;
        }

        public AnimationEllipse(float x, float y, float xAxis, float yAxis, Color color, int width = 1, float duration = 0.0f)
        {
            _radius_x = xAxis;
            _radius_y = yAxis;
            _dimension = new RectangleF(x , y, 2.0f * _radius_x, 2.0f * _radius_y);
            _color = color;
            _width = width;
            _duration = duration;
        }

        public override AnimationFrame GetCopy()
        {
            return new AnimationEllipse(this);
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
            return $"AnimationEllipse [ Color: {_color.ToString()} Dimensions: {_dimension.ToString()} Width: {_width} Duration: {_duration} Angle: {_angle} ]";
        }
    }
}
