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
    /// Interaction logic for Control_Dota2RespawnLayer.xaml
    /// </summary>
    public partial class Control_Dota2RespawnLayer : UserControl
    {
        private bool settingsset = false;

        public Control_Dota2RespawnLayer()
        {
            InitializeComponent();
        }

        public Control_Dota2RespawnLayer(Dota2RespawnLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is Dota2RespawnLayerHandler && !settingsset)
            {
                this.ColorPicker_background.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2RespawnLayerHandler).Properties._BackgroundColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_respawn.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2RespawnLayerHandler).Properties._RespawnColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_respawning.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2RespawnLayerHandler).Properties._RespawningColor ?? System.Drawing.Color.Empty);
                this.KeySequence_sequence.Sequence = (this.DataContext as Dota2RespawnLayerHandler).Properties._Sequence;

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

        private void ColorPicker_background_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2RespawnLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2RespawnLayerHandler).Properties._BackgroundColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);

        }

        private void ColorPicker_respawn_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2RespawnLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2RespawnLayerHandler).Properties._RespawnColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_respawning_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2RespawnLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2RespawnLayerHandler).Properties._RespawningColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void KeySequence_sequence_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2RespawnLayerHandler && sender is Aurora.Controls.KeySequence)
                (this.DataContext as Dota2RespawnLayerHandler).Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
        }
    }
}
