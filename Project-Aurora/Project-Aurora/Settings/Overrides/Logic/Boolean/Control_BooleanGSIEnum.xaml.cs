
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {

    public partial class Control_BooleanGSIEnum : UserControl {

        public Control_BooleanGSIEnum(BooleanGSIEnum context) {
            InitializeComponent();
            ((FrameworkElement)Content).DataContext = context;
            UpdateEnumDropDown();
        }

        /// <summary>Updates the enum value dropdown with a list of enum values for the current application and selected variable path.</summary>
        private void UpdateEnumDropDown() {
            Type selectedEnumType = null;
            var application = Utils.AttachedApplication.GetApplication(this);
            var isValid = ((FrameworkElement)Content).DataContext is BooleanGSIEnum ctx
                && !string.IsNullOrWhiteSpace(ctx.StatePath) // If the path to the enum GSI isn't empty
                && application?.ParameterLookup != null // If the application parameter lookup is ready (and application isn't null)
                && application.ParameterLookup.ContainsKey(ctx.StatePath) // If the param lookup has the specified GSI key
                && (selectedEnumType = application.ParameterLookup[ctx.StatePath].Item1).IsEnum; // And the GSI variable is an enum type

            EnumVal.IsEnabled = isValid;
            EnumVal.ItemsSource = isValid ? Utils.EnumUtils.GetEnumItemsSource(selectedEnumType) : null;
        }

        // Update the enum dropdown when the user selects a different enum path
        private void GameStateParameterPicker_SelectedPathChanged(object sender, Controls.SelectedPathChangedEventArgs e) {
            UpdateEnumDropDown();
        }
    }
}
