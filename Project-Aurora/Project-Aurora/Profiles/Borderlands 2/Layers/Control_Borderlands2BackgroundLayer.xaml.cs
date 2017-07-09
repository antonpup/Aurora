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

namespace Aurora.Profiles.Borderlands2.Layers
{
    /// <summary>
    /// Interaction logic for Control_Borderlands2BackgroundLayer.xaml
    /// </summary>
    public partial class Control_Borderlands2BackgroundLayer : UserControl
    {
        private bool settingsset = false;
        private bool profileset = false;

        public Control_Borderlands2BackgroundLayer()
        {
            InitializeComponent();
        }

        public Control_Borderlands2BackgroundLayer(Borderlands2BackgroundLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is Borderlands2BackgroundLayerHandler && !settingsset)
            {
                /// Health
                this.ColorPicker_Health.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ColorHealth ?? System.Drawing.Color.Empty);
                this.ColorPicker_Health_Low.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ColorHealthLow ?? System.Drawing.Color.Empty);

                /// Shield
                this.ColorPicker_Shield.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ColorShield ?? System.Drawing.Color.Empty);
                this.ColorPicker_Shield_Low.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ColorShieldLow ?? System.Drawing.Color.Empty);

                /// Background
                this.ColorPicker_Background.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ColorBackground ?? System.Drawing.Color.Empty);
                this.ColorPicker_Background_Death.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ColorBackgroundDeath ?? System.Drawing.Color.Empty);

                /// Show Status Bars
                this.Checkbox_ShowHealthStatus.IsChecked = (this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ShowHealthStatus ?? false;
                this.Checkbox_ShowShieldStatus.IsChecked = (this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ShowShieldStatus ?? false;

                settingsset = true;
            }
        }

        internal void SetProfile(Application profile)
        {
            if (profile != null && !profileset)
            {
                var var_types_numerical = profile.ParameterLookup?.Where(kvp => Utils.TypeUtils.IsNumericType(kvp.Value.Item1));

                profileset = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }

        private void ColorPicker_Health_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Borderlands2BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ColorHealth = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Health_Low_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Borderlands2BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ColorHealthLow = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Shield_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Borderlands2BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ColorShield = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Shield_Low_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Borderlands2BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ColorShieldLow = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Background_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Borderlands2BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ColorBackground = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Background_Death_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Borderlands2BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ColorBackgroundDeath = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void Checkbox_ShowHealthStatus_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is Borderlands2BackgroundLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
                (this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ShowHealthStatus = (sender as CheckBox).IsChecked.Value;
        }

        private void Checkbox_ShowShieldStatus_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is Borderlands2BackgroundLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
                (this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ShowShieldStatus = (sender as CheckBox).IsChecked.Value;
        }
    }
}
