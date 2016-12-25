using Aurora.Profiles.Desktop;
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
    /// Interaction logic for Control_AmbilightLayer.xaml
    /// </summary>
    public partial class Control_AmbilightLayer : UserControl
    {
        private bool settingsset = false;

        public Control_AmbilightLayer()
        {
            InitializeComponent();
        }

        public Control_AmbilightLayer(AmbilightLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is AmbilightLayerHandler && !settingsset)
            {
                this.combobox_ambilight_effect_type.SelectedIndex = (int)(this.DataContext as AmbilightLayerHandler).Properties._AmbilightType;

                settingsset = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }


        private void combobox_ambilight_effect_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is AmbilightLayerHandler && sender is ComboBox)
            {
                (this.DataContext as AmbilightLayerHandler).Properties._AmbilightType = (AmbilightType)Enum.Parse(typeof(AmbilightType), (sender as ComboBox).SelectedIndex.ToString());
            }
        }
    }
}
