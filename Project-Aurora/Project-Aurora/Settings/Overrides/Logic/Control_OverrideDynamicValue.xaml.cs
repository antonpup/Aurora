using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_OverrideDynamicValue.xaml
    /// </summary>
    public partial class Control_OverrideDynamicValue : UserControl {

        public OverrideDynamicValue Context { get; }
        public Profiles.Application Application { get; }
        public List<Tuple<string, EvaluatableType, IEvaluatable, string>> Parameters { get; }

        public Control_OverrideDynamicValue(OverrideDynamicValue context, Profiles.Application application) {
            InitializeComponent();
            Context = context;
            Application = application;
            // Get a list that contains the label (string), the type of evaluatable (to restrict the list box), the evaluatable itself and the description.
            Parameters = context.ConstructorParameters.Select(kvp => new Tuple<string, EvaluatableType, IEvaluatable, string>(
                kvp.Key,
                OverrideDynamicValue.typeDynamicDefMap[context.VarType].constructorParameters.First(p => p.name == kvp.Key).type,
                kvp.Value,
                OverrideDynamicValue.typeDynamicDefMap[context.VarType].constructorParameters.First(p => p.name == kvp.Key).description
            )).ToList();
            DataContext = this;
        }

        private void VariableEvaluatable_ExpressionChanged(object sender, ExpressionChangeEventArgs e) {
            // Item1 refers to the parameter name, used as the key in the IEvaluatable list
            var paramName = ((dynamic)((Control_EvaluatablePresenter)sender).DataContext).Item1;
            Context.ConstructorParameters[paramName] = e.NewExpression;
        }
    }

    #region UI converters
    /// <summary>Results in an underline decoration if the given input string is non-null or empty, else returns nothing. Used for the tooltip textblock.</summary>
    public class UnderlineConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            string.IsNullOrWhiteSpace(value?.ToString()) ? null : new TextDecorationCollection(new[] { new TextDecoration() }); // Default decoration is a solid black line, so this is fine
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    /// <summary>Results in a the hand cursor if the given input string is non-null or empty, else returns nothing. Used for the tooltip textblock.</summary>
    public class CurosrConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            string.IsNullOrWhiteSpace(value?.ToString()) ? null : Cursors.Help;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
    #endregion
}
