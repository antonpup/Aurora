using System;

namespace Aurora.Utils
{
    public static class TypeExtensions
    {

        public static object TryClone(this object self)
        {
            if (self is ICloneable)
                return ((ICloneable)self).Clone();
            else
                return self;
        }
    }
}
