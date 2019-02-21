using Aurora.Profiles;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {

    #region Types Enum
    /** Unfortunately, this section has been added to allow the genericness of the EvaluatablePresenter. We required it to be able to handle
     multiple types of IEvaluatable, but allow it to be restricted to one specific type (e.g. IEvaluatableBoolean). To do this I, originally
     had it take a "EvalType" property as a System.Type and you would pass the interface type that you wanted (e.g. IEvaluatableBoolean),
     which I thought worked well as it was possible to do in XAML by setting `EvalType="{x:Type, local:IEvaluatable}`, but it turns out that
     with XAML you cannot reference _interfaces_ that way, only actually classes. This seems like a design flaw with XAML if you ask me.
     The next option was to then bind the EvalType to a property, however this meant that I was adding extra properties to the DataContexts
     which was cluttering the class and was frankly quite ugly.
     The final solution I've settled on is to make the EvalType a enum instead (EvaluatableType) enum. That way, it can easily be set in the
     XAML editor (since enums are supported) and also does not polute the DataContext of the presenter containers. The downside is, I needed
     some way of converting the enum to interface type so that the code can generate a list of the matching classes. That's what this is: */

    /// <summary>Enum of all evaluatable types.</summary>
    public enum EvaluatableType { All, Boolean, Number, String }
    /// <summary>Class that stores a dictionary to convert EvaluatableType enums into the interface type.</summary>
    public static class EvaluatableTypeResolver {
        private static Dictionary<EvaluatableType, Type> enumToTypeDictionary = new Dictionary<EvaluatableType, Type> {
            { EvaluatableType.All, typeof(IEvaluatable) },
            { EvaluatableType.Boolean, typeof(IEvaluatableBoolean) },
            { EvaluatableType.Number, typeof(IEvaluatableNumber) },
            { EvaluatableType.String, typeof(IEvaluatableString) }
        };
        private static Dictionary<EvaluatableType, Func<IEvaluatable>> enumToDefaultDictionary = new Dictionary<EvaluatableType, Func<IEvaluatable>> {
            { EvaluatableType.All, () => null },
            { EvaluatableType.Boolean, () => new BooleanConstant() },
            { EvaluatableType.Number, () => new NumberConstant() },
            { EvaluatableType.String, () => new StringConstant() }
        };
        public static Type Resolve(EvaluatableType inType) => enumToTypeDictionary.TryGetValue(inType, out Type outType) ? outType : typeof(IEvaluatable);
        public static IEvaluatable GetDefault(EvaluatableType inType) => enumToDefaultDictionary.TryGetValue(inType, out Func<IEvaluatable> outFunc) ? outFunc() : null;
    }
    #endregion

    /// <summary>
    /// Interface that defines a logic operand that can be evaluated into a value. Should also have a Visual control that can
    /// be used to edit the operand. The control will be given the current application that can be used to have contextual
    /// prompts (e.g. a dropdown list with the valid game state variable paths) for that application.
    /// </summary>
    public interface IEvaluatable {
        /// <summary>Should evaluate the operand and return the evaluation result.</summary>
        object Evaluate(IGameState gameState);

        /// <summary>Should return a control that is bound to this logic element.</summary>
        Visual GetControl(Application application);

        /// <summary>Indicates the UserControl should be updated with a new application.</summary>
        void SetApplication(Application application);

        /// <summary>Creates a copy of this IEvaluatable.</summary>
        IEvaluatable Clone();
    }

    /// <summary>
    /// Interface that defines a boolean logic operand that can be tested for truthiness.
    /// </summary>
    public interface IEvaluatableBoolean : IEvaluatable {
        /// <summary>Should evaluate the current condition (maybe based on the current game state) and return
        /// a bool indicating the result of the evaluation.</summary>
        new bool Evaluate(IGameState gameState);

        /// <summary>Creates a copy of this IEvaluatableBoolean.</summary>
        new IEvaluatableBoolean Clone();
    }

    /// <summary>
    /// Interface that defines a numberic logic operand that can be evaluated into a float.
    /// </summary>
    public interface IEvaluatableNumber : IEvaluatable {
        /// <summary>Should evaluate the current expression and return a double with the result of the evaluation.</summary>
        new double Evaluate(IGameState gameState);

        /// <summary>Creates a copy of this IEvaluatableNumber.</summary>
        new IEvaluatableNumber Clone();
    }

    /// <summary>
    /// Interface that defines a string logic operand that can be evaluated into a string of characters.
    /// </summary>
    public interface IEvaluatableString : IEvaluatable {
        /// <summary>Should evaluate the current expression and return a string with the result.</summary>
        new string Evaluate(IGameState gameState);

        /// <summary>Creates a copy of this IEvaluatableString.</summary>
        new IEvaluatableString Clone();
    }
}
