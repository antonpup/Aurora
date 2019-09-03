using Aurora.Profiles;

namespace Aurora.Settings.Overrides.Logic
{

    /// <summary>
    /// Evaluatable that accesses some specified game state variables (of numeric type) and returns it.
    /// </summary>
    [OverrideLogic("Numeric State Variable", category: OverrideLogicCategory.State)]
    public class NumberGSINumeric : IEvaluatable<double>
    {

        /// <summary>Creates a new numeric game state lookup evaluatable that doesn't target anything.</summary>
        public NumberGSINumeric() { }
        /// <summary>Creates a new evaluatable that returns the game state variable at the given path.</summary>
        public NumberGSINumeric(string path) { VariablePath = path; }

        // Path to the GSI variable
        public string VariablePath { get; set; }

        /// <summary>Parses the numbers, compares the result, and returns the result.</summary>
        public double Evaluate(IGameState gameState) => Utils.GameStateUtils.TryGetDoubleFromState(gameState, VariablePath);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Update the assigned control with the new application.</summary>
        public void SetApplication(Application application)
        {
            // Check to ensure the variable path is valid
            if (application != null && !double.TryParse(VariablePath, out _) && !string.IsNullOrWhiteSpace(VariablePath) && !application.ParameterLookup.ContainsKey(VariablePath))
                VariablePath = string.Empty;
        }

        public IEvaluatable<double> Clone() => new NumberGSINumeric { VariablePath = VariablePath };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
}
