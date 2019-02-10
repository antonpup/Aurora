using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for ConditionPresenter.xaml
    /// </summary>
    public partial class Control_EvaluatablePresenter : UserControl {

        /// <summary>
        /// Event that is raised when the condition is changed and the parent should update it's reference to the condition.
        /// </summary>
        public event EventHandler<ConditionChangeEventArgs> ConditionChanged;

        /// <summary>Whether or not the type selection box is being programmatically set.</summary>
        private bool settingTypeSelection = false;

        /// <summary>
        /// Creates a new ConditionPresenter. Use the properties/dependency objects to set the application and condition.
        /// </summary>
        public Control_EvaluatablePresenter() {
            InitializeComponent();

            ListCollectionView lcv = new ListCollectionView(EvaluatableRegistry.Get<IEvaluatableBoolean>().OrderBy(kvp => (int)kvp.Value.Category).ThenBy(kvp => kvp.Value.Name).ToList());
            lcv.GroupDescriptions.Add(new PropertyGroupDescription("Value.CategoryStr"));
            ConditionSelection.ItemsSource = lcv;
        }

        // Fires when the condition selection combobox changes. Creates a new condition to replace the old one.
        private void ConditionSelection_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (settingTypeSelection) return; // Do nothing if the source is NOT from the combobox being changed (we dont want to create a new instance when it's being set from Binding)
            var selectedType = (Type)ConditionSelection.SelectedValue; // The SelectedValue (NOT SelectedItem) of the combobox references a Type that implements ICondition.
            if (selectedType == null || Condition == null || selectedType == Condition.GetType()) return; // Do nothing if the types are the same.
            var newCondition = (IEvaluatableBoolean)Activator.CreateInstance(selectedType); // Create an instance from the selected type
            ConditionChanged?.Invoke(this, new ConditionChangeEventArgs { OldCondition = Condition, NewCondition = newCondition });
            Condition = newCondition;
        }

        /// <summary>
        /// Copies the current IEvaluatable to the clipboard
        /// </summary>
        private void CopyButton_Click(object sender, RoutedEventArgs e) {
            if (Condition != null)
                Global.Clipboard = Condition.Clone(); // We clone it so if the user changes the evaluatable after pressing copy, the changes don't effect the one on the clipboard.
        }

        /// <summary>
        /// Replaces the current IEvaluatable with the one on the clipboard
        /// </summary>
        private void PasteButton_Click(object sender, RoutedEventArgs e) {
            if (Global.Clipboard is IEvaluatableBoolean clipboardContents && MessageBox.Show("Are you sure you wish to REPLACE this expression with the one on your clipboard?", "Confirm paste", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                var @new = clipboardContents.Clone(); // We clone again when pasting so that if the user pastes it in two places, they aren't the same object
                ConditionChanged?.Invoke(this, new ConditionChangeEventArgs { OldCondition = Condition, NewCondition = @new });
                Condition = @new;
            }
        }

        #region Dependency Objects
        // Condition Property (the condition whose UserControl this component should host)
        private static void OnConditionChange(DependencyObject conditionPresenter, DependencyPropertyChangedEventArgs eventArgs) {
            var control = (Control_EvaluatablePresenter)conditionPresenter;
            var cond = (IEvaluatableBoolean)eventArgs.NewValue;
            var x = cond?.GetType();
            control.ContentContainer.Content = cond?.GetControl(control.Application);
            control.settingTypeSelection = true;
            control.ConditionSelection.SelectedValue = cond?.GetType();
            control.settingTypeSelection = false;
        }

        public static readonly DependencyProperty ConditionProperty = DependencyProperty.Register("Condition", typeof(IEvaluatableBoolean), typeof(Control_EvaluatablePresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnConditionChange));
        public IEvaluatableBoolean Condition {
            get => (IEvaluatableBoolean)GetValue(ConditionProperty);
            set => SetValue(ConditionProperty, value);
        }

        // Application Property (the application passed to the component's UserControl to allow it do detect GSI variables names and such)
        private static void OnApplicationChange(DependencyObject conditionPresenter, DependencyPropertyChangedEventArgs eventArgs) {
            var control = (Control_EvaluatablePresenter)conditionPresenter;
            control.Condition?.SetApplication((Profiles.Application)eventArgs.NewValue);
        }

        public static readonly DependencyProperty ApplicationProperty = DependencyProperty.Register("Application", typeof(Profiles.Application), typeof(Control_EvaluatablePresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnApplicationChange));
        public Profiles.Application Application {
            get => (Profiles.Application)GetValue(ApplicationProperty);
            set => SetValue(ApplicationProperty, value);
        }
        #endregion
    }

    public class ConditionChangeEventArgs : EventArgs {
        public IEvaluatable OldCondition { get; set; }
        public IEvaluatable NewCondition { get; set; }
    }
}
