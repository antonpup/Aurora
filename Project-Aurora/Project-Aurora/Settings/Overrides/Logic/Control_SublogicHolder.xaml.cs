using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_SubconditionHolder.xaml
    /// </summary>
    public partial class Control_SubconditionHolder : UserControl {
        public Control_SubconditionHolder(IHasSubConditons parent, Profiles.Application application, string title="") {
            InitializeComponent();
            DataContext = new Control_SubconditionHolder_Context { Parent = parent, Application = application, Title = title };
        }

        private ObservableCollection<IEvaluatableBoolean> SubConditions => ((Control_SubconditionHolder_Context)DataContext).Parent.SubConditions;

        private void AddSubconditionButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            SubConditions.Add(new BooleanConstant());
        }

        // We cannot do a TwoWay binding on the items of an ObservableCollection if that item may be replaced (it would be fine if only the instance's
        // values were being changed), so we have to capture change events and replace them in the list.
        private void ConditionPresenter_ConditionChanged(object sender, ExpressionChangeEventArgs e) {
            SubConditions[SubConditions.IndexOf((IEvaluatableBoolean)e.OldExpression)] = (IEvaluatableBoolean)e.NewExpression;
        }

        private void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            // The DataContext of the clicked button (which is the sender) is the ICondition since
            // it is created for each item inside the ItemsControl items source.
            // We can then simply call remove on the conditions list to remove it.
            var cond = (IEvaluatableBoolean)((System.Windows.FrameworkElement)sender).DataContext;
            SubConditions.Remove(cond);
        }
    }

    /// <summary>
    /// The datatype that is used as the DataContext for `Control_SubconditionHolder`.
    /// </summary>
    internal class Control_SubconditionHolder_Context {
        public IHasSubConditons Parent { get; set; }
        public Profiles.Application Application { get; set; }
        public string Title { get; set; }
    }
}
