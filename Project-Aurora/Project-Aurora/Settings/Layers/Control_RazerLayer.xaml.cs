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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Control_RazerLayer : UserControl
    {
        private bool settingsset = false;

        public Control_RazerLayer()
        {
            InitializeComponent();
        }

        public Control_RazerLayer(RazerLayerHandler datacontext)
        {
            InitializeComponent();

            DataContext = datacontext;
        }
        public void SetSettings()
        {
            if (DataContext is RazerLayerHandler handler && !settingsset)
            {
                ColorPostProcessCheckBox.IsChecked = handler.Properties.ColorPostProcessEnabled;
                BrightnessBoostSlider.Value = handler.Properties.BrightnessBoost;
                settingsset = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();
            this.Loaded -= UserControl_Loaded;
        }
    }
}
