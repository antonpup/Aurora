using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationGradientCircle : AnimationCircle
    {
        [Newtonsoft.Json.JsonProperty]
        internal EffectBrush _gradientBrush;

        public EffectBrush GradientBrush { get { return _gradientBrush; } }

        public AnimationGradientCircle() : base()
        {
        }

        public AnimationGradientCircle(Rectangle dimension, EffectBrush brush, int width = 1, float duration = 0.0f) : base(dimension, Color.Transparent, width, duration)
        {
            _gradientBrush = brush;
        }

        public AnimationGradientCircle(RectangleF dimension, EffectBrush brush, int width = 1, float duration = 0.0f) : base(dimension, Color.Transparent, width, duration)
        {
            _gradientBrush = brush;
        }

        public AnimationGradientCircle(PointF center, float radius, EffectBrush brush, int width = 1, float duration = 0.0f) : base(center, radius, Color.Transparent, width, duration)
        {
            _gradientBrush = brush;
        }

        public AnimationGradientCircle(float x, float y, float radius, EffectBrush brush, int width = 1, float duration = 0.0f) : base(x, y, radius, Color.Transparent, width, duration)
        {
            _gradientBrush = brush;
        }

        public override void Draw(Graphics g, float scale = 1.0f, PointF offset = default(PointF))
        {
            RectangleF _scaledDimension = new RectangleF(_dimension.X * scale, _dimension.Y * scale, _dimension.Width * scale, _dimension.Height * scale);
            _scaledDimension.Offset(offset);

            EffectBrush _newbrush = new EffectBrush(_gradientBrush);
            _newbrush.start = new PointF(0.0f, 0.0f);
            _newbrush.end = new PointF(1.0f, 1.0f);
            _newbrush.center = new PointF(0.5f, 0.5f);

            SortedDictionary<float, System.Drawing.Color> newColorGradients = new SortedDictionary<float, System.Drawing.Color>();
            ColorSpectrum spectrum = _newbrush.GetColorSpectrum();
            var colors = _newbrush.colorGradients;

            float _cutOffPoint = _width / _radius;

            if (_cutOffPoint < 1.0f)
            {
                _cutOffPoint = 1.0f - _cutOffPoint;

                foreach (var kvp in spectrum.GetSpectrumColors())
                    newColorGradients.Add((1 - _cutOffPoint) * kvp.Key + _cutOffPoint, kvp.Value);

                newColorGradients.Add(_cutOffPoint - 0.0001f, Color.Transparent);
                newColorGradients.Add(0.0f, Color.Transparent);

                _newbrush.colorGradients = newColorGradients;
            }
            else if (_cutOffPoint > 1.0f)
            {
                foreach (var kvp in spectrum.GetSpectrumColors())
                {
                    if (kvp.Key >= (1 - 1 / _cutOffPoint))
                    {
                        newColorGradients.Add((1 - 1 / _cutOffPoint) * kvp.Key + _cutOffPoint, kvp.Value);
                    }
                }

                newColorGradients.Add(0.0f, spectrum.GetColorAt((1 - 1 / _cutOffPoint)));
            }

            _newbrush.SetBrushType(EffectBrush.BrushType.Radial);
            Brush brush = _newbrush.GetDrawingBrush();

            if(brush is PathGradientBrush)
            {
                (brush as PathGradientBrush).TranslateTransform(_scaledDimension.X, _scaledDimension.Y);
                (brush as PathGradientBrush).ScaleTransform(_scaledDimension.Width - (2.0f), _scaledDimension.Height - (2.0f));

                Matrix rotationMatrix = new Matrix();
                rotationMatrix.RotateAt(-_angle, new PointF(_center.X * scale, _center.Y * scale), MatrixOrder.Append);

                Matrix originalMatrix = g.Transform;
                g.Transform = rotationMatrix;
                g.FillEllipse(brush, _scaledDimension);
                g.Transform = originalMatrix;
            }
        }

        public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            if (!(otherAnim is AnimationGradientCircle))
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

            return new AnimationGradientCircle(newrect, _gradientBrush.BlendEffectBrush((otherAnim as AnimationGradientCircle)._gradientBrush, amount), newwidth).SetAngle(newAngle);
        }

        public override AnimationFrame GetCopy()
        {
            RectangleF newrect = new RectangleF(_dimension.X,
                _dimension.Y,
                _dimension.Width,
                _dimension.Height
                );

            return new AnimationGradientCircle(newrect, new EffectBrush(_gradientBrush), _width, _duration).SetAngle(_angle).SetTransitionType(_transitionType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AnimationGradientCircle)obj);
        }

        public bool Equals(AnimationGradientCircle p)
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
            return $"AnimationGradientCircle [ Color: {_color.ToString()} Dimensions: {_dimension.ToString()} Width: {_width} Duration: {_duration} Angle: {_angle} ]";
        }
    }
}
