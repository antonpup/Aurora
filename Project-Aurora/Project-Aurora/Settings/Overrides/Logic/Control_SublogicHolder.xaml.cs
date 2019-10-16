using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_SubconditionHolder.xaml
    /// </summary>
    public partial class Control_SubconditionHolder : UserControl {
        public Control_SubconditionHolder(IHasSubConditons parent, Profiles.Application app, string description="") {
            InitializeComponent();
            ParentExpr = parent;
            Application = app;
            Description = description;
            ((FrameworkElement)Content).DataContext = this;
        }

        private void AddSubconditionButton_Click(object sender, RoutedEventArgs e) {
            ParentExpr.SubConditions.Add(new BooleanConstant());
        }

        // We cannot do a TwoWay binding on the items of an ObservableCollection if that item may be replaced (it would be fine if only the instance's
        // values were being changed), so we have to capture change events and replace them in the list.
        private void ConditionPresenter_ConditionChanged(object sender, ExpressionChangeEventArgs e) {
            ParentExpr.SubConditions[ParentExpr.SubConditions.IndexOf((IEvaluatable<bool>)e.OldExpression)] = (IEvaluatable<bool>)e.NewExpression;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            // The DataContext of the clicked button (which is the sender) is the ICondition since
            // it is created for each item inside the ItemsControl items source.
            // We can then simply call remove on the conditions list to remove it.
            var cond = (IEvaluatable<bool>)((FrameworkElement)sender).DataContext;
            ParentExpr.SubConditions.Remove(cond);
        }

        #region Properties/Dependency Properties
        /// <summary>The parent evaluatable of this control. Must be an evaluatable that has sub conditions.</summary>
        public IHasSubConditons ParentExpr { get; }

        /// <summary>The title/description text of this control.</summary>
        public string Description { get; }

        /// <summary>The application context of this control. Is passed to the EvaluatablePresenter children.</summary>
        public Profiles.Application Application {
            get => (Profiles.Application)GetValue(ApplicationProperty);
            set => SetValue(ApplicationProperty, value);
        }
        
        /// <summary>The property used as a backing store for the application context.</summary>
        public static readonly DependencyProperty ApplicationProperty =
            DependencyProperty.Register("Application", typeof(Profiles.Application), typeof(Control_SubconditionHolder), new PropertyMetadata(null));
        #endregion
    }
}
