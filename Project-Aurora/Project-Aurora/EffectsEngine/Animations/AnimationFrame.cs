using System;
using System.ComponentModel;
using System.Drawing;

namespace Aurora.EffectsEngine.Animations
{
    /// <summary>
    /// Enum list for different frame transition types
    /// </summary>
    public enum AnimationFrameTransitionType
    {
        /// <summary>
        /// None
        /// </summary>
        [Description("None")]
        None = -1,

        /// <summary>
        /// Linear
        /// </summary>
        [Description("Linear")]
        Linear = 0,

        /// <summary>
        /// Exponential
        /// </summary>
        [Description("Exponential")]
        Exponential = 1,

        /// <summary>
        /// Squared
        /// </summary>
        [Description("Squared")]
        Squared = 2,

        /// <summary>
        /// Cubed
        /// </summary>
        [Description("Cubed")]
        Cubed = 3,

        /// <summary>
        /// Square Rooted
        /// </summary>
        [Description("Square Rooted")]
        SquareRooted = 4,

        /// <summary>
        /// Square Rooted
        /// </summary>
        [Description("Sin Pulse")]
        SinPulse = 5,
    }

    public class AnimationFrame
    {
        [Newtonsoft.Json.JsonProperty]
        internal Color _color;
        [Newtonsoft.Json.JsonProperty]
        internal RectangleF _dimension;
        [Newtonsoft.Json.JsonProperty]
        internal int _width;
        [Newtonsoft.Json.JsonProperty]
        internal float _duration;
        internal Pen _pen = null;
        internal Brush _brush = null;
        internal bool _invalidated = true;
        [Newtonsoft.Json.JsonProperty]
        internal AnimationFrameTransitionType _transitionType = AnimationFrameTransitionType.Linear;
        [Newtonsoft.Json.JsonProperty]
        internal float _angle = 0.0f;

        public Color Color { get { return _color; } }
        public RectangleF Dimension { get { return _dimension; } }
        public int Width { get { return _width; } }
        public float Duration { get { return _duration; } }
        public AnimationFrameTransitionType TransitionType { get { return _transitionType; } }
        public float Angle { get { return _angle; } }

        public AnimationFrame()
        {
            _color = new Color();
            _dimension = new RectangleF();
            _width = 1;
            _duration = 0.0f;
        }

        public AnimationFrame(Rectangle dimension, Color color, int width = 1, float duration = 0.0f)
        {
            _color = color;
            _dimension = dimension;
            _width = width;
            _duration = duration;
        }

        public AnimationFrame(RectangleF dimension, Color color, int width = 1, float duration = 0.0f)
        {
            _color = color;
            _dimension = dimension;
            _width = width;
            _duration = duration;
        }

        public AnimationFrame SetColor(Color color)
        {
            _color = color;
            _invalidated = true;

            return this;
        }

        public AnimationFrame SetDimension(RectangleF dimension)
        {
            _dimension = dimension;
            _invalidated = true;

            return this;
        }

        public AnimationFrame SetWidth(int width)
        {
            _width = width;
            _invalidated = true;

            return this;
        }

        public AnimationFrame SetDuration(float duration)
        {
            //Duration cannot be negative
            if (duration < 0)
                Global.logger.Warn($"Negative duration!!! duration={duration}");
            else
                _duration = duration;

            return this;
        }

        public AnimationFrame SetTransitionType(AnimationFrameTransitionType type)
        {
            _transitionType = type;

            return this;
        }

        public AnimationFrame SetAngle(float angle)
        {
            _angle = angle;
            _invalidated = true;

            return this;
        }

        public virtual void Draw(Graphics g, float scale = 1.0f, PointF offset = default(PointF)) { }
        public virtual AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            amount = GetTransitionValue(amount);

            RectangleF newrect = new RectangleF((float)CalculateNewValue(_dimension.X, otherAnim._dimension.X, amount),
                (float)CalculateNewValue(_dimension.Y, otherAnim._dimension.Y, amount),
                (float)CalculateNewValue(_dimension.Width, otherAnim._dimension.Width, amount),
                (float)CalculateNewValue(_dimension.Height, otherAnim._dimension.Height, amount)
                );

            AnimationFrame newframe = new AnimationFrame();
            newframe._dimension = newrect;
            newframe._color = Utils.ColorUtils.BlendColors(_color, otherAnim._color, amount);

            return newframe;
        }

        internal double GetTransitionValue(double amount)
        {
            double returnamount = 0.0;

            switch (_transitionType)
            {
                //A linear relationship between frames y = x
                case AnimationFrameTransitionType.Linear:
                    returnamount = amount;
                    break;
                //An exponential relationship between frames y = (e^x - 1)/(e - 1)
                case AnimationFrameTransitionType.Exponential:
                    returnamount = (Math.Exp(amount) - 1) / (Math.E - 1);
                    break;
                //A squared relationship between frames y = x^2
                case AnimationFrameTransitionType.Squared:
                    returnamount = Math.Pow(amount, 2.0);
                    break;
                //A cubed relationship between frames y = x^3
                case AnimationFrameTransitionType.Cubed:
                    returnamount = Math.Pow(amount, 3.0);
                    break;
                //A cubed relationship between frames y = x^0.5
                case AnimationFrameTransitionType.SquareRooted:
                    returnamount = Math.Pow(amount, 0.5);
                    break;
                //A cubed relationship between frames y = sin ^ 2(x * 1.5 * pi) * x
                case AnimationFrameTransitionType.SinPulse:
                    returnamount = Math.Pow(Math.Sin(amount * 1.5 * Math.PI), 2) * amount;
                    break;
                default:
                    returnamount = 0.0;
                    break;
            }

            if (returnamount < 0.0)
                returnamount = 0;
            else if (returnamount > 1.0)
                returnamount = 1.0;

            return returnamount;
        }

        internal double CalculateNewValue(double first, double second, double amount)
        {
            if (first == second)
                return first;
            else
                return (double)(first * (1.0 - amount) + second * (amount));
        }

        public virtual AnimationFrame GetCopy()
        {
            RectangleF newrect = new RectangleF(_dimension.X,
                _dimension.Y,
                _dimension.Width,
                _dimension.Height
                );

            AnimationFrame copy = new AnimationFrame();
            copy._dimension = newrect;
            copy._color = Color.FromArgb(_color.A, _color.R, _color.G, _color.B);

            return copy;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AnimationFrame)obj);
        }

        public bool Equals(AnimationFrame p)
        {
            return _color.Equals(p._color) &&
                _dimension.Equals(p._dimension) &&
                _width.Equals(p._width) &&
                _duration.Equals(p._duration);
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
                return hash;
            }
        }

        public override string ToString()
        {
            return "AnimationFrame [ Color: " + _color.ToString() + " Dimensions: " + _dimension.ToString() + " Width: " + _width + "]";
        }
    }
}
