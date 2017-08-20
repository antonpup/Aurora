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
    }
}
