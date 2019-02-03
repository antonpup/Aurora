using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Aurora.Settings.Conditions {
    /// <summary>
    /// Interaction logic for Control_SubconditionHolder.xaml
    /// </summary>
    public partial class Control_SubconditionHolder : UserControl {
        public Control_SubconditionHolder(ObservableCollection<ICondition> subconditions, Profiles.Application application, string title="") {
            InitializeComponent();
            DataContext = new Control_SubconditionHolder_Context { SubConditions = subconditions, Application = application, Title = title };
        }

        private ObservableCollection<ICondition> SubConditions => ((Control_SubconditionHolder_Context)DataContext).SubConditions;

        private void AddSubconditionButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            SubConditions.Add(new ConditionTrue());
        }

        private void ConditionPresenter_ConditionChanged(object sender, ConditionChangeEventArgs e) {
            SubConditions[SubConditions.IndexOf(e.OldCondition)] = e.NewCondition;
        }

        private void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            // The DataContext of the clicked button (which is the sender) is the ICondition since
            // it is created for each item inside the ItemsControl items source.
            // We can then simply call remove on the conditions list to remove it.
            var cond = (ICondition)((System.Windows.FrameworkElement)sender).DataContext;
            SubConditions.Remove(cond);
        }
    }

    /// <summary>
    /// The datatype that is used as the DataContext for `Control_SubconditionHolder`.
    /// </summary>
    internal class Control_SubconditionHolder_Context {
        public ObservableCollection<ICondition> SubConditions { get; set; }
        public Profiles.Application Application { get; set; }
        public string Title { get; set; }
    }
}
