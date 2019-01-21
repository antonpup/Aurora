using Aurora.Profiles;
using System;
using System.Linq;
using System.Windows.Controls;

namespace Aurora.Settings.Conditions {
    /// <summary>
    /// Interaction logic for Control_ConditionGSIBoolean.xaml
    /// </summary>
    public partial class Control_ConditionGSIBoolean : UserControl {

        public Control_ConditionGSIBoolean(ConditionGSIBoolean context, Application application) {
            InitializeComponent();
            DataContext = context;
            SetApplication(application);
        }

        internal void SetApplication(Application application) {
            ValidPathsCombo.ItemsSource = application?.ParameterLookup?
                .Where(kvp => Type.GetTypeCode(kvp.Value.Item1) == TypeCode.Boolean)
                .Select(kvp => kvp.Key);
        }
    }
}
