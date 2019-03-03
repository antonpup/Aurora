using Aurora.Profiles;
using Aurora.Utils;
using System;
using System.Linq;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_ConditionGSIBoolean.xaml
    /// </summary>
    public partial class Control_ConditionGSIBoolean : UserControl {

        public Control_ConditionGSIBoolean(BooleanGSIBoolean context, Application application) {
            InitializeComponent();
            DataContext = context;
            SetApplication(application);
        }

        internal void SetApplication(Application application) {
            ValidPathsCombo.ItemsSource = application?.ParameterLookup?.GetBooleanParameters();
        }
    }
}
