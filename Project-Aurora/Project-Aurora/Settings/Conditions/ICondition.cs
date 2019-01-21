using Aurora.Profiles;
using System.Windows.Controls;

namespace Aurora.Settings.Conditions {

    /// <summary>
    /// Interface that defines a condition that can be tested for truthiness.
    /// Should also have a UserControl that can be added to the condition editor control and can be used to edit the condition.
    /// The control will be given the current application that can be used to have contextual prompts (e.g. a dropdown list with
    /// the valid game state variable paths) for that application.
    /// </summary>
    public interface ICondition {
        /// <summary>Should evaluate the current condition (maybe based on the current game state) and return
        /// a bool indicating the result of the evaluation.</summary>
        bool Evaluate(IGameState gameState);

        /// <summary>Should return a control that is bound to this condition.</summary>
        UserControl GetControl(Application application);

        /// <summary>Indicates the UserControl should be updated with a new application.</summary>
        void SetApplication(Application application);
    }
}
