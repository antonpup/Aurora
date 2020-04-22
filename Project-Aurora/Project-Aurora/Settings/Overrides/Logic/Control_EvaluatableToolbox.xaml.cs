using Aurora.Controls;
using Aurora.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static Aurora.Settings.Overrides.Logic.EvaluatableHelpers;

namespace Aurora.Settings.Overrides.Logic {

    public partial class Control_EvaluatableToolbox : UserControl {

        public Control_EvaluatableToolbox() {
            InitializeComponent();
        }

        /// <summary>Returns a list of all detected evaluatables grouped by their category.</summary>
        public IEnumerable<IGrouping<EvaluatableCategory, EvaluatableRegistry.EvaluatableTypeContainer>> GroupedEvalutables =>
            EvaluatableRegistry.Get().GroupBy(x => x.Metadata.Category);

        /// <summary>Property that forwards the evaluatable template store from the global config.</summary>
        public ObservableConcurrentDictionary<string, IEvaluatable> TemplateSource => Global.Configuration.EvaluatableTemplates;


        #region New Evaluatable Spawning
        /// <summary>Handler for when the user drags a new evalutable from the list of available evaluatables.</summary>
        private void EvaluatableSpawnerItem_StartDrag(object sender, MouseEventArgs e, Point initial) {
            var dc = (EvaluatableRegistry.EvaluatableTypeContainer)((FrameworkElement)sender).DataContext;
            DragDrop.DoDragDrop(this, Activator.CreateInstance(dc.Evaluatable), DragDropEffects.Move);
        }
        #endregion


        #region Evaluatable Templates
        /// <summary>Handler for when the user drags an evaluatable out from their list of saved evaluatables.</summary>
        private void EvaluatableTemplateItem_StartDrag(object sender, MouseEventArgs e, Point initial) {
            var dc = (KeyValuePair<string, IEvaluatable>)((FrameworkElement)sender).DataContext;
            DragDrop.DoDragDrop(this, dc.Value.Clone(), DragDropEffects.Move);
        }

        private void EvaluatableTemplateList_DragEnter(object sender, DragEventArgs e) {
            // Check for an evaluatable in the drag data. Check also the source is non-null (i.e. check it
            // must have come from a placed evaluatable, not from the toolbox or template list).
            if (TryGetData(e.Data, out _, out var source, null) && source != null) {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            } else {
                e.Effects = DragDropEffects.None;
            }
        }

        private void EvaluatableTemplateList_Drop(object sender, DragEventArgs e) {
            // Check for an evaluatable in the drag data. Check also the source is non-null (i.e. check it
            // must have come from a placed evaluatable, not from the toolbox or template list).
            if (TryGetData(e.Data, out var eval, out var source, null) && source != null && AskUserForName(out var newName))
                TemplateSource[newName] = eval.Clone();
        }

        /// <summary>Asks the user to enter a name for an evaluatable. Returns whether successful (false = user cancel).</summary>
        private bool AskUserForName(out string name) {
            var confirmName = "";
            var wnd = new Window_Prompt {
                Description = "Enter a name to save this evaluatable as a template."
            };
            wnd.Validate = n => {
                // Check name isn't empty
                if (string.IsNullOrWhiteSpace(n)) {
                    wnd.ErrorMessage = "Name cannot be empty or only consist of whitespace.";
                    return false;

                // If an evaluatable with this name already exists, make the user confirm by clicking okay again.
                } else if (TemplateSource.ContainsKey(n) && n != confirmName) {
                    wnd.ErrorMessage = "A template with this name already exists. Click 'Okay' again to overwrite it.";
                    confirmName = n;
                    return false;
                }

                return true;
            };

            var res = wnd.ShowDialog() == true;
            name = wnd.Text;
            return res;
        }
        #endregion
    }


    /// <summary>
    /// A more-specialised version of the <see cref="EvaluatableBackgroundSelector"/> that deals with selecting the relevant
    /// background color for an evaluatable template.
    /// </summary>
    public class TemplateEvaluatableBackgroundSelector : EvaluatableBackgroundSelector {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            base.Convert(value.GetType().GetGenericInterfaceTypes(typeof(IEvaluatable<>))[0], targetType, parameter, culture);
    }
}
