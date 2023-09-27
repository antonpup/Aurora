using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Aurora.Utils;

namespace Aurora.Profiles.EliteDangerous.Layers
{
    /// <summary>
    /// Interaction logic for Control_EliteDangerousKeyBindsLayer.xaml
    /// </summary>
    public partial class Control_EliteDangerousKeyBindsLayer : UserControl
    {
        private bool settingsset = false;

        public Control_EliteDangerousKeyBindsLayer()
        {
            InitializeComponent();
        }

        public Control_EliteDangerousKeyBindsLayer(EliteDangerousKeyBindsLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (DataContext is EliteDangerousKeyBindsLayerHandler && !settingsset)
            {
                ColorPicker_HudModeCombat.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._HudModeCombatColor ?? System.Drawing.Color.Empty);
                ColorPicker_HudModeDiscovery.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._HudModeDiscoveryColor ?? System.Drawing.Color.Empty);
                ColorPicker_Ui.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._UiColor ?? System.Drawing.Color.Empty);
                ColorPicker_UiAlt.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._UiAltColor ?? System.Drawing.Color.Empty);
                ColorPicker_ShipStuff.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._ShipStuffColor ?? System.Drawing.Color.Empty);
                ColorPicker_Camera.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._CameraColor ?? System.Drawing.Color.Empty);
                ColorPicker_Defence.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._DefenceColor ?? System.Drawing.Color.Empty);
                ColorPicker_Offence.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._OffenceColor ?? System.Drawing.Color.Empty);
                ColorPicker_MovementSpeed.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._MovementSpeedColor ?? System.Drawing.Color.Empty);
                ColorPicker_MovementSecondary.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._MovementSecondaryColor ?? System.Drawing.Color.Empty);
                ColorPicker_Wing.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._WingColor ?? System.Drawing.Color.Empty);
                ColorPicker_Navigation.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._NavigationColor ?? System.Drawing.Color.Empty);
                ColorPicker_ModeEnable.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._ModeEnableColor ?? System.Drawing.Color.Empty);
                ColorPicker_ModeDisable.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._ModeDisableColor ?? System.Drawing.Color.Empty);

                settingsset = true;
            }
        }

        internal void SetProfile(Application profile)
        {
        }

        private void UserControl_Loaded(object? sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }

        private void ColorPicker_HudModeCombat_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._HudModeCombatColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_HudModeDiscovery_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._HudModeDiscoveryColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Ui_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._UiColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_UiAlt_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._UiAltColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_ShipStuff_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._ShipStuffColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Camera_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._CameraColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Defence_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._DefenceColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Offence_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._OffenceColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_MovementSpeed_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._MovementSpeedColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_MovementSecondary_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._MovementSecondaryColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Wing_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._WingColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Navigation_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._NavigationColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_ModeEnable_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._ModeEnableColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_ModeDisable_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._ModeDisableColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }
    }
}
