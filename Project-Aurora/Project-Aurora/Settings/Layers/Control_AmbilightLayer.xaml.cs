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
                var properties = (this.DataContext as AmbilightLayerHandler).Properties;
                this.combobox_ambilight_effect_type.SelectedItem = properties._AmbilightType;
                this.combobox_ambilight_capture_type.SelectedItem = properties._AmbilightCaptureType;
                this.txtBox_process_name.Text = properties._SpecificProcess;
                this.txtbox_output_id.Text = properties.AmbilightOutputId.ToString();
                this.combobox_ambilight_fps.SelectedItem = properties._AmbiLightUpdatesPerSecond;

                ToggleProcessTxtBox();

                settingsset = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }

        private void txtbox_output_id_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            try
            {
                int outputId = int.Parse(textBox.Text);
                if (IsLoaded && settingsset && this.DataContext is AmbilightLayerHandler)
                {
                    var context = this.DataContext as AmbilightLayerHandler;
                    if (outputId != context.Properties._AmbilightOutputId)
                    {
                        context.Properties._AmbilightOutputId = outputId;
                        context.Initialize();
                    }
                }
            }
            catch (FormatException){}

        }

        private void combobox_fps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is AmbilightLayerHandler && sender is ComboBox)
            {
                String strValue = (sender as ComboBox).SelectedIndex.ToString();
                AmbilightFpsChoice fps = (AmbilightFpsChoice)Enum.Parse(typeof(AmbilightFpsChoice), strValue);
                (this.DataContext as AmbilightLayerHandler).Properties._AmbiLightUpdatesPerSecond = fps;
            }
        }

        private void combobox_ambilight_effect_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is AmbilightLayerHandler && sender is ComboBox)
                (this.DataContext as AmbilightLayerHandler).Properties._AmbilightType = (AmbilightType)Enum.Parse(typeof(AmbilightType), (sender as ComboBox).SelectedIndex.ToString());
        }

        private void combobox_ambilight_capture_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is AmbilightLayerHandler && sender is ComboBox)
            {
                (this.DataContext as AmbilightLayerHandler).Properties._AmbilightCaptureType = (AmbilightCaptureType)Enum.Parse(typeof(AmbilightCaptureType), (sender as ComboBox).SelectedIndex.ToString());

                ToggleProcessTxtBox();
            }
        }

        private void txtBox_process_name_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is AmbilightLayerHandler && sender is TextBox)
                (this.DataContext as AmbilightLayerHandler).Properties._SpecificProcess = (sender as TextBox).Text;
        }

        private void ToggleProcessTxtBox()
        {
            if ((this.DataContext as AmbilightLayerHandler).Properties._AmbilightCaptureType == AmbilightCaptureType.SpecificProcess)
                txtBox_process_name.IsEnabled = true;
            else
                txtBox_process_name.IsEnabled = false;
        }
    }
}
