using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Aurora.Utils;

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

        

        bool _isEditingLayout = false;
        bool isEditingLayout { get => _isEditingLayout; set { _isEditingLayout = value; isEditingChanged(); } }

        private void isEditingChanged()
        {
            //this.btnEditLayout.Content = isEditingLayout ? "Stop Editing Layout" : "Edit Layout";
            foreach (Thumb t in UIUtils.FindVisualChildren<Thumb>(GlobalDeviceLayout.Instance.GetControl())) {
                t.IsHitTestVisible = isEditingLayout;
            }
        }

        private void Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            isEditingLayout = this.IsVisible;
        }

        /*private void BtnEditLayout_Click(object sender, RoutedEventArgs e)
        {
            isEditingLayout = !isEditingLayout;
        }

        private void BtnEditLayout_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            isEditingLayout = false;
        }*/
    }
}
