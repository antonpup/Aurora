using System;
using System.Reflection;
using static System.Linq.Expressions.Expression;

namespace Aurora.Utils {

    // Using Activator.CreateInstance is slower than using a compiled LINQ expression: https://stackoverflow.com/a/16162809
    // These utility classes allow for stictly-typed instantiation of classes, using cached LINQ expressions. The first call to one
    // of these methods may be slow since it has to do reflection, but the result is cached and subsequent calls will be quicker.

    /// <summary>
    /// A static class for instantiating types using a parameterless constructor.
    /// </summary>
    /// <typeparam name="T">The type that will be instantiated.</typeparam>
    public static class Instantiator<T> {
        public static Func<T> Create { get; } = Lambda<Func<T>>(New(typeof(T))).Compile();
    }

    /// <summary>
    /// A static class for instantiating types using a single-parameter constructor.
    /// </summary>
    /// <typeparam name="T">The type that will be instantiated.</typeparam>
    /// <typeparam name="T1">The type of the first parameter passed to the constructor.</typeparam>
    public static class Instantiator<T, T1> {
        static Instantiator() {
            var arg0 = Parameter(typeof(T1));
            Create = Lambda<Func<T1, T>>(New(Instantiator.Constructor(typeof(T), typeof(T1)), arg0), arg0).Compile();
        }

        public static Func<T1, T> Create { get; }
    }


    // Utility class for instantiators
    static class Instantiator {
        internal static ConstructorInfo Constructor(Type @class, params Type[] @params) =>
            @class.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, @params, null);
    }
}
