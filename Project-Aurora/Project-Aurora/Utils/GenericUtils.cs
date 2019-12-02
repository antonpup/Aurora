using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> pair, out T1 key, out T2 value)
        {
            key = pair.Key;
            value = pair.Value;
        }
    }
}
