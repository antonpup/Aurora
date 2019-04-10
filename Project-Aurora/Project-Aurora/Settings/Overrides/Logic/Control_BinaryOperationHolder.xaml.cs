using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Aurora.Profiles;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_SubconditionHolder.xaml
    /// </summary>
    public partial class Control_BinaryOperationHolder : UserControl {
        /// <summary>
        /// Creates a new numeric binary operation holder control.
        /// </summary>
        public Control_BinaryOperationHolder(Profiles.Application application, EvaluatableType evalType) {
            InitializeComponent();
            Application = application;
            EvalType = evalType;
            DataContext = this;
        }

        /// <summary>
        /// Creates a new numeric binary operation holder control using the values of the specified enum as the operators item source.
        /// </summary>
        public Control_BinaryOperationHolder(Profiles.Application application, EvaluatableType evalType, Type enumType) : this(application, evalType) {
            operatorSelection.ItemsSource = Enum.GetValues(enumType)
                .Cast<object>()
                .ToDictionary(
                    op => enumType.GetMember(op.ToString()).FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? op.ToString(),
                    op => op
                );
        }

        #region Properties
        public Profiles.Application Application { get; set; }
        public EvaluatableType EvalType { get; set; }
        #endregion

        #region Methods
        public void SetApplication(Profiles.Application application) {
            Application = application;
        }
        #endregion Methods

        #region Dependency Properties
        public static readonly DependencyProperty Operand1Property = DependencyProperty.Register("Operand1", typeof(IEvaluatable), typeof(Control_BinaryOperationHolder), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public IEvaluatable Operand1 {
            get => (IEvaluatable)GetValue(Operand1Property);
            set => SetValue(Operand1Property, value);
        }

        public static readonly DependencyProperty Operand2Property = DependencyProperty.Register("Operand2", typeof(IEvaluatable), typeof(Control_BinaryOperationHolder), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public IEvaluatable Operand2 {
            get => (IEvaluatable)GetValue(Operand2Property);
            set => SetValue(Operand2Property, value);
        }

        public static readonly DependencyProperty SelectedOperatorProperty = DependencyProperty.Register("SelectedOperator", typeof(object), typeof(Control_BinaryOperationHolder), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public object SelectedOperator {
            get => GetValue(SelectedOperatorProperty);
            set => SetValue(SelectedOperatorProperty, value);
        }
        #endregion
    }
}
