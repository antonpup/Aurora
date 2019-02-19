using Aurora.Profiles;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Condition that accesses a specific game state variable (of boolean type) and returns the state.
    /// </summary>
    [OverrideLogic("Boolean State Variable", category: OverrideLogicCategory.State)]
    public class BooleanGSIBoolean : IEvaluatableBoolean {

        /// <summary>Creates an empty boolean state variable lookup.</summary>
        public BooleanGSIBoolean() { }
        /// <summary>Creates a evaluatable that returns the boolean variable at the given path.</summary>
        public BooleanGSIBoolean(string variablePath) { VariablePath = variablePath; }

        /// <summary>The path to the variable the user wants to evaluate.</summary>
        public string VariablePath { get; set; } = "";

        /// <summary>The control assigned to this condition. Stored as a reference
        /// so that the application be updated if required.</summary>
        [Newtonsoft.Json.JsonIgnore]
        private Control_ConditionGSIBoolean control;
        public Visual GetControl(Application application) {
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
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Update the assigned control with the new application.</summary>
        public void SetApplication(Application application) {
            control?.SetApplication(application);

            // Check to ensure the variable path is valid
            if (application != null && !string.IsNullOrWhiteSpace(VariablePath) && !application.ParameterLookup.ContainsKey(VariablePath))
                VariablePath = string.Empty;
        }

        public IEvaluatableBoolean Clone() => new BooleanGSIBoolean { VariablePath = VariablePath };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }



    /// <summary>
    /// Condition that accesses some specified game state variables (of numeric type) and returns a comparison between them.
    /// </summary>
    [OverrideLogic("Numeric State Variable", category: OverrideLogicCategory.State)]
    public class BooleanGSINumeric : IEvaluatableBoolean {

        /// <summary>Creates a blank numeric game state lookup evaluatable.</summary>
        public BooleanGSINumeric() { }
        /// <summary>Creates a numeric game state lookup that returns true when the variable at the given path equals the given value.</summary>
        public BooleanGSINumeric(string path1, double val) { Operand1Path = path1; Operand2Path = val.ToString(); }
        /// <summary>Creates a numeric game state lookup that returns true when the variable at path1 equals the given variable at path2.</summary>
        public BooleanGSINumeric(string path1, string path2) { Operand1Path = path1; Operand2Path = path2; }
        /// <summary>Creates a numeric game state lookup that returns a boolean depending on the given operator's comparison between the variable at the given path and the value.</summary>
        public BooleanGSINumeric(string path1, ComparisonOperator op, double val) { Operand1Path = path1; Operand2Path = val.ToString(); Operator = op; }
        /// <summary>Creates a numeric game state lookup that returns a boolean depending on the given operator's comparison between the variable at path1 and the variable at path2.</summary>
        public BooleanGSINumeric(string path1, ComparisonOperator op, string path2) { Operand1Path = path1; Operand2Path = path2; Operator = op; }

        // Path to the two GSI variables (or numbers themselves) and the operator to compare them with
        public string Operand1Path { get; set; }
        public string Operand2Path { get; set; }
        public ComparisonOperator Operator { get; set; } = ComparisonOperator.EQ;

        // Control assigned to this condition
        [Newtonsoft.Json.JsonIgnore]
        private Control_ConditionGSINumeric control;
        public Visual GetControl(Application application) {
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
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Update the assigned control with the new application.</summary>
        public void SetApplication(Application application) {
            control?.SetApplication(application);

            // Check to ensure the variable paths are valid
            if (application != null && !double.TryParse(Operand1Path, out _) && !string.IsNullOrWhiteSpace(Operand1Path) && !application.ParameterLookup.ContainsKey(Operand1Path))
                Operand1Path = string.Empty;
            if (application != null && !double.TryParse(Operand2Path, out _) && !string.IsNullOrWhiteSpace(Operand2Path) && !application.ParameterLookup.ContainsKey(Operand2Path))
                Operand2Path = string.Empty;
        }

        public IEvaluatableBoolean Clone() => new BooleanGSINumeric { Operand1Path = Operand1Path, Operand2Path = Operand2Path, Operator = Operator };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
}