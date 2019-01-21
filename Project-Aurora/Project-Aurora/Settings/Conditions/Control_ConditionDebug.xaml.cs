using System.Windows.Controls;

namespace Aurora.Settings.Conditions {
    /// <summary>
    /// Interaction logic for Control_ConditionDebug.xaml
    /// </summary>
    public partial class Control_ConditionDebug : UserControl {
        public Control_ConditionDebug(ConditionDebug ctx) {
            InitializeComponent();
            DataContext = ctx;
        }
    }
}
