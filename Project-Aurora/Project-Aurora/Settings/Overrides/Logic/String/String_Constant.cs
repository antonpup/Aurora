using Aurora.Profiles;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Represents a constant string value that will always evaluate to the same value.
    /// </summary>
    [OverrideLogic("String Constant", category: OverrideLogicCategory.String)]
    public class StringConstant : IEvaluatableString {

        /// <summary>The value of the constant.</summary>
        public string Value { get; set; } = "";

        /// <summary>A control for setting the string value</summary>
        [Newtonsoft.Json.JsonIgnore]
        private TextBox control;
        public Visual GetControl(Application application) {
            if (control == null) {
                control = new TextBox { Margin = new System.Windows.Thickness(0, 0, 0, 6) };
                control.SetBinding(TextBox.TextProperty, new Binding("Value") { Source = this, Mode = BindingMode.TwoWay });
            }
            return control;
        }

        /// <summary>Simply return the constant value.</summary>
        public string Evaluate(IGameState gameState) => Value;
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Does nothing. This is an application-independent evaluatable.</summary>
        public void SetApplication(Application application) { }

        /// <summary>Clones this constant string value.</summary>
        public IEvaluatableString Clone() => new StringConstant { Value = Value };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
}
