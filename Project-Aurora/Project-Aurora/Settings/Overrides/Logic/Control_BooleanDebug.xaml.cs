using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_ConditionDebug.xaml
    /// </summary>
    public partial class Control_ConditionDebug : UserControl {
        public Control_ConditionDebug(BooleanTogglable ctx) {
            InitializeComponent();
            DataContext = ctx;
        }
    }
}
