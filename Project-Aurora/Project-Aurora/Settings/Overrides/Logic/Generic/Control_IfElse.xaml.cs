using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_ConditionNot.xaml
    /// </summary>
    public abstract partial class Control_Ternary : UserControl {
        public Control_Ternary() {
            InitializeComponent();
        }

        internal virtual void AddElseIfCase_Click(object sender, RoutedEventArgs e) { }
        internal virtual void AddElseCase_Click(object sender, RoutedEventArgs e) { }
    }

    public partial class Control_Ternary<T> : Control_Ternary {

        private Control_Ternary_Context<T> context;

        public Control_Ternary(IfElseGeneric<T> context, Profiles.Application application) : base() {
            DataContext = this.context = new Control_Ternary_Context<T> {
                Application = application,
                ParentCondition = context,
                EvaluatableType = EvaluatableTypeResolver.GetEvaluatableType(typeof(IEvaluatable<T>))
            };
        }

        internal override void AddElseIfCase_Click(object sender, RoutedEventArgs e) {
            context.ParentCondition.Cases.Insert(
                context.ParentCondition.Cases.Count - (HasElseBlock ? 2 : 1),
                new IfElseGeneric<T>.Branch(new BooleanConstant(), (IEvaluatable<T>)EvaluatableTypeResolver.GetDefault(context.EvaluatableType))
            );
        }

        internal override void AddElseCase_Click(object sender, RoutedEventArgs e) {
            if (!HasElseBlock)
                context.ParentCondition.Cases.Add(new IfElseGeneric<T>.Branch(null, (IEvaluatable<T>)EvaluatableTypeResolver.GetDefault(context.EvaluatableType)));
        }

        private bool HasElseBlock => context.ParentCondition.Cases[context.ParentCondition.Cases.Count - 1].Condition == null;
    }


    /// <summary>
    /// The datatype that is used as the DataContext for `Control_ConditionNot`.
    /// </summary>
    internal class Control_Ternary_Context<T> {
        public IfElseGeneric<T> ParentCondition { get; set; }
        public Profiles.Application Application { get; set; }
        public EvaluatableType EvaluatableType { get; set; }
    }


    /// <summary>
    /// Converter that takes 2 parameters (current condition and previous condition) and returns a string that is the relevant "If" verb.
    /// If the previous condition is null, returns "If" (since it's the first condition). If the current current is null, returns "Else",
    /// otherwise returns "Else If".
    /// </summary>
    public class IfElseTextConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
            values[1] == System.Windows.DependencyProperty.UnsetValue ? "If" : (values[0] == null ? "Else" : "Else If");
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }


    /// <summary>
    /// Converter for the "Add Else" button. Will disable the button if there is already an else block in the case list.
    /// </summary>
    /// <remarks>This is created as a converter rather than a property so that it automatically updates with the collection without needing
    /// to add any additional INotifyPropertyChanged logic or anything.</remarks>
    public class HasElseBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            ((dynamic)value)[((dynamic)value).Count - 1].Condition != null; // This horrid "dynamic" code is because we don't know the evaluatable type in the converter (converters cannot be generic)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
