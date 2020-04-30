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

namespace Aurora.Profiles.GTA5.Layers
{
    /// <summary>
    /// Interaction logic for Control_GTA5BackgroundLayer.xaml
    /// </summary>
    public partial class Control_GTA5BackgroundLayer : UserControl
    {
        private bool settingsset = false;

        public Control_GTA5BackgroundLayer()
        {
            InitializeComponent();
        }

        public Control_GTA5BackgroundLayer(GTA5BackgroundLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is GTA5BackgroundLayerHandler && !settingsset)
            {
                this.ColorPicker_Default.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as GTA5BackgroundLayerHandler).Properties._DefaultColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Michael.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as GTA5BackgroundLayerHandler).Properties._MichaelColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Franklin.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as GTA5BackgroundLayerHandler).Properties._FranklinColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Trevor.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as GTA5BackgroundLayerHandler).Properties._TrevorColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Chop.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as GTA5BackgroundLayerHandler).Properties._ChopColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_OnlineMission.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as GTA5BackgroundLayerHandler).Properties._OnlineMissionColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_OnlineHeistFinale.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as GTA5BackgroundLayerHandler).Properties._OnlineHeistFinaleColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_OnlineSpectator.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as GTA5BackgroundLayerHandler).Properties._OnlineSpectatorColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_RaceGold.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as GTA5BackgroundLayerHandler).Properties._RaceGoldColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_RaceSilver.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as GTA5BackgroundLayerHandler).Properties._RaceSilverColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_RaceBronze.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as GTA5BackgroundLayerHandler).Properties._RaceBronzeColor ?? System.Drawing.Color.Empty);

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

        private void ColorPicker_Default_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GTA5BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as GTA5BackgroundLayerHandler).Properties._DefaultColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Michael_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GTA5BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as GTA5BackgroundLayerHandler).Properties._MichaelColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Franklin_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GTA5BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as GTA5BackgroundLayerHandler).Properties._FranklinColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Trevor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GTA5BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as GTA5BackgroundLayerHandler).Properties._TrevorColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Chop_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GTA5BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as GTA5BackgroundLayerHandler).Properties._ChopColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_OnlineMission_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GTA5BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as GTA5BackgroundLayerHandler).Properties._OnlineMissionColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_OnlineHeistFinale_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GTA5BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as GTA5BackgroundLayerHandler).Properties._OnlineHeistFinaleColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_OnlineSpectator_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GTA5BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as GTA5BackgroundLayerHandler).Properties._OnlineSpectatorColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_RaceGold_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GTA5BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as GTA5BackgroundLayerHandler).Properties._RaceGoldColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_RaceSilver_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GTA5BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as GTA5BackgroundLayerHandler).Properties._RaceSilverColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_RaceBronze_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GTA5BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as GTA5BackgroundLayerHandler).Properties._RaceBronzeColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }
    }
}
