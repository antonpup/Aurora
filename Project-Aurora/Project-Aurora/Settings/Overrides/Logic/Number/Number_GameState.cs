using Aurora.Profiles;
using Aurora.Utils;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Evaluatable that accesses some specified game state variables (of numeric type) and returns it.
    /// </summary>
    [OverrideLogic("Numeric State Variable", category: OverrideLogicCategory.State)]
    public class NumberGSINumeric : IEvaluatableNumber {

        /// <summary>Creates a new numeric game state lookup evaluatable that doesn't target anything.</summary>
        public NumberGSINumeric() { }
        /// <summary>Creates a new evaluatable that returns the game state variable at the given path.</summary>
        public NumberGSINumeric(string path) { VariablePath = path; }

        // Path to the GSI variable
        public string VariablePath { get; set; }

        // Control assigned to this evaluatable
        [Newtonsoft.Json.JsonIgnore]
        private Controls.Control_GameStateParameterPicker control;
        public Visual GetControl(Application application) {
            if (control == null) {
                control = new Controls.Control_GameStateParameterPicker { PropertyType = PropertyType.Number, Margin = new System.Windows.Thickness(0, 0, 0, 6) };
                control.SetBinding(Controls.Control_GameStateParameterPicker.SelectedPathProperty, new Binding("VariablePath") { Source = this, Mode = BindingMode.TwoWay });
                SetApplication(application);
            }
            return control;
        }

        /// <summary>Parses the numbers, compares the result, and returns the result.</summary>
        public double Evaluate(IGameState gameState) => Utils.GameStateUtils.TryGetDoubleFromState(gameState, VariablePath);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Update the assigned control with the new application.</summary>
        public void SetApplication(Application application) {
            if (control != null)
                control.Application = application;

            // Check to ensure the variable path is valid
            if (application != null && !double.TryParse(VariablePath, out _) && !string.IsNullOrWhiteSpace(VariablePath) && !application.ParameterLookup.IsValidParameter(VariablePath))
                VariablePath = string.Empty;
        }

        public IEvaluatableNumber Clone() => new NumberGSINumeric { VariablePath = VariablePath };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
}
