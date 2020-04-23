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
                ColorPicker_HudModeCombat.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._HudModeCombatColor ?? System.Drawing.Color.Empty);
                ColorPicker_HudModeDiscovery.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._HudModeDiscoveryColor ?? System.Drawing.Color.Empty);
                ColorPicker_Ui.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._UiColor ?? System.Drawing.Color.Empty);
                ColorPicker_UiAlt.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._UiAltColor ?? System.Drawing.Color.Empty);
                ColorPicker_ShipStuff.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._ShipStuffColor ?? System.Drawing.Color.Empty);
                ColorPicker_Camera.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._CameraColor ?? System.Drawing.Color.Empty);
                ColorPicker_Defence.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._DefenceColor ?? System.Drawing.Color.Empty);
                ColorPicker_Offence.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._OffenceColor ?? System.Drawing.Color.Empty);
                ColorPicker_MovementSpeed.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._MovementSpeedColor ?? System.Drawing.Color.Empty);
                ColorPicker_MovementSecondary.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._MovementSecondaryColor ?? System.Drawing.Color.Empty);
                ColorPicker_Wing.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._WingColor ?? System.Drawing.Color.Empty);
                ColorPicker_Navigation.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._NavigationColor ?? System.Drawing.Color.Empty);
                ColorPicker_ModeEnable.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._ModeEnableColor ?? System.Drawing.Color.Empty);
                ColorPicker_ModeDisable.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((DataContext as EliteDangerousKeyBindsLayerHandler).Properties._ModeDisableColor ?? System.Drawing.Color.Empty);

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

        private void ColorPicker_HudModeCombat_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._HudModeCombatColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_HudModeDiscovery_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._HudModeDiscoveryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Ui_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._UiColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_UiAlt_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._UiAltColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_ShipStuff_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._ShipStuffColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Camera_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._CameraColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Defence_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._DefenceColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Offence_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._OffenceColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_MovementSpeed_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._MovementSpeedColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_MovementSecondary_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._MovementSecondaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Wing_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._WingColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Navigation_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._NavigationColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_ModeEnable_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._ModeEnableColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_ModeDisable_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousKeyBindsLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousKeyBindsLayerHandler).Properties._ModeDisableColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }
    }
}
