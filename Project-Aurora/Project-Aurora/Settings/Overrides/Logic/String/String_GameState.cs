using Aurora.Profiles;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {

    [OverrideLogic("String State Variable", category: OverrideLogicCategory.State)]
    public class StringGSIString : IEvaluatableString {

        /// <summary>Path to the GSI variable</summary>
        public string VariablePath { get; set; } = "";

        /// <summary>Control assigned to this logic node.</summary>
        [Newtonsoft.Json.JsonIgnore]
        private ComboBox control;
        public Visual GetControl(Application application) {
            if (control == null) {
                control = new ComboBox { Margin = new System.Windows.Thickness(0, 0, 0, 6) };
                control.SetBinding(ComboBox.SelectedItemProperty, new Binding("VariablePath") { Source = this });
                SetApplication(application);
            }
            return control;
        }

        /// <summary>Attempts to return the string at the given state variable.</summary>
        public string Evaluate(IGameState gameState) {
            if (VariablePath.Length > 0)
                try { return (string)Utils.GameStateUtils.RetrieveGameStateParameter(gameState, VariablePath); }
                catch { }
            return "";
        }
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Update the assigned combobox with the new application context.</summary>
        public void SetApplication(Application application) {
            if (control != null)
                control.ItemsSource = application?.ParameterLookup?
                    .Where(kvp => Type.GetTypeCode(kvp.Value.Item1) == TypeCode.String)
                    .Select(kvp => kvp.Key);

            // Check to ensure var path is valid
            if (application != null && !string.IsNullOrWhiteSpace(VariablePath) && !application.ParameterLookup.ContainsKey(VariablePath))
                VariablePath = string.Empty;
        }

        /// <summary>Clones this StringGSIString.</summary>
        public IEvaluatableString Clone() => new StringGSIString { VariablePath = VariablePath };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
}
