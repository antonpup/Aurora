using Aurora.Settings;
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

namespace Aurora.Profiles.Payday_2.Layers
{
    /// <summary>
    /// Interaction logic for Control_WinningTeamLayer.xaml
    /// </summary>
    public partial class Control_PD2MaskOnAnimationLayer : UserControl
    {
        public Control_PD2MaskOnAnimationLayer()
        {
            InitializeComponent();
        }

        public Control_PD2MaskOnAnimationLayer(PD2MaskOnAnimationLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
        }
    }
}
