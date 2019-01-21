using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aurora.Settings.Conditions {
    /// <summary>
    /// Interaction logic for Control_ConditionDebug.xaml
    /// </summary>
    public partial class Control_ConditionDebug : UserControl {
        public Control_ConditionDebug(ConditionDebug ctx) {
            InitializeComponent();
            DataContext = ctx;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            ((ConditionDebug)DataContext).state = ((CheckBox)sender).IsChecked ?? false;
        }
    }
}
