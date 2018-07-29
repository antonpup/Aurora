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

namespace Aurora.Profiles.Witcher3.Layers
{
    /// <summary>
    /// Interaction logic for Control_Witcher3BackgroundLayer.xaml
    /// </summary>
    public partial class Control_Witcher3BackgroundLayer : UserControl
    {
        private bool settingsset = false;
        private bool profileset = false;

        public Control_Witcher3BackgroundLayer()
        {
            InitializeComponent();
        }

        public Control_Witcher3BackgroundLayer(Witcher3BackgroundLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is Witcher3BackgroundLayerHandler && !settingsset)
            {
                this.ColorPicker_Aard.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Witcher3BackgroundLayerHandler).Properties._AardColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Igni.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Witcher3BackgroundLayerHandler).Properties._IgniColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Quen.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Witcher3BackgroundLayerHandler).Properties._QuenColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Yrden.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Witcher3BackgroundLayerHandler).Properties._YrdenColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Axii.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Witcher3BackgroundLayerHandler).Properties._AxiiColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Default.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Witcher3BackgroundLayerHandler).Properties._DefaultColor ?? System.Drawing.Color.Empty);

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

        private void ColorPicker_Aard_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Witcher3BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Witcher3BackgroundLayerHandler).Properties._AardColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Igni_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Witcher3BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Witcher3BackgroundLayerHandler).Properties._IgniColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Quen_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Witcher3BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Witcher3BackgroundLayerHandler).Properties._QuenColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Yrden_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Witcher3BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Witcher3BackgroundLayerHandler).Properties._YrdenColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Axii_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Witcher3BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Witcher3BackgroundLayerHandler).Properties._AxiiColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Default_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Witcher3BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Witcher3BackgroundLayerHandler).Properties._DefaultColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }
    }
}
