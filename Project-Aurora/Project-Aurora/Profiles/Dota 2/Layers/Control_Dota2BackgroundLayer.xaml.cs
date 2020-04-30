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

namespace Aurora.Profiles.Dota_2.Layers
{
    /// <summary>
    /// Interaction logic for Control_Dota2BackgroundLayer.xaml
    /// </summary>
    public partial class Control_Dota2BackgroundLayer : UserControl
    {
        private bool settingsset = false;

        public Control_Dota2BackgroundLayer()
        {
            InitializeComponent();
        }

        public Control_Dota2BackgroundLayer(Dota2BackgroundLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is Dota2BackgroundLayerHandler && !settingsset)
            {
                this.ColorPicker_Dire.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2BackgroundLayerHandler).Properties._DireColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Radiant.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2BackgroundLayerHandler).Properties._RadiantColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Default.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2BackgroundLayerHandler).Properties._DefaultColor ?? System.Drawing.Color.Empty);
                this.Checkbox_DimEnabled.IsChecked = (this.DataContext as Dota2BackgroundLayerHandler).Properties._DimEnabled;
                this.TextBox_DimValue.Content = (int)(this.DataContext as Dota2BackgroundLayerHandler).Properties._DimDelay + "s";
                this.Slider_DimSelector.Value = (this.DataContext as Dota2BackgroundLayerHandler).Properties._DimDelay.Value;

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

        private void ColorPicker_Dire_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2BackgroundLayerHandler).Properties._DireColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Radiant_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2BackgroundLayerHandler).Properties._RadiantColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Default_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2BackgroundLayerHandler).Properties._DefaultColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void Checkbox_DimEnabled_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2BackgroundLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
                (this.DataContext as Dota2BackgroundLayerHandler).Properties._DimEnabled  = (sender as CheckBox).IsChecked.Value;
        }

        private void Slider_DimSelector_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2BackgroundLayerHandler && sender is Slider)
            {
                (this.DataContext as Dota2BackgroundLayerHandler).Properties._DimDelay = (sender as Slider).Value;

                if (this.TextBox_DimValue is Label)
                    this.TextBox_DimValue.Content = (int)(sender as Slider).Value + "s";
            }
        }
    }
}
