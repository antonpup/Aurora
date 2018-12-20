using Aurora.Devices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.EffectsEngine.Animations
{
    public class AnimationManualColorFrame : AnimationFrame
    {
        [Newtonsoft.Json.JsonProperty]
        private Dictionary<DeviceKeys, Color> _BitmapColors = new Dictionary<DeviceKeys, Color>();

        public Dictionary<DeviceKeys, Color> BitmapColors {
            get { return new Dictionary<DeviceKeys, Color>(_BitmapColors); }
        }

        public AnimationFrame SetKeyColor(DeviceKeys Key, Color Color)
        {
            if (_BitmapColors.ContainsKey(Key))
                _BitmapColors[Key] = Color;
            else
                _BitmapColors.Add(Key, Color);

            return this;
        }

        public AnimationFrame SetBitmapColors(Dictionary<DeviceKeys, Color> ColorMapping)
        {
            if(ColorMapping != null)
                _BitmapColors = ColorMapping;

            return this;
        }

        public AnimationManualColorFrame()
        {
            _BitmapColors = new Dictionary<DeviceKeys, Color>();
            _duration = 0.0f;
        }

        public AnimationManualColorFrame(Dictionary<DeviceKeys, Color> ColorMapping, float duration = 0.0f)
        {
            _BitmapColors = ColorMapping;

            _duration = duration;
        }

        public override void Draw(Graphics g, float scale = 1.0f, PointF offset = default(PointF))
        {
            // Offset has no effect on this type of animation frame
            if (_brush == null || _invalidated)
            {
                _brush = new SolidBrush(_color);
                _invalidated = false;
            }

            foreach (var kvp in _BitmapColors)
            {
                var region = Effects.GetBitmappingFromDeviceKey(kvp.Key);

                g.FillRectangle(new SolidBrush(kvp.Value), region.Left * scale, region.Top * scale, region.Width * scale, region.Height * scale);
            }
        }

        public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
        {
            if (!(otherAnim is AnimationManualColorFrame))
            {
                throw new FormatException("Cannot blend with another type");
            }

            Dictionary<DeviceKeys, Color> _combinedBitmapColors = new Dictionary<DeviceKeys, Color>();
            amount = GetTransitionValue(amount);

            foreach (var kvp in _BitmapColors)
            {
                if ((otherAnim as AnimationManualColorFrame)._BitmapColors.ContainsKey(kvp.Key))
                {
                    _combinedBitmapColors.Add(kvp.Key, Utils.ColorUtils.BlendColors(kvp.Value, (otherAnim as AnimationManualColorFrame)._BitmapColors[kvp.Key], amount));
                }
                else
                {
                    _combinedBitmapColors.Add(kvp.Key, Utils.ColorUtils.MultiplyColorByScalar(kvp.Value, 1.0 - amount));
                }
            }

            foreach (var kvp in (otherAnim as AnimationManualColorFrame)._BitmapColors)
            {
                if (!_BitmapColors.ContainsKey(kvp.Key))
                {
                    _combinedBitmapColors.Add(kvp.Key, Utils.ColorUtils.MultiplyColorByScalar(kvp.Value, amount));
                }
            }

            AnimationManualColorFrame newAnim = new AnimationManualColorFrame();
            newAnim._BitmapColors = _combinedBitmapColors;

            return newAnim;
        }

        public override AnimationFrame GetCopy()
        {
            Dictionary<DeviceKeys, Color> newmapping = new Dictionary<DeviceKeys, Color>(_BitmapColors);

            return new AnimationManualColorFrame(newmapping, _duration).SetAngle(_angle).SetTransitionType(_transitionType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AnimationManualColorFrame)obj);
        }

        public bool Equals(AnimationManualColorFrame p)
        {
            return _BitmapColors.Equals(p._BitmapColors) &&
                _duration.Equals(p._duration);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _BitmapColors.GetHashCode();
                hash = hash * 23 + _duration.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"AnimationManualColorFrame [ Duration: {_duration} ]";
        }


    }
}
