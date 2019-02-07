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

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_OverrideLookupTable.xaml
    /// </summary>
    public partial class Control_OverrideLookupTable : UserControl {

        private OverrideLookupTable table;

        public Control_OverrideLookupTable(OverrideLookupTable context) {
            InitializeComponent();
            DataContext = table = context;
        }

        private void AddNewLookup_Click(object sender, RoutedEventArgs e) {
            table?.CreateNewLookup();
        }

        private void DeleteLookupEntry_Click(object sender, RoutedEventArgs e) {
            var dc = (OverrideLookupTable.LookupTableEntry)((Button)sender).DataContext;
            table.LookupTable.Remove(dc);
        }
    }
}