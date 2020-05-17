using Aurora.Controls;
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
    [Evaluatable("Numeric State Variable", category: EvaluatableCategory.State)]
    public class NumberGSINumeric : Evaluatable<double> {

        /// <summary>Creates a new numeric game state lookup evaluatable that doesn't target anything.</summary>
        public NumberGSINumeric() { }
        /// <summary>Creates a new evaluatable that returns the game state variable at the given path.</summary>
        public NumberGSINumeric(string path) { VariablePath = path; }

        // Path to the GSI variable
        public string VariablePath { get; set; }

        // Control assigned to this evaluatable
        public override Visual GetControl() => new GameStateParameterPicker { PropertyType = GSIPropertyType.Number }
            .WithBinding(GameStateParameterPicker.ApplicationProperty, new AttachedApplicationBinding())
            .WithBinding(GameStateParameterPicker.SelectedPathProperty, new Binding("VariablePath") { Source = this });

        /// <summary>Parses the numbers, compares the result, and returns the result.</summary>
        protected override double Execute(IGameState gameState) => gameState.GetNumber(VariablePath);

        public override Evaluatable<double> Clone() => new NumberGSINumeric { VariablePath = VariablePath };
    }
}
