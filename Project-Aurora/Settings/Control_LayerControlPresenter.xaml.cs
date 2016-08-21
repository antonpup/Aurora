using Aurora.Settings.Layers;
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

namespace Aurora.Settings
{
    /// <summary>
    /// Interaction logic for Control_LayerControlPresenter.xaml
    /// </summary>
    public partial class Control_LayerControlPresenter : UserControl
    {
        public Control_LayerControlPresenter()
        {
            InitializeComponent();
        }

        public Control_LayerControlPresenter(DefaultLayer layer)
        {
            InitializeComponent();

            SetLayer(layer);
        }

        public void SetLayer(DefaultLayer layer)
        {
            layer_control_grid.Children.Clear();
            layer_control_grid.Children.Add(layer.Control);
        }
    }
}
