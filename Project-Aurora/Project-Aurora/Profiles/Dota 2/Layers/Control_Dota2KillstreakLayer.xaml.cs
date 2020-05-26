using Aurora.Devices;
using Aurora.Settings;
using Aurora.Utils;
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
    /// Interaction logic for Control_Dota2KillstreakLayer.xaml
    /// </summary>
    public partial class Control_Dota2KillstreakLayer : UserControl
    {
        private bool settingsset = false;

        public Control_Dota2KillstreakLayer()
        {
            InitializeComponent();
        }

        public Control_Dota2KillstreakLayer(Dota2KillstreakLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is Dota2KillstreakLayerHandler && !settingsset)
            {
                this.ColorPicker_doublekill.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2KillstreakLayerHandler).Properties._DoubleKillstreakColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_triplekill.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2KillstreakLayerHandler).Properties._TripleKillstreakColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_quadkill.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2KillstreakLayerHandler).Properties._QuadKillstreakColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_pentakill.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2KillstreakLayerHandler).Properties._PentaKillstreakColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_hexakill.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2KillstreakLayerHandler).Properties._HexaKillstreakColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_septakill.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2KillstreakLayerHandler).Properties._SeptaKillstreakColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_octakill.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2KillstreakLayerHandler).Properties._OctaKillstreakColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_nonakill.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2KillstreakLayerHandler).Properties._NonaKillstreakColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_decakill.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2KillstreakLayerHandler).Properties._DecaKillstreakColor ?? System.Drawing.Color.Empty);

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

        private void abilities_canuse_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2AbilityLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2AbilityLayerHandler).Properties._CanCastAbilityColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void abilities_cannotuse_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2AbilityLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2AbilityLayerHandler).Properties._CanNotCastAbilityColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_doublekill_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2KillstreakLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2KillstreakLayerHandler).Properties._DoubleKillstreakColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_triplekill_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2KillstreakLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2KillstreakLayerHandler).Properties._TripleKillstreakColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_quadkill_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2KillstreakLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2KillstreakLayerHandler).Properties._QuadKillstreakColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_pentakill_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2KillstreakLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2KillstreakLayerHandler).Properties._PentaKillstreakColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_hexakill_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2KillstreakLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2KillstreakLayerHandler).Properties._HexaKillstreakColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_septakill_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2KillstreakLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2KillstreakLayerHandler).Properties._SeptaKillstreakColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_octakill_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2KillstreakLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2KillstreakLayerHandler).Properties._OctaKillstreakColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_nonakill_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2KillstreakLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2KillstreakLayerHandler).Properties._NonaKillstreakColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_decakill_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2KillstreakLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2KillstreakLayerHandler).Properties._DecaKillstreakColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }
    }
}
