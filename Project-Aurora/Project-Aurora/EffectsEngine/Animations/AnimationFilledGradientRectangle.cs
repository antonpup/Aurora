using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationFilledGradientRectangle : AnimationFilledRectangle
    {
        [Newtonsoft.Json.JsonProperty]
        internal EffectBrush _gradientBrush;

        private readonly Brush _drawingBrush;

        public EffectBrush GradientBrush => _gradientBrush;

        public AnimationFilledGradientRectangle()
        {
        }

        public AnimationFilledGradientRectangle(AnimationFilledGradientRectangle animationFilledGradientRectangle) : base(animationFilledGradientRectangle)
        {
            _gradientBrush = animationFilledGradientRectangle.GradientBrush;
            _drawingBrush = _gradientBrush.GetDrawingBrush();
        }

        public AnimationFilledGradientRectangle(AnimationFrame animationFrame, EffectBrush effectBrush) : base(animationFrame)
        {
            _gradientBrush = effectBrush;
            _drawingBrush = _gradientBrush.GetDrawingBrush();
        }

        public AnimationFilledGradientRectangle(RectangleF dimension, EffectBrush brush, float duration = 0.0f) : base(dimension, Color.Transparent, duration)
        {
            _gradientBrush = brush;
            _drawingBrush = _gradientBrush.GetDrawingBrush();
        }

        public AnimationFilledGradientRectangle(float x, float y, float rect_width, float rect_height, EffectBrush brush, float duration = 0.0f) : base(x, y, rect_width, rect_height, Color.Transparent, duration)
        {
            _gradientBrush = brush;
            _drawingBrush = _gradientBrush.GetDrawingBrush();
        }

        public override void Draw(Graphics g)
        {
            if (_invalidated)
            {
                virtUpdate();
                _invalidated = false;
            }
            
            g.ResetTransform();
            g.Transform = _transformationMatrix;
            float drawX = _dimension.X - _dimension.Width/2;
            float drawY = _dimension.Y - _dimension.Height/2;
            g.FillRectangle(_drawingBrush, drawX, drawY, _dimension.Width, _dimension.Height);
        }

        public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            if (!(otherAnim is AnimationFilledGradientRectangle))
            {
                throw new FormatException("Cannot blend with another type");
            }
            AnimationFilledGradientRectangle otherCircle = (AnimationFilledGradientRectangle)otherAnim;

            amount = GetTransitionValue(amount);

            AnimationFrame newFrame = base.BlendWith(otherCircle, amount);

            return new AnimationFilledGradientRectangle(newFrame, _gradientBrush);
        }

        public override AnimationFrame GetCopy()
        {
            return new AnimationFilledGradientRectangle(this);
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
