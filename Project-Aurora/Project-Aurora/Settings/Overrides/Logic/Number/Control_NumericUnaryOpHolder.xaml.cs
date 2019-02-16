using System;
using System.ComponentModel;
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
        public Control_NumericUnaryOpHolder(Profiles.Application application) {
            InitializeComponent();
            Application = application;
            DataContext = this;
        }

        public Control_NumericUnaryOpHolder(Profiles.Application application, Type enumType) : this(application) {
            operatorSelection.ItemsSource = Enum.GetValues(enumType)
                .Cast<object>()
                .ToDictionary(
                    op => enumType.GetMember(op.ToString()).FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? op.ToString(),
                    op => op
                );
        }

        public Profiles.Application Application { get; set; }

        public static readonly DependencyProperty OperandProperty = DependencyProperty.Register("Operand", typeof(IEvaluatableNumber), typeof(Control_NumericUnaryOpHolder), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public IEvaluatableNumber Operand {
            get => (IEvaluatableNumber)GetValue(OperandProperty);
            set => SetValue(OperandProperty, value);
        }

        public static readonly DependencyProperty SelectedOperatorProperty = DependencyProperty.Register("SelectedOperator", typeof(object), typeof(Control_NumericUnaryOpHolder), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public object SelectedOperator {
            get => GetValue(SelectedOperatorProperty);
            set => SetValue(SelectedOperatorProperty, value);
        }

        public void SetApplication(Profiles.Application application) {
            Application = application;
        }
    }
}
