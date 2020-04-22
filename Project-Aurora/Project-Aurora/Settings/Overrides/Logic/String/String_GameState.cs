using Aurora.Controls;
using Aurora.Profiles;
using Aurora.Utils;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {

    [Evaluatable("String State Variable", category: EvaluatableCategory.State)]
    public class StringGSIString : IEvaluatable<string> {

        /// <summary>Path to the GSI variable</summary>
        public string VariablePath { get; set; } = "";

        /// <summary>Control assigned to this logic node.</summary>
        public Visual GetControl() => new GameStateParameterPicker { PropertyType = PropertyType.String }
            .WithBinding(GameStateParameterPicker.ApplicationProperty, new AttachedApplicationBinding())
            .WithBinding(GameStateParameterPicker.SelectedPathProperty, new Binding("VariablePath") { Source = this });

        /// <summary>Attempts to return the string at the given state variable.</summary>
        public string Evaluate(IGameState gameState) {
            if (VariablePath.Length > 0)
                try { return (string)Utils.GameStateUtils.RetrieveGameStateParameter(gameState, VariablePath); }
                catch { }
            return "";
        }
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Clones this StringGSIString.</summary>
        public IEvaluatable<string> Clone() => new StringGSIString { VariablePath = VariablePath };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
}
