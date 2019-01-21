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



    /// <summary>
    /// Condition that accesses some specified game state variables (of numeric type) and returns a comparison between them.
    /// </summary>
    [Condition("Numeric Game State Variable")]
    public class ConditionGSINumeric : ICondition {

        // Path to the two GSI variables (or numbers themselves) and the operator to compare them with
        public string Operand1Path { get; set; }
        public string Operand2Path { get; set; }
        public ComparisonOperator Operator { get; set; } = ComparisonOperator.EQ;

        // Control assigned to this condition
        private Control_ConditionGSINumeric control;
        public UserControl GetControl(Application application) {
            if (control == null)
                control = new Control_ConditionGSINumeric(this, application);
            return control;
        }

        /// <summary>Parses the numbers, compares the result, and returns the result.</summary>
        public bool Evaluate(IGameState gameState) {
            // Parse the operands (either as numbers or paths)
            double op1 = Utils.GameStateUtils.TryGetDoubleFromState(gameState, Operand1Path);
            double op2 = Utils.GameStateUtils.TryGetDoubleFromState(gameState, Operand2Path);

            // Evaluate the operands based on the selected operator and return the result.
            switch (Operator) {
                case ComparisonOperator.EQ: return op1 == op2;
                case ComparisonOperator.NEQ: return op1 != op2;
                case ComparisonOperator.LT: return op1 < op2;
                case ComparisonOperator.LTE: return op1 <= op2;
                case ComparisonOperator.GT: return op1 > op2;
                case ComparisonOperator.GTE: return op1 >= op2;
                default: return false;
            }
        }

        /// <summary>Update the assigned control with the new application.</summary>
        public void SetApplication(Application application) {
            control?.SetApplication(application);
        }
    }
}
