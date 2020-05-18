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


        /// <summary>Checks whether the given 'this' type extends the given *generic* interface, regardless of type parameters.</summary>
        /// <param name="type">The type that will be checked to see if ti implements the given interface.</param>
        /// <param name="interfaceType">The type of generic interface to check if 'type' extends. Does not account for type parameters.</param>
        public static bool ImplementsGenericInterface(this Type type, Type interfaceType) =>
            type.GetInterfaces().Select(i => i.IsGenericType ? i.GetGenericTypeDefinition() : i).Contains(interfaceType);

        /// <summary>Checks whether the given 'this' type extends the given *generic* interface with the given type parameters.</summary>
        /// <param name="type">The type that will be checked to see if it implements the given interface.</param>
        /// <param name="interfaceType">The type of generic interface to check if 'type' extends.</param>
        /// <param name="interfaceGenericParameters">An array of types to check against the type parameters in the generic interface.
        /// Note that the length of this array must match the number of type parameters in the target interface.</param>
        public static bool ImplementsGenericInterface(Type type, Type interfaceType, params Type[] interfaceGenericParameters) =>
            type.GetInterfaces()
                .Where(i => i.IsGenericType)
                .SingleOrDefault(i => i.GetGenericTypeDefinition() == interfaceType)?
                .GetGenericArguments()
                    .Select((arg, i) => arg == interfaceGenericParameters[i])
                    .All(v => v)
                ?? false;

        /// <summary>Gets the generic argument types of the given interface for the given type.</summary>
        /// <param name="type">The type to check interfaces on.</param>
        /// <param name="interfaceType">The type of interface whose generic parameters to fetch.</param>
        /// <returns>An array containing all the types of generic parameters defined for this type on the given interface, or null if interface not found.</returns>
        public static Type[] GetGenericInterfaceTypes(this Type type, Type interfaceType) =>
            type.GetInterfaces()
                .Where(i => i.IsGenericType)
                .SingleOrDefault(i => i.GetGenericTypeDefinition() == interfaceType)?
                .GetGenericArguments();

        /// <summary>Gets the generic argument types of the given parent type for this type.
        /// Searches the parent heirarchy until <paramref name="parentType"/> is found.</summary>
        /// <param name="type">The type to check parent types for.</param>
        /// <param name="parentType">Searches the parent types of <paramref name="type"/> until this generic type is found, returning this type's type parameters.</param>
        /// <returns></returns>
        public static Type[] GetGenericParentTypes(this Type type, Type parentType) {
            var curType = type;
            while (curType != null) {
                if (curType.IsGenericType && curType.GetGenericTypeDefinition() == parentType)
                    return curType.GetGenericArguments();
                curType = curType.BaseType;
            }
            return null;
        }
    }
}
