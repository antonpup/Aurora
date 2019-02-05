using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Aurora.Controls {

    public partial class Control_FieldPresenter : UserControl {

        public Control_FieldPresenter() {
            InitializeComponent();
        }

        #region Dependency Objects
        // The type of the field to edit
        private static void OnTypeChange(DependencyObject fieldPresenter, DependencyPropertyChangedEventArgs eventArgs) {
            var control = (Control_FieldPresenter)fieldPresenter;
            var t = (Type)eventArgs.NewValue;

            if (t.IsEnum) {
                // If the type to edit is an enum, create a combobox containing all values of that enum
                var cb = new ComboBox { DisplayMemberPath = "Label", SelectedValuePath = "Value" };
                cb.ItemsSource = t.GetEnumValues().Cast<object>().Select(@enum => new {
                    Label = t.GetMember(@enum.ToString()).FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? @enum.ToString(),
                    Value = @enum
                });
                cb.SetBinding(ComboBox.SelectedValueProperty, new Binding("Value") { Source = control });
                control.SubControl.Content = cb;

            } else
                // Else, see if there is a control defined in the TypeControlMap, use that control, otherwise tell the user they cannot edit this type
                control.SubControl.Content = TypeControlMap.ContainsKey(t)
                    ? TypeControlMap[t].Invoke(new Binding("Value") { Source = control })
                    : new TextBlock { Text = "Field type not supported.", Foreground = Brushes.Red };
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(Type), typeof(Control_FieldPresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnTypeChange));
        public Type Type {
            get => (Type)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        // The value of the field to edit
        private static void OnValueChange(DependencyObject fieldPresenter, DependencyPropertyChangedEventArgs eventArgs) {
            var control = (Control_FieldPresenter)fieldPresenter;
            // Do something?
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(Control_FieldPresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnValueChange));
        public object Value {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        #endregion

        /// <summary><para>Dictionary that contains constructors for the types that this presenter can handle.</para>
        /// The value (Func&lt;Binding, Visual&gt;) is a method which takes a newly created Binding object that should
        /// be used as the control's value's binding and should return the control to display to the user.</summary>
        private static Dictionary<Type, Func<Binding, Visual>> TypeControlMap = new Dictionary<Type, Func<Binding, Visual>> {

            { typeof(bool), bind => {
                var cb = new CheckBox();
                cb.SetBinding(CheckBox.IsCheckedProperty, bind);
                return cb;
            } },

            { typeof(string), bind => {
                var tb = new TextBox();
                tb.SetBinding(TextBox.TextProperty, bind);
                return tb;
            } },

            { typeof(int), bind => {
                var stepper = new IntegerUpDown();
                stepper.SetBinding(IntegerUpDown.ValueProperty, bind);
                return stepper;
            } },

            { typeof(long), bind => {
                var stepper = new LongUpDown();
                stepper.SetBinding(LongUpDown.ValueProperty, bind);
                return stepper;
            } },

            { typeof(double), bind => {
                var stepper = new DoubleUpDown();
                stepper.SetBinding(DoubleUpDown.ValueProperty, bind);
                return stepper;
            } },

            { typeof(float), bind => {
                var stepper = new SingleUpDown();
                stepper.SetBinding(SingleUpDown.ValueProperty, bind);
                return stepper;
            } },

            { typeof(System.Drawing.Color), bind => {
                var picker = new ColorPicker{ ColorMode = ColorMode.ColorCanvas };
                bind.Converter = new Utils.ColorConverter();
                picker.SetBinding(ColorPicker.SelectedColorProperty, bind);
                return picker;
            } },

            { typeof(RealColor), bind => {
                var picker = new ColorPicker{ ColorMode = ColorMode.ColorCanvas };
                bind.Converter = new RealColorConverter();
                picker.SetBinding(ColorPicker.SelectedColorProperty, bind);
                return picker;
            } }
        };
    }
}
