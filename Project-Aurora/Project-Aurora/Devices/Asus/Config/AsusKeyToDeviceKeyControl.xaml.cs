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

namespace Aurora.Devices.Asus.Config
{
    /// <summary>
    /// Interaction logic for AsusKeyToDeviceKeyControl.xaml
    /// </summary>
    public partial class AsusKeyToDeviceKeyControl : UserControl
    {
        public Action BlinkCallback;
        
        public AsusKeyToDeviceKeyControl()
        {
            InitializeComponent();
        }

        private void TestBlink(object sender, RoutedEventArgs e)
        {
            BlinkCallback?.Invoke();
        }
    }
}
