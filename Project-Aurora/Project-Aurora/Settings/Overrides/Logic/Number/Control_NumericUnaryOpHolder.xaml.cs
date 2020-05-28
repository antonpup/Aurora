using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Aurora.Profiles;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_NumericUnaryOpHolder.xaml
    /// </summary>
    public partial class Control_NumericUnaryOpHolder : UserControl {

        /// <summary>Base constructor for the unary operation holder.</summary>
        public Control_NumericUnaryOpHolder() {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>Creates a new unary operation control using the given Enum type as a source for the possible operators to choose from.</summary>
        public Control_NumericUnaryOpHolder(Type enumType) : this() {
            OperatorList = Utils.EnumUtils.GetEnumItemsSource(enumType).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>Creates a new unary opeartion control using the given string as the name of the operator, disallowing the user to choose an option.</summary>
        public Control_NumericUnaryOpHolder(string operatorName) : this() {
            StaticOperator = operatorName;
        }

        public Dictionary<string, object> OperatorList { get; set; } = null;
        public string StaticOperator { get; set; } = null;

        /// <summary>The dependency property that can be used to access the single operand for this operation.</summary>
        public static readonly DependencyProperty OperandProperty = DependencyProperty.Register("Operand", typeof(Evaluatable<double>), typeof(Control_NumericUnaryOpHolder), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public Evaluatable<double> Operand {
            get => (Evaluatable<double>)GetValue(OperandProperty);
            set => SetValue(OperandProperty, value);
        }

        /// <summary>If the unary operation holder allows for an operator selection, this is the dependency property that can be used to access that selection.</summary>
        public static readonly DependencyProperty SelectedOperatorProperty = DependencyProperty.Register("SelectedOperator", typeof(object), typeof(Control_NumericUnaryOpHolder), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public object SelectedOperator {
            get => GetValue(SelectedOperatorProperty);
            set => SetValue(SelectedOperatorProperty, value);
        }
    }
}
