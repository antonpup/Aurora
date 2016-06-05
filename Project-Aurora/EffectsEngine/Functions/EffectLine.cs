using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.EffectsEngine.Functions
{
    public class EffectLine : EffectFunction
    {
        private float slope;
        private float y_offset;

        private float x_low_bound;
        private float x_high_bound;
        private float y_low_bound;
        private float y_high_bound;

        public EffectLine()
        {
            slope = 1.0f;
            y_offset = 0.0f;

            SetDefaultBounds();
        }

        public EffectLine(float slope, float y_offset = 0.0f)
        {
            this.slope = slope;
            this.y_offset = y_offset;

            SetDefaultBounds();
        }

        public EffectLine(EffectPoint start, EffectPoint end, bool isBoundedByPoints = false)
        {
            if ((end.X - start.X) <= 0.1f && (end.X - start.X) >= -0.1f)
            {
                this.slope = float.NaN;
                this.y_offset = start.X + start.Y;
            }
            else
            {
                this.slope = (end.Y - start.Y) / (end.X - start.X);
                this.y_offset = (-this.slope * start.X) + start.Y;
            }

            if(isBoundedByPoints)
            {
                if(start.X < end.X)
                    SetXBounds(start.X, end.X);
                else
                    SetXBounds(end.X, start.X);

                if (start.Y < end.Y)
                    SetYBounds(start.Y, end.Y);
                else
                    SetYBounds(end.Y, start.Y);

            }
            else
                SetDefaultBounds();
        }

        private void SetDefaultBounds()
        {
            x_low_bound = 0.0f;
            x_high_bound = (float)Effects.canvas_width;
            y_low_bound = 0.0f;
            y_high_bound = (float)Effects.canvas_height;
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

        public EffectPoint GetPoint(float x)
        {
            EffectPoint ret;

            if (float.IsNaN(slope))
            {
                ret = new EffectPoint(y_offset, x);
            }
            else
            {
                ret = new EffectPoint(x, slope * x + y_offset);
            }

            if (ret.X < this.x_low_bound || ret.X > this.x_high_bound)
                ret.X = float.NaN;
            if (ret.Y < this.y_low_bound || ret.Y > this.y_high_bound)
                ret.Y = float.NaN;

            return ret;
        }

        public EffectPoint GetOrigin()
        {
            if (this.x_low_bound == float.MinValue && this.x_high_bound == float.MaxValue)
            {
                return GetPoint(0.0f);
            }
            else if (this.x_low_bound > this.x_high_bound)
            {
                return GetPoint(this.x_high_bound);
            }
            else if (this.x_low_bound < this.x_high_bound)
            {
                return GetPoint(this.x_low_bound);
            }

            return GetPoint(0.0f);
        }
    }
}
