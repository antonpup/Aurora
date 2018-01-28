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

namespace Aurora.Settings.Layers
{
    /// <summary>
    /// Interaction logic for Control_SolidFillLayer.xaml
    /// </summary>
    public partial class Control_WrapperLightsLayer : UserControl
    {
        private bool settingsset = false;

        public Control_WrapperLightsLayer()
        {
            InitializeComponent();
        }

        public Control_WrapperLightsLayer(WrapperLightsLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if(this.DataContext is WrapperLightsLayerHandler && !settingsset)
            {
                settingsset = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }

        private void ce_color_factor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
                this.ce_color_factor_label.Text = ((float)this.ce_color_factor.Value).ToString();
        }

        private void ce_color_hsv_sine_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
                this.ce_color_hsv_sine_label.Text = ((float)this.ce_color_hsv_sine.Value).ToString();
        }

        private void ce_color_hsv_gamma_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
                this.ce_color_hsv_gamma_label.Text = ((float)this.ce_color_hsv_gamma.Value).ToString();
        }
    }
}
