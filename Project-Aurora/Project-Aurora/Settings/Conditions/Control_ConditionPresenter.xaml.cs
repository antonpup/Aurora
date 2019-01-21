﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Settings.Conditions {
    /// <summary>
    /// Interaction logic for ConditionPresenter.xaml
    /// </summary>
    public partial class Control_ConditionPresenter : UserControl {

        /// <summary>
        /// Event that is raised when the condition is changed and the parent should update it's reference to the condition.
        /// </summary>
        public event EventHandler<ConditionChangeEventArgs> ConditionChanged;

        /// <summary>Whether or not the type selection box is being programmatically set.</summary>
        private bool settingTypeSelection = false;
              
        /// <summary>
        /// Creates a new ConditionPresenter. Use the properties/dependency objects to set the application and condition.
        /// </summary>
        public Control_ConditionPresenter() {
            InitializeComponent();
            ConditionSelection.ItemsSource = ConditionRegistry;
        }

        // List of all conditions that should appear in the dropdown lists.
        // Generated dynamically from any class with the [ConditionAttribute] custom attribute.
        private static Dictionary<Type, ConditionAttribute> _conditionRegistry;
        public static Dictionary<Type, ConditionAttribute> ConditionRegistry {
            get {
                if (_conditionRegistry == null)
                    _conditionRegistry = Utils.TypeUtils.GetTypesWithCustomAttribute<ConditionAttribute>().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                return _conditionRegistry;
            }
        }

        // Fires when the condition selection combobox changes. Creates a new condition to replace the old one.
        private void ConditionSelection_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (settingTypeSelection) return; // Do nothing if this is programmatically being set.
            var selectedType = (Type)ConditionSelection.SelectedValue; // The SelectedValue (NOT SelectedItem) of the combobox references a Type that implements ICondition.
            if (selectedType == null || Condition == null || selectedType == Condition.GetType()) return; // Do nothing if the types are the same.
            var newCondition = (ICondition)Activator.CreateInstance(selectedType); // Create an instance from the selected type
            ConditionChanged?.Invoke(this, new ConditionChangeEventArgs { OldCondition = Condition, NewCondition = newCondition });
            Condition = newCondition;
        }

        #region Dependency Objects
        // Condition Property (the condition whose UserControl this component should host)
        private static void OnConditionChange(DependencyObject conditionPresenter, DependencyPropertyChangedEventArgs eventArgs) {
            var control = (Control_ConditionPresenter)conditionPresenter;
            var cond = (ICondition)eventArgs.NewValue;
            var x = cond?.GetType();
            control.ContentContainer.Content = cond?.GetControl(control.Application);
            control.settingTypeSelection = true;
            control.ConditionSelection.SelectedValue = cond?.GetType();
            control.settingTypeSelection = false;
        }

        public static readonly DependencyProperty ConditionProperty = DependencyProperty.Register("Condition", typeof(ICondition), typeof(Control_ConditionPresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnConditionChange));
        public ICondition Condition {
            get => (ICondition)GetValue(ConditionProperty);
            set => SetValue(ConditionProperty, value);
        }

        // Application Property (the application passed to the component's UserControl to allow it do detect GSI variables names and such)
        private static void OnApplicationChange(DependencyObject conditionPresenter, DependencyPropertyChangedEventArgs eventArgs) {
            var control = (Control_ConditionPresenter)conditionPresenter;
            control.Condition?.SetApplication((Profiles.Application)eventArgs.NewValue);
        }

        public static readonly DependencyProperty ApplicationProperty = DependencyProperty.Register("Application", typeof(Profiles.Application), typeof(Control_ConditionPresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnApplicationChange));
        public Profiles.Application Application {
            get => (Profiles.Application)GetValue(ApplicationProperty);
            set => SetValue(ApplicationProperty, value);
        }
        #endregion
    }

    public class ConditionChangeEventArgs : EventArgs {
        public ICondition OldCondition { get; set; }
        public ICondition NewCondition { get; set; }
    }
}