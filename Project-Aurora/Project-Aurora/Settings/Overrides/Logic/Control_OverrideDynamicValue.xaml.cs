using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_OverrideDynamicValue.xaml
    /// </summary>
    public partial class Control_OverrideDynamicValue : UserControl {

        public OverrideDynamicValue Context { get; }
        public Profiles.Application Application { get; }
        public List<Tuple<string, EvaluatableType, IEvaluatable>> Parameters { get; }

        public Control_OverrideDynamicValue(OverrideDynamicValue context, Profiles.Application application) {
            InitializeComponent();
            Context = context;
            Application = application;
            // Get a list that contains the label (string), the type of evaluatable (to restrict the list box) and the evaluatable itself.
            Parameters = context.ConstructorParameters.Select(kvp => new Tuple<string, EvaluatableType, IEvaluatable>(kvp.Key, OverrideDynamicValue.typeDynamicDefMap[context.VarType].constructorParameters[kvp.Key], kvp.Value)).ToList();
            DataContext = this;
        }

        private void VariableEvaluatable_ExpressionChanged(object sender, ExpressionChangeEventArgs e) {
            // Item1 refers to the parameter name, used as the key in the IEvaluatable list
            var paramName = ((dynamic)((Control_EvaluatablePresenter)sender).DataContext).Item1;
            Context.ConstructorParameters[paramName] = e.NewExpression;
        }
    }
}
