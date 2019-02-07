using System;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Class that handles the dynamic searching of any IEvaluatable classes with the OverrideLogic attribute applied to them.
    /// </summary>
    public static class EvaluatableRegistry {

        /// <summary>Cached list of all classes that have the OverrideLogic attribute applied to them.</summary>
        private static readonly Dictionary<Type, OverrideLogicAttribute> allOverrideLogics = Utils.TypeUtils
            .GetTypesWithCustomAttribute<OverrideLogicAttribute>()
            .Where(kvp => typeof(IEvaluatable).IsAssignableFrom(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        /// <summary>Cached list of all classes with a OverrideLogic attribute that also are a specific subtype.</summary>
        private static readonly Dictionary<Type, Dictionary<Type, OverrideLogicAttribute>> specificOverrideLogics = new Dictionary<Type, Dictionary<Type, OverrideLogicAttribute>>();

        /// <summary>Fetches a specific subset of logic operand types (e.g. all booleans).
        /// Caches results to that subsequent calls are marginally faster.</summary>
        /// <typeparam name="T">The type to fetch (e.g. IEvaluatableBoolean).</typeparam>
        public static Dictionary<Type, OverrideLogicAttribute> Get<T>() where T : IEvaluatable {
            if (!specificOverrideLogics.ContainsKey(typeof(T)))
                specificOverrideLogics[typeof(T)] = allOverrideLogics
                    .Where(kvp => typeof(T).IsAssignableFrom(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return specificOverrideLogics[typeof(T)];
        }

        /// <summary>Fetches all logic operands that have been found with the OverrideLogicAttribute attached.</summary>
        public static Dictionary<Type, OverrideLogicAttribute> Get() => allOverrideLogics;
    }
}
