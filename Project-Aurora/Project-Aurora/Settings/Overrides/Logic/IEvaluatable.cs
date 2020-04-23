using Aurora.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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


    /// <summary>
    /// Helper classes for the Evaluatables.
    /// </summary>
    public static class EvaluatableHelpers {
        /// <summary>Attempts to get an evaluatable from the suppliied data object. Will return true/false indicating if data is of correct format
        /// (an <see cref="IEvaluatable{T}"/> where T matches the given type. If the eval type is null, no type check is performed, the returned
        /// evaluatable may be of any sub-type.</summary>
        internal static bool TryGetData(IDataObject @do, out IEvaluatable evaluatable, out Control_EvaluatablePresenter source, Type evalType) {
            if (@do.GetData(@do.GetFormats().FirstOrDefault(x => x != "SourcePresenter")) is IEvaluatable data && (evalType == null || Utils.TypeUtils.ImplementsGenericInterface(data.GetType(), typeof(IEvaluatable<>), evalType))) {
                evaluatable = data;
                source = @do.GetData("SourcePresenter") as Control_EvaluatablePresenter;
                return true;
            }
            evaluatable = null;
            source = null;
            return false;
        }
    }
}
