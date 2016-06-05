using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.EffectsEngine.Functions
{
    public class EffectCircle : EffectFunction
    {
        private EffectPoint origin;
        private float radius;

        private float x_low_bound;
        private float x_high_bound;
        private float y_low_bound;
        private float y_high_bound;

        public EffectCircle(float origin_x, float origin_y, float radius = 1.0f)
        {
            origin = new EffectPoint(origin_x, origin_y);
            this.radius = radius;

            SetDefaultBounds();
        }

        public EffectCircle(EffectPoint origin, float radius = 1.0f)
        {
            this.origin = origin;
            this.radius = radius;

            SetDefaultBounds();
        }

        private void SetDefaultBounds()
        {
            x_low_bound = 0.0f;
            x_high_bound = (float)Effects.canvas_width;
            y_low_bound = 0.0f;
            y_high_bound = (float)Effects.canvas_height;
        }

        public EffectPoint GetOrigin()
        {
            return origin;
        }

        public EffectPoint GetPoint(float t)
        {
            EffectPoint ret = new EffectPoint(radius * (float)Math.Cos(t) + origin.X, radius * (float)Math.Sin(t) + origin.Y);

            if (ret.X < this.x_low_bound || ret.X > this.x_high_bound)
                ret.X = float.NaN;
            if (ret.Y < this.y_low_bound || ret.Y > this.y_high_bound)
                ret.Y = float.NaN;

            return ret;
        }

        public void SetXBounds(float low_bound, float high_bound)
        {
            this.x_low_bound = low_bound;
            this.x_high_bound = high_bound;
        }

        public void SetYBounds(float low_bound, float high_bound)
        {
            this.y_low_bound = low_bound;
            this.y_high_bound = high_bound;
        }

        public bool IsASegment()
        {
            return true;
        }
    }
}
