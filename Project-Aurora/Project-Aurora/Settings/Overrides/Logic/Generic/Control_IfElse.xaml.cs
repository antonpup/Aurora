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

        // Event handlers for the buttons, marked virtual so they can be overriden and handled far easier by the `Control_Ternary<T>` class.
        protected virtual void AddElseIfCase_Click(object sender, RoutedEventArgs e) { }
        protected virtual void CaseUp_Click(object sender, RoutedEventArgs e) { }
        protected virtual void CaseDown_Click(object sender, RoutedEventArgs e) { }
        protected virtual void DeleteCase_Click(object sender, RoutedEventArgs e) { }
    }

    public partial class Control_Ternary<T> : Control_Ternary {

        private Control_Ternary_Context<T> context;

        public Control_Ternary(IfElseGeneric<T> context) : base() {
            DataContext = this.context = new Control_Ternary_Context<T> {
                ParentCondition = context,
                EvaluatableType = typeof(T)
            };
        }

        protected override void AddElseIfCase_Click(object sender, RoutedEventArgs e) {
            context.ParentCondition.Cases.Insert(
                context.ParentCondition.Cases.Count - (HasElseCase ? 2 : 1), // If there is an "Else" case, we need to insert this Else-If before that
                new IfElseGeneric<T>.Branch(new BooleanConstant(), EvaluatableDefaults.Get<T>())
            );
        }

        protected override void CaseUp_Click(object sender, RoutedEventArgs e) {
            var branch = (IfElseGeneric<T>.Branch)((Button)sender).DataContext;
            var index = context.ParentCondition.Cases.IndexOf(branch);

            // Check the clicked case to move up wasn't the first one or the last one (we don't want to move Elses around), move it up by
            // removing it and re-adding it to the collection
            if (index > 0 && !(index == context.ParentCondition.Cases.Count - 1 && HasElseCase)) {
                context.ParentCondition.Cases.RemoveAt(index);
                context.ParentCondition.Cases.Insert(index - 1, branch);
            }
        }

        protected override void CaseDown_Click(object sender, RoutedEventArgs e) {
            var branch = (IfElseGeneric<T>.Branch)((Button)sender).DataContext;
            var index = context.ParentCondition.Cases.IndexOf(branch);

            // Check the clicked case to move up wasn't the last one, and also check that the clicked one wasn't the second last one (we
            // don't want to move Elses), and move it up by removing it and re-adding it to the collection
            if (index < context.ParentCondition.Cases.Count - 1 && index != context.ParentCondition.Cases.Count - 2) {
                context.ParentCondition.Cases.RemoveAt(index);
                context.ParentCondition.Cases.Insert(index + 1, branch);
            }
        }

        protected override void DeleteCase_Click(object sender, RoutedEventArgs e) {
            var branch = (IfElseGeneric<T>.Branch)((Button)sender).DataContext;

            // Check that when we are deleting, there is atleast two branches left after the deletion, so we have at minimum an If and an Else.
            if (context.ParentCondition.Cases.Count > 2)
                context.ParentCondition.Cases.Remove(branch);
        }

        private bool HasElseCase =>
            context.ParentCondition.Cases[context.ParentCondition.Cases.Count - 1].Condition == null;
    }


    /// <summary>
    /// The datatype that is used as the DataContext for `Control_ConditionNot`.
    /// </summary>
    internal class Control_Ternary_Context<T> {
        public IfElseGeneric<T> ParentCondition { get; set; }
        public Type EvaluatableType { get; set; }
    }


    /// <summary>
    /// Converter that takes 2 parameters (current condition and previous condition) and returns a string that is the relevant "If" verb.
    /// If the previous condition is null, returns "If" (since it's the first condition). If the current current is null, returns "Else",
    /// otherwise returns "Else If".
    /// </summary>
    public class IfElseTextConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
            values[1] == DependencyProperty.UnsetValue ? "If" : (values[0] == null ? "Else" : "Else If");
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
