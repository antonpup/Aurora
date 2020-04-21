using Aurora.Profiles;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_ConditionGSINumeric.xaml
    /// </summary>
    public partial class Control_ConditionGSINumeric : UserControl {

        public Control_ConditionGSINumeric(BooleanGSINumeric context) {
            InitializeComponent();
            DataContext = context;
            OperatorCb.ItemsSource = Utils.EnumUtils.GetEnumItemsSource<ComparisonOperator>();
        }
    }
}
