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
using Xceed.Wpf.Toolkit;

namespace Aurora.Profiles.CSGO.Layers
{
    /// <summary>
    /// Interaction logic for Control_CSGOBackgroundLayer.xaml
    /// </summary>
    public partial class Control_CSGOBackgroundLayer : UserControl
    {
        private bool settingsset = false;

        public Control_CSGOBackgroundLayer()
        {
            InitializeComponent();
        }

        public Control_CSGOBackgroundLayer(CSGOBackgroundLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is CSGOBackgroundLayerHandler && !settingsset)
            {
                this.ColorPicker_CT.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as CSGOBackgroundLayerHandler).Properties._CTColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_T.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as CSGOBackgroundLayerHandler).Properties._TColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Default.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as CSGOBackgroundLayerHandler).Properties._DefaultColor ?? System.Drawing.Color.Empty);
                this.Checkbox_DimEnabled.IsChecked = (this.DataContext as CSGOBackgroundLayerHandler).Properties._DimEnabled;
                this.TextBox_DimValue.Content = (int)(this.DataContext as CSGOBackgroundLayerHandler).Properties._DimDelay + "s";
                this.Slider_DimSelector.Value = (this.DataContext as CSGOBackgroundLayerHandler).Properties._DimDelay.Value;
                this.IntegerUpDown_DimAmount.Value = (this.DataContext as CSGOBackgroundLayerHandler).Properties._DimAmount.Value;

                settingsset = true;
            }
        }

        internal void SetProfile(Application profile)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }

        private void ColorPicker_CT_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOBackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as CSGOBackgroundLayerHandler).Properties._CTColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_T_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOBackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as CSGOBackgroundLayerHandler).Properties._TColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Default_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOBackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as CSGOBackgroundLayerHandler).Properties._DefaultColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void Checkbox_DimEnabled_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOBackgroundLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
                (this.DataContext as CSGOBackgroundLayerHandler).Properties._DimEnabled  = (sender as CheckBox).IsChecked.Value;
        }

        private void Slider_DimSelector_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOBackgroundLayerHandler && sender is Slider)
            {
                (this.DataContext as CSGOBackgroundLayerHandler).Properties._DimDelay = (sender as Slider).Value;

                this.TextBox_DimValue.Content = (int)(sender as Slider).Value + "s";
            }
        }

        private void IntegerUpDown_DimAmount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOBackgroundLayerHandler && sender is IntegerUpDown)
            {
                (this.DataContext as CSGOBackgroundLayerHandler).Properties._DimAmount = (sender as IntegerUpDown).Value;
            }
        }
    }
}
