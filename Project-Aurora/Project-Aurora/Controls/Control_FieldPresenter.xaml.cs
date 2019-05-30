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

            // Create a control to handle this type:-
            if (TypeControlMap.ContainsKey(t)) {
                // If there is a defined control (in TypeControlMap) for this type, use that control
                control.SubControl.Content = TypeControlMap[t].Invoke(new Binding("Value") { Source = control });

            } else if (t.IsEnum) {
                // If there is no predifined type but the type to edit is an enum, create a combobox containing all values of that enum
                control.SubControl.Content = new ComboBox {
                    DisplayMemberPath = "Label", SelectedValuePath = "Value",
                    ItemsSource = t.GetEnumValues().Cast<object>().Select(@enum => new {
                        Label = t.GetMember(@enum.ToString()).FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? @enum.ToString(),
                        Value = @enum
                    })
                }.SetBindingChain(ComboBox.SelectedValueProperty, new Binding("Value") { Source = control });

            } else
                // If there is no predefined type and the type is not an enum, we don't know what to do
                control.SubControl.Content = new TextBlock { Text = "Field type not supported.", Foreground = Brushes.Red };
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

            // Boolean
            { typeof(bool), bind => new CheckBox().SetBindingChain(CheckBox.IsCheckedProperty, bind) },

            // String
            { typeof(string), bind => new TextBox().SetBindingChain(TextBox.TextProperty, bind) },

            // Numbers
            { typeof(int), bind => new IntegerUpDown().SetBindingChain(IntegerUpDown.ValueProperty, bind) },
            { typeof(long), bind => new LongUpDown().SetBindingChain(LongUpDown.ValueProperty, bind) },
            { typeof(double), bind => new DoubleUpDown().SetBindingChain(DoubleUpDown.ValueProperty, bind) },
            { typeof(float), bind => new SingleUpDown{ Increment = .1f }.SetBindingChain(SingleUpDown.ValueProperty, bind) },

            // Colours
            { typeof(System.Drawing.Color), bind => new ColorPicker{ ColorMode = ColorMode.ColorCanvas }.SetBindingChain(ColorPicker.SelectedColorProperty, bind, new Utils.ColorConverter()) },
            { typeof(RealColor), bind => new ColorPicker{ ColorMode = ColorMode.ColorCanvas }.SetBindingChain(ColorPicker.SelectedColorProperty, bind, new RealColorConverter()) },

            // Gradient colour
            { typeof(Settings.LayerEffectConfig), bind => new Control_GradientEditor((Settings.LayerEffectConfig)((Control_FieldPresenter)bind.Source).Value) },
            { typeof(EffectsEngine.EffectBrush), bind => new ColorBox.ColorBox().SetBindingChain(ColorBox.ColorBox.BrushProperty, bind, new EffectBrushToBrushConverter(), BindingMode.TwoWay) },

            // KeySequences
            { typeof(Settings.KeySequence), bind => new Controls.KeySequence {
                    Title = "Assigned Keys",
                    RecordingTag = "FieldPresenterKeySequence",
                    Height = 120,
                }.SetBindingChain(Controls.KeySequence.SequenceProperty, bind, bindingMode: BindingMode.TwoWay)
            },

            // Single key inputs
            { typeof(System.Windows.Forms.Keys), bind => new Control_SingleKeyEditor().SetBindingChain(Control_SingleKeyEditor.SelectedKeyProperty, bind, bindingMode: BindingMode.TwoWay) }
        };
    }
    
    static class FrameworkElementExtension {
        /// <summary>
        /// Tiny extension for the FrameworkElement that allows to set a binding on an element and return that element (so it can be chained).
        /// Used in the TypeControlMap to shorten the code.
        /// </summary>
        public static FrameworkElement SetBindingChain(this FrameworkElement self, DependencyProperty dp, Binding binding, IValueConverter converter = null, BindingMode? bindingMode = null) {
            if (converter != null)
                binding.Converter = converter;
            if (bindingMode.HasValue)
                binding.Mode = bindingMode.Value;
            self.SetBinding(dp, binding);
            return self;
        }
    }
}
