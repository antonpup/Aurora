using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Aurora.Core.Utils
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Gets the type from the Assembly with `name`, as the usual GetType uses the 'FullName' which includes the namespace the Type is in
        /// </summary>
        /// <param name="assem"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Type GetTypeFromShortName(this Assembly assem, string name)
        {
            return Array.Find(assem.GetTypes(), (typ) => typ.Name.Equals(name));
        }
    }
}
