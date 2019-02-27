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

namespace Aurora.Devices.Layout
{
    /// <summary>
    /// Interaction logic for Control_GlobalDeviceLayout.xaml
    /// </summary>
    public partial class Control_GlobalDeviceLayout : UserControl
    {
        public Control_GlobalDeviceLayout()
        {
            InitializeComponent();
            this.DataContext = GlobalDeviceLayout.Instance;
        }
    }
}
