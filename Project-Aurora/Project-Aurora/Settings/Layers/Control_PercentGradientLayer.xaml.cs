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
    /// Interaction logic for Control_PercentGradientLayer.xaml
    /// </summary>
    public partial class Control_PercentGradientLayer : UserControl
    {
        private bool settingsset = false;
        private bool profileset = false;

        public Control_PercentGradientLayer()
        {
            InitializeComponent();
        }

        public Control_PercentGradientLayer(PercentGradientLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is PercentGradientLayerHandler && !settingsset)
            {
                this.ComboBox_effect_type.SelectedIndex = (int)(this.DataContext as PercentGradientLayerHandler).Properties._PercentType;
                this.updown_blink_value.Value = (int)((this.DataContext as PercentGradientLayerHandler).Properties._BlinkThreshold * 100);
                this.CheckBox_threshold_reverse.IsChecked = (this.DataContext as PercentGradientLayerHandler).Properties._BlinkDirection;
                this.KeySequence_keys.Sequence = (this.DataContext as PercentGradientLayerHandler).Properties._Sequence;

                Brush brush = (this.DataContext as PercentGradientLayerHandler).Properties._Gradient.GetMediaBrush();
                try
                {
                    this.gradient_editor.Brush = brush;
                }
                catch (Exception exc)
                {
                    Global.logger.Error("Could not set brush, exception: " + exc);
                }

                settingsset = true;
            }
        }

        internal void SetApplication(Profiles.Application profile)
        {
            VariablePath.Application = MaxVariablePath.Application = profile;
        }

        private void KeySequence_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is PercentGradientLayerHandler && sender is Aurora.Controls.KeySequence)
                (this.DataContext as PercentGradientLayerHandler).Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }

        private void Gradient_editor_BrushChanged(object sender, ColorBox.BrushChangedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is PercentGradientLayerHandler && sender is ColorBox.ColorBox)
                (this.DataContext as PercentGradientLayerHandler).Properties._Gradient = new EffectsEngine.EffectBrush((sender as ColorBox.ColorBox).Brush);
        }

        private void ColorPicker_progressColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is PercentGradientLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as PercentGradientLayerHandler).Properties._PrimaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_backgroundColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is PercentGradientLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as PercentGradientLayerHandler).Properties._SecondaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void updown_blink_value_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && settingsset && this.DataContext is PercentGradientLayerHandler && sender is Xceed.Wpf.Toolkit.IntegerUpDown && (sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value.HasValue)
                (this.DataContext as PercentGradientLayerHandler).Properties._BlinkThreshold = (sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value.Value / 100.0D;
        }

        private void ComboBox_effect_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is PercentGradientLayerHandler && sender is ComboBox)
            {
                (this.DataContext as PercentGradientLayerHandler).Properties._PercentType = (PercentEffectType)Enum.Parse(typeof(PercentEffectType), (sender as ComboBox).SelectedIndex.ToString());
            }
        }

        private void CheckBox_threshold_reverse_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is PercentGradientLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
            {
                (this.DataContext as PercentGradientLayerHandler).Properties._BlinkDirection = (sender as CheckBox).IsChecked.Value;
            }
        }
    }
}
