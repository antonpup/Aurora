using Aurora.Profiles;

namespace Aurora.Settings.Overrides.Logic
{

    [OverrideLogic("String State Variable", category: OverrideLogicCategory.State)]
    public class StringGSIString : IEvaluatable<string>
    {

        /// <summary>Path to the GSI variable</summary>
        public string VariablePath { get; set; } = "";

        /// <summary>Attempts to return the string at the given state variable.</summary>
        public string Evaluate(IGameState gameState)
        {
            if (VariablePath.Length > 0)
                try { return (string)Utils.GameStateUtils.RetrieveGameStateParameter(gameState, VariablePath); }
                catch { }
            return "";
        }
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Update the assigned combobox with the new application context.</summary>
        public void SetApplication(Application application)
        {
            // Check to ensure var path is valid
            if (application != null && !string.IsNullOrWhiteSpace(VariablePath) && !application.ParameterLookup.ContainsKey(VariablePath))
                VariablePath = string.Empty;
        }

        /// <summary>Clones this StringGSIString.</summary>
        public IEvaluatable<string> Clone() => new StringGSIString { VariablePath = VariablePath };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
}
