using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Class that handles the dynamic searching of any IEvaluatable classes with the OverrideLogic attribute applied to them.
    /// </summary>
    public static class EvaluatableRegistry {

        /// <summary>Cached list of all classes that have the OverrideLogic attribute applied to them.</summary>
        private static readonly IEnumerable<EvaluatableTypeContainer> allOverrideLogics = Utils.TypeUtils
            .GetTypesWithCustomAttribute<EvaluatableAttribute>()
            .Where(kvp => typeof(IEvaluatable).IsAssignableFrom(kvp.Key))
            .OrderBy(kvp => kvp.Value.Name, StringComparer.OrdinalIgnoreCase)
            .Select(kvp => new EvaluatableTypeContainer {
                Evaluatable = kvp.Key,
                Metadata = kvp.Value,
                ResultType = kvp.Key.GetGenericParentTypes(typeof(Evaluatable<>))[0]
            });

        /// <summary>Fetches a specific subset of logic operand types (e.g. all booleans).
        /// Caches results to that subsequent calls are marginally faster.</summary>
        /// <typeparam name="T">The type to fetch (e.g. IEvaluatable&lt;bool>&gt;).</typeparam>
        public static IEnumerable<EvaluatableTypeContainer> Get<T>() where T : IEvaluatable => Get(typeof(T));

        /// <summary>Fetches a specific subset of logic operand types (e.g. all booleans).</summary>
        /// <param name="t">The type to fetch (e.g. IEvaluatable&lt;bool&gt;).</param>
        public static IEnumerable<EvaluatableTypeContainer> Get(Type t) {
            // Ensure all numbers (double, float, int, etc) become double
            if (TypeUtils.IsNumericType(t)) t = typeof(double);

            return allOverrideLogics.Where(kvp => kvp.ResultType == t);
        }

        /// <summary>Fetches all logic operands that have been found with the OverrideLogicAttribute attached.</summary>
        public static IEnumerable<EvaluatableTypeContainer> Get() => allOverrideLogics;



        public class EvaluatableTypeContainer {

            /// <summary>The <see cref="Type"/> that represents the evaluatable.</summary>
            public Type Evaluatable { get; set; }

            /// <summary>The <see cref="EvaluatableAttribute"/> that contains the metadata about this evaluatable.</summary>
            public EvaluatableAttribute Metadata { get; set; }

            /// <summary>The <see cref="Type"/> that represents the type of value that is returned when this evaluatable is evaluated.</summary>
            public Type ResultType { get; set; }
        }
    }
}
