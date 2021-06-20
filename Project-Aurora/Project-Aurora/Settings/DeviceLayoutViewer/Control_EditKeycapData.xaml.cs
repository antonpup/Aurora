using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Aurora.Settings.DeviceLayoutViewer
{
    /// <summary>
    /// Interaction logic for Control_EditKeycapData.xaml
    /// </summary>
    public partial class Control_EditKeycapData : UserControl
    {
        private bool _isCollapsed;
        public bool IsCollapsed
        {
            get { return _isCollapsed; }
            set
            {
                _isCollapsed = value;
                if (_isCollapsed)
                    keycapData.Visibility = Visibility.Collapsed;
                else
                    keycapData.Visibility = Visibility.Visible;
                //LoadDeviceLayout();
            }
        }
        public Control_EditKeycapData()
        {
            InitializeComponent();
            Loaded += OnLoad;
            IsCollapsed = true;
        }
        public Control_EditKeycapData(Control_Keycap layout)
        {
            InitializeComponent();
            Loaded += OnLoad;
            DataContext = layout;
            IsCollapsed = true;

        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            if ((DataContext as Control_Keycap).Config.Tag == -1)
            {
                GhostKeyLabel.Visibility = Visibility.Visible;
                TagTextBox.Visibility = Visibility.Collapsed;
            }
        }
        private void collapseButton_Click(object sender, RoutedEventArgs e)
        {
            IsCollapsed = !IsCollapsed;
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
