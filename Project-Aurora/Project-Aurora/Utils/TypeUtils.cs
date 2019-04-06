using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Utils
{
    public static class TypeUtils
    {
        //Solution from http://stackoverflow.com/questions/1749966/c-sharp-how-to-determine-whether-a-type-is-a-number
        public static bool IsNumericType(this object o)
        {
            return IsNumericType(o.GetType());
        }

        public static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Inspects all types in all current assemblies and returns a dictionary containing all types that have the specified custom attribute.
        /// </summary>
        /// <typeparam name="T">The type of custom attribute to fetch.</typeparam>
        /// <returns>A dictionary containing the type and the custom attribute assigned to it.</returns>
        public static IEnumerable<KeyValuePair<Type, T>> GetTypesWithCustomAttribute<T>() where T : Attribute {
            return AppDomain.CurrentDomain.GetAssemblies() // Get all the assemblies
                .SelectMany(assembly => {
                    try { return assembly.GetTypes(); } // Attempt to get all types in these assemblies
                    catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); } // If there was an error loading some of them, return only the successful ones
                })
                .ToDictionary(type => type, type => (T)Attribute.GetCustomAttribute(type, typeof(T))) // Attempt to get the specified attibute of them
                .Where(kvp => kvp.Value != null); // Filter out any where the attribute is null (i.e. doesn't exist)
        }

        /// <summary>
        /// Checks if the given type implements the given interface type.
        /// </summary>
        public static bool IsInterface(Type type, Type @interface) => @interface.IsAssignableFrom(type);
    }
}
