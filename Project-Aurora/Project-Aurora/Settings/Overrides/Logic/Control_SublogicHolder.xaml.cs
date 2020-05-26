using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Interaction logic for Control_SubconditionHolder.xaml
    /// </summary>
    public partial class Control_SubconditionHolder : UserControl {

        public Control_SubconditionHolder(IHasSubConditons parent, string description = "") {
            InitializeComponent();
            DataContext = Context = parent;
            Description = description;
        }

        /// <summary>The parent evaluatable of this control. Must be an evaluatable that has sub conditions.</summary>
        public IHasSubConditons Context { get; }

        /// <summary>The title/description text of this control.</summary>
        public string Description { get; }

        private void AddSubconditionButton_Click(object sender, RoutedEventArgs e) {
            Context.SubConditions.Add(new BooleanConstant());
        }

        // We cannot do a TwoWay binding on the items of an ObservableCollection if that item may be replaced (it would be fine if only the instance's
        // values were being changed), so we have to capture change events and replace them in the list.
        private void ConditionPresenter_ConditionChanged(object sender, ExpressionChangeEventArgs e) {
            Context.SubConditions[Context.SubConditions.IndexOf((Evaluatable<bool>)e.OldExpression)] = (Evaluatable<bool>)e.NewExpression;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            // The DataContext of the clicked button (which is the sender) is the ICondition since
            // it is created for each item inside the ItemsControl items source.
            // We can then simply call remove on the conditions list to remove it.
            var cond = (Evaluatable<bool>)((FrameworkElement)sender).DataContext;
            Context.SubConditions.Remove(cond);
        }
    }
}
