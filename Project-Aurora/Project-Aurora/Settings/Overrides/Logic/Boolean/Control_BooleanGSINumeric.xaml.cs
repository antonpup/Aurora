using Aurora.Profiles;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_ConditionGSINumeric.xaml
    /// </summary>
    public partial class Control_ConditionGSINumeric : UserControl {

        public Control_ConditionGSINumeric(BooleanGSINumeric context, Application application) {
            InitializeComponent();
            DataContext = context;
            SetApplication(application);

            OperatorCb.ItemsSource = Enum.GetValues(typeof(ComparisonOperator))
                .Cast<ComparisonOperator>()
                .Select(op => new {
                    Label = typeof(ComparisonOperator).GetMember(op.ToString()).FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description,
                    Value = op
                });
        }

        internal void SetApplication(Application application) {
            Operand1Cb.ItemsSource = Operand2Cb.ItemsSource = application?.ParameterLookup?
                .Where(kvp => Utils.TypeUtils.IsNumericType(kvp.Value.Item1))
                .Select(kvp => kvp.Key);
        }
    }
}
