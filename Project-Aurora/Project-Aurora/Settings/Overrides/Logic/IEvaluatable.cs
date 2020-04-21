using Aurora.Profiles;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Interface that defines a logic operand that can be evaluated into a value. Should also have a Visual control
    /// that can be used to edit the operand.
    /// </summary>
    public interface IEvaluatable {
        /// <summary>Should evaluate the operand and return the evaluation result.</summary>
        object Evaluate(IGameState gameState);

        /// <summary>Should return a control that is bound to this logic element.</summary>
        Visual GetControl();

        /// <summary>Creates a copy of this IEvaluatable.</summary>
        IEvaluatable Clone();
    }

    public interface IEvaluatable<T> : IEvaluatable
    {
        /// <summary>Should evaluate the operand and return the evaluation result.</summary>
        new T Evaluate(IGameState gameState);

        /// <summary>Creates a copy of this IEvaluatable.</summary>
        new IEvaluatable<T> Clone();
    }


    /// <summary>
    /// Class that provides a lookup for the default Evaluatable for a particular type.
    /// </summary>
    public static class EvaluatableDefaults {

        private static Dictionary<Type, Type> defaultsMap = new Dictionary<Type, Type> {
            [typeof(bool)] = typeof(BooleanConstant),
            [typeof(double)] = typeof(NumberConstant),
            [typeof(string)] = typeof(StringConstant)
        };

        public static IEvaluatable<T> Get<T>() => (IEvaluatable<T>)Get(typeof(T));

        public static IEvaluatable Get(Type t) {
            if (!defaultsMap.TryGetValue(t, out Type @default))
                throw new ArgumentException($"Type '{t.Name}' does not have a default evaluatable type.");
            return (IEvaluatable)Activator.CreateInstance(@default);
        }
    }
}
