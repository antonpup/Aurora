using Aurora.Profiles;

namespace Aurora.Settings.Overrides.Logic
{
    /// <summary>
    /// Represents a constant string value that will always evaluate to the same value.
    /// </summary>
    [OverrideLogic("String Constant", category: OverrideLogicCategory.String)]
    public class StringConstant : IEvaluatable<string>
    {

        /// <summary>The value of the constant.</summary>
        public string Value { get; set; } = "";

        /// <summary>Simply return the constant value.</summary>
        public string Evaluate(IGameState gameState) => Value;
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Does nothing. This is an application-independent evaluatable.</summary>
        public void SetApplication(Application application) { }

        /// <summary>Clones this constant string value.</summary>
        public IEvaluatable<string> Clone() => new StringConstant { Value = Value };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
}
