using Aurora.Profiles;
using System.Windows.Controls;

namespace Aurora.Settings.Conditions {

    /// <summary>
    /// Condition that accesses a specific game state variable (of boolean type) and returns the state.
    /// </summary>
    [Condition("Boolean Game State Variable")]
    public class ConditionGSIBoolean : ICondition {

        /// <summary>The path to the variable the user wants to evaluate.</summary>
        public string VariablePath { get; set; } = "";

        /// <summary>The control assigned to this condition. Stored as a reference
        /// so that the application be updated if required.</summary>
        private Control_ConditionGSIBoolean control;
        public UserControl GetControl(Application application) {
            if (control == null)
                control = new Control_ConditionGSIBoolean(this, application);
            return control;
        }

        /// <summary>Fetches the given boolean value from the game state and returns it.</summary>
        public bool Evaluate(IGameState gameState) {
            bool result = false;
            if (VariablePath.Length > 0)
                try {
                    object tmp = Utils.GameStateUtils.RetrieveGameStateParameter(gameState, VariablePath);
                    result = (bool)Utils.GameStateUtils.RetrieveGameStateParameter(gameState, VariablePath);
                } catch { }
            return result;
        }

        /// <summary>Update the assigned control with the new application.</summary>
        public void SetApplication(Application application) {
            control?.SetApplication(application);
        }
    }
}
