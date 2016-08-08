using System;

namespace Aurora.EffectsEngine.Functions
{
    public class EffectPoint : EffectFunction
    {
        public float X;
        public float Y;

        public EffectPoint()
        {
            X = 0.0f;
            Y = 0.0f;
        }

        public EffectPoint(float x = 0.0f, float y = 0.0f)
        {
            X = x;
            Y = y;
        }

        public EffectPoint(EffectPoint otherPoint)
        {
            X = otherPoint.X;
            Y = otherPoint.Y;
        }

        public System.Drawing.PointF ToPointF()
        {
            return new System.Drawing.PointF(X, Y);
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
                return false;

            EffectPoint p = obj as EffectPoint;
            if ((System.Object)p == null)
                return false;

            // Return true if the fields match:
            return (X == p.X) && (Y == p.Y);
        }

        public bool Equals(EffectPoint p)
        {
            if ((object)p == null)
                return false;

            return (X == p.X) && (Y == p.Y);
        }

        public static bool operator ==(EffectPoint lhs, EffectPoint rhs)
        {
            if ((object)lhs == null || (object)rhs == null)
            {
                return false;
            }

            return (lhs.X == rhs.X && lhs.Y == rhs.Y);
        }

        public static bool operator !=(EffectPoint lhs, EffectPoint rhs)
        {
            return !(lhs == rhs);
        }

        public static EffectPoint operator +(EffectPoint lhs, EffectPoint rhs)
        {
            if ((object)lhs == null)
            {
                return rhs;
            }

            if ((object)rhs == null)
            {
                return lhs;
            }

            return new EffectPoint(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }

        public static EffectPoint operator -(EffectPoint lhs, EffectPoint rhs)
        {
            if ((object)lhs == null)
            {
                return new EffectPoint(-rhs.X, -rhs.Y);
            }

            if ((object)rhs == null)
            {
                return lhs;
            }

            return new EffectPoint(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static EffectPoint operator *(float lhs, EffectPoint rhs)
        {
            if ((object)lhs == null)
            {
                return rhs;
            }

            return new EffectPoint(rhs.X * lhs, rhs.Y * lhs);
        }

        public static EffectPoint operator /(float lhs, EffectPoint rhs)
        {
            if ((object)lhs == null || lhs == 0)
            {
                return rhs;
            }

            return rhs * (1.0f / lhs);
        }

        public static EffectPoint operator *(EffectPoint lhs, float rhs)
        {
            if ((object)rhs == null)
            {
                return lhs;
            }

            return new EffectPoint(lhs.X * rhs, lhs.Y * rhs);
        }

        public static EffectPoint operator /(EffectPoint lhs, float rhs)
        {
            if ((object)rhs == null || rhs == 0)
            {
                return lhs;
            }

            return lhs * (1.0f / rhs);
        }

        public override string ToString()
        {
            return (String.Format("<X: {0}, Y: {1}>", X, Y));
        }

        public override int GetHashCode()
        {
            return Math.Pow(X, Y).GetHashCode();
        }

        public void SetXBounds(float low_bound, float high_bound)
        {
            return; //There are no bounds
        }

        public void SetYBounds(float low_bound, float high_bound)
        {
            return; //There are no bounds
        }

        public EffectPoint GetPoint(float x)
        {
            return this;
        }

        public float GetStartingT()
        {
            return 0.0f;
        }

        public float GetEndingT()
        {
            return 0.0f;
        }

        public EffectPoint GetOrigin()
        {
            return this;
        }

        public bool IsOutOfRange()
        {
            return (float.IsNaN(X) || float.IsInfinity(X) || float.IsNaN(Y) || float.IsInfinity(Y));
        }
    }
}
