using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_EvaluatablePresenter.xaml
    /// </summary>
    public partial class Control_EvaluatablePresenter : UserControl {

        #region Fields
        /// <summary>Whether or not the type selection box is being programmatically set.</summary>
        private bool settingTypeSelection = false;
        #endregion

        /// <summary>
        /// Creates a new ExpressionPresenter. Use the properties/dependency objects to set the application, expression and evaltype.
        /// </summary>
        public Control_EvaluatablePresenter() {
            InitializeComponent();
            //UpdateExpressionListItems(typeof(IEvaluatable));
        }

        #region Events
        /// <summary>
        /// Event that is raised when the expression is changed and the parent should update it's reference to the expression.
        /// </summary>
        public event EventHandler<ExpressionChangeEventArgs> ExpressionChanged;
        #endregion

        #region Methods
        /// <summary>
        /// Re-creates the list collection view that is used for the expression selection list.
        /// </summary>
        private void UpdateExpressionListItems(EvaluatableType t) {
            ListCollectionView lcv = new ListCollectionView(EvaluatableRegistry
                .Get(EvaluatableTypeResolver.Resolve(t)) // Get all condition types that match the required type
                .OrderBy(kvp => (int)kvp.Value.Category) // Order them by the numeric value of the category (so they appear in the order specified)
                .ThenBy(kvp => kvp.Value.Name) // Then, order the items alphabetically in their category
                .ToList());
            lcv.GroupDescriptions.Add(new PropertyGroupDescription("Value.CategoryStr"));
            expressionSelection.ItemsSource = lcv;
        }

        // Fires when the expression selection combobox changes. Creates a new expression to replace the old one.
        private void ExpressionSelection_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (settingTypeSelection) return; // Do nothing if the source is NOT from the combobox being changed (we dont want to create a new instance when it's being set from Binding)
            var selectedType = (Type)expressionSelection.SelectedValue; // The SelectedValue (NOT SelectedItem) of the combobox references a Type that implements IEvaluatable.
            if (selectedType == null || Expression == null || selectedType == Expression.GetType()) return; // Do nothing if the types are the same.
            var newExpression = (IEvaluatable)Activator.CreateInstance(selectedType); // Create an instance from the selected type
            ExpressionChanged?.Invoke(this, new ExpressionChangeEventArgs { OldExpression = Expression, NewExpression = newExpression });
            Expression = newExpression;
        }

        /// <summary>
        /// Copies the current IEvaluatable to the clipboard
        /// </summary>
        private void CopyButton_Click(object sender, RoutedEventArgs e) {
            if (Expression != null)
                Global.Clipboard = Expression.Clone(); // We clone it so if the user changes the evaluatable after pressing copy, the changes don't effect the one on the clipboard.
        }

        /// <summary>
        /// Replaces the current IEvaluatable with the one on the clipboard
        /// </summary>
        private void PasteButton_Click(object sender, RoutedEventArgs e) {
            if (Global.Clipboard is IEvaluatable clipboardContents && MessageBox.Show("Are you sure you wish to REPLACE this expression with the one on your clipboard?", "Confirm paste", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                var @new = clipboardContents.Clone(); // We clone again when pasting so that if the user pastes it in two places, they aren't the same object
                ExpressionChanged?.Invoke(this, new ExpressionChangeEventArgs { OldExpression = Expression, NewExpression = @new });
                Expression = @new;
            }
        }
        #endregion

        #region Dependency Objects
        // Expression Property (the expression whose UserControl this component should host)
        private static void OnExpressionChange(DependencyObject evaluatablePresenter, DependencyPropertyChangedEventArgs eventArgs) {
            var control = (Control_EvaluatablePresenter)evaluatablePresenter;
            var expr = (IEvaluatable)eventArgs.NewValue;
            var x = expr?.GetType();
            control.ContentContainer.Content = expr?.GetControl(control.Application);
            control.settingTypeSelection = true;
            control.expressionSelection.SelectedValue = expr?.GetType();
            control.settingTypeSelection = false;
        }

        public static readonly DependencyProperty ExpressionProperty = DependencyProperty.Register("Expression", typeof(IEvaluatable), typeof(Control_EvaluatablePresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnExpressionChange));
        public IEvaluatable Expression {
            get => (IEvaluatable)GetValue(ExpressionProperty);
            set => SetValue(ExpressionProperty, value);
        }

        // Application Property (the application passed to the component's UserControl to allow it do detect GSI variables names and such)
        private static void OnApplicationChange(DependencyObject evaluatablePresenter, DependencyPropertyChangedEventArgs eventArgs) {
            var control = (Control_EvaluatablePresenter)evaluatablePresenter;
            control.Expression?.SetApplication((Profiles.Application)eventArgs.NewValue);
        }

        public static readonly DependencyProperty ApplicationProperty = DependencyProperty.Register("Application", typeof(Profiles.Application), typeof(Control_EvaluatablePresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnApplicationChange));
        public Profiles.Application Application {
            get => (Profiles.Application)GetValue(ApplicationProperty);
            set => SetValue(ApplicationProperty, value);
        }

        // The subtype of evaluatable to restrict the user to (e.g. IEvaluatableBoolean)
        private static void OnEvalTypeChange(DependencyObject evaluatablePresenter, DependencyPropertyChangedEventArgs eventArgs) {
            var control = (Control_EvaluatablePresenter)evaluatablePresenter;
            control.UpdateExpressionListItems((EvaluatableType)eventArgs.NewValue);
        }

        public static readonly DependencyProperty EvalTypeProperty = DependencyProperty.Register("EvalType", typeof(EvaluatableType), typeof(Control_EvaluatablePresenter), new FrameworkPropertyMetadata(EvaluatableType.All, FrameworkPropertyMetadataOptions.AffectsRender, OnEvalTypeChange));
        public EvaluatableType EvalType {
            get => (EvaluatableType)GetValue(EvalTypeProperty);
            set => SetValue(EvalTypeProperty, value);
        }
        #endregion
    }

    public class ExpressionChangeEventArgs : EventArgs {
        public IEvaluatable OldExpression { get; set; }
        public IEvaluatable NewExpression { get; set; }
    }
}
