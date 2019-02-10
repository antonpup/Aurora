using Aurora.Profiles;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {

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
}
