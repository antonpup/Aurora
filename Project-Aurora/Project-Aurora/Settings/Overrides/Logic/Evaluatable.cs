using Aurora.Profiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Interface that defines a logic operand that can be evaluated into a value. Should also have a Visual control
    /// that can be used to edit the operand.
    /// </summary>
    public interface IEvaluatable {
        /// <summary>The most recent value that was output from the evaluatable.</summary>
        object LastValue { get; }

        /// <summary>Should evaluate the operand and return the evaluation result.</summary>
        object Evaluate(IGameState gameState);

        /// <summary>Should return a control that is bound to this logic element.</summary>
        Visual GetControl();

        /// <summary>Creates a copy of this IEvaluatable.</summary>
        IEvaluatable Clone();
    }

    public abstract class Evaluatable<T> : IEvaluatable, System.ComponentModel.INotifyPropertyChanged {

        /// <summary>The most recent value that was output from the evaluatable.</summary>
        public T LastValue { get; private set; } = default;
        object IEvaluatable.LastValue => LastValue;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Should evaluate the operand and return the evaluation result.</summary>
        protected abstract T Execute(IGameState gameState);

        /// <summary>Evaluates the result of this evaluatable with the given gamestate and returns the result.</summary>
        // Execute the evaluatable logic, store the latest value and return this value
        public T Evaluate(IGameState gameState) => (LastValue = Execute(gameState));
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Should return a control that is bound to this logic element.</summary>
        public abstract Visual GetControl();

        /// <summary>Creates a copy of this IEvaluatable.</summary>
        public abstract Evaluatable<T> Clone();
        IEvaluatable IEvaluatable.Clone() => Clone();
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

        public static Evaluatable<T> Get<T>() => (Evaluatable<T>)Get(typeof(T));

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
        /// (an <see cref="Evaluatable{T}"/> where T matches the given type. If the eval type is null, no type check is performed, the returned
        /// evaluatable may be of any sub-type.</summary>
        internal static bool TryGetData(IDataObject @do, out IEvaluatable evaluatable, out Control_EvaluatablePresenter source, Type evalType) {
            if (@do.GetData(@do.GetFormats().FirstOrDefault(x => x != "SourcePresenter")) is IEvaluatable data && (evalType == null || Utils.TypeUtils.GetGenericParentTypes(data.GetType(), typeof(Evaluatable<>))[0] == evalType)) {
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
