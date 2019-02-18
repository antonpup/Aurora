using Aurora.Profiles.Desktop;
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
    /// Interaction logic for Control_InteractiveLayer.xaml
    /// </summary>
    public partial class Control_InteractiveLayer : UserControl
    {
        private bool settingsset = false;

        public Control_InteractiveLayer()
        {
            InitializeComponent();
        }

        public Control_InteractiveLayer(InteractiveLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is InteractiveLayerHandler && !settingsset)
            {
                //this.ColorPicker_primaryColor.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as InteractiveLayerHandler).PrimaryColor);
                //this.KeySequence_keys.Sequence = (this.DataContext as InteractiveLayerHandler).AffectedSequence;

                this.interactive_effects_type.SelectedIndex = (int)(this.DataContext as InteractiveLayerHandler).Properties._InteractiveEffect;
                this.interactive_effects_primary_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as InteractiveLayerHandler).Properties._PrimaryColor ?? System.Drawing.Color.Empty);
                this.interactive_effects_random_primary_color_enabled.IsChecked = (this.DataContext as InteractiveLayerHandler).Properties._RandomPrimaryColor;
                this.interactive_effects_secondary_color_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as InteractiveLayerHandler).Properties._SecondaryColor ?? System.Drawing.Color.Empty);
                this.interactive_effects_random_secondary_color_enabled.IsChecked = (this.DataContext as InteractiveLayerHandler).Properties._RandomSecondaryColor;
                this.interactive_effects_speed_label.Text = "x " + (this.DataContext as InteractiveLayerHandler).Properties._EffectSpeed;
                this.interactive_effects_speed_slider.Value = (float)(this.DataContext as InteractiveLayerHandler).Properties._EffectSpeed;
                this.interactive_effects_width_label.Text = (this.DataContext as InteractiveLayerHandler).Properties._EffectWidth + " px";
                this.interactive_effects_width_slider.Value = (float)(this.DataContext as InteractiveLayerHandler).Properties._EffectWidth;
                this.interactive_effects_start_on_key_up_enabled.IsChecked = (this.DataContext as InteractiveLayerHandler).Properties._WaitOnKeyUp;
                this.usePressBuffer.IsChecked = (this.DataContext as InteractiveLayerHandler).Properties._UsePressBuffer ?? true;
                this.KeySequence_keys.Sequence = (this.DataContext as InteractiveLayerHandler).Properties._Sequence;
                this.KeySequence_keys.FreestyleEnabled = false;

                settingsset = true;
            }
        }

        private void ColorPicker_primaryColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is InteractiveLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            {
                (this.DataContext as InteractiveLayerHandler).Properties._PrimaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
            }
        }

        private void KeySequence_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is InteractiveLayerHandler && sender is Aurora.Controls.KeySequence)
            {
                (this.DataContext as InteractiveLayerHandler).Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }


        private void interactive_effects_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is InteractiveLayerHandler && sender is ComboBox)
            {
                (this.DataContext as InteractiveLayerHandler).Properties._InteractiveEffect = (InteractiveEffects)Enum.Parse(typeof(InteractiveEffects), (sender as ComboBox).SelectedIndex.ToString());
            }
        }

        private void interactive_effects_primary_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is InteractiveLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            {
                (this.DataContext as InteractiveLayerHandler).Properties._PrimaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
            }
        }

        private void interactive_effects_random_primary_color_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is InteractiveLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
            {
                (this.DataContext as InteractiveLayerHandler).Properties._RandomPrimaryColor = (sender as CheckBox).IsChecked.Value;
            }
        }

        private void interactive_effects_secondary_color_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is InteractiveLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            {
                (this.DataContext as InteractiveLayerHandler).Properties._SecondaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
            }
        }

        private void interactive_effects_random_secondary_color_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is InteractiveLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
            {
                (this.DataContext as InteractiveLayerHandler).Properties._RandomSecondaryColor = (sender as CheckBox).IsChecked.Value;
            }
        }

        private void interactive_effects_speed_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && settingsset && this.DataContext is InteractiveLayerHandler && sender is Slider)
            {
                (this.DataContext as InteractiveLayerHandler).Properties._EffectSpeed = (float)(sender as Slider).Value;

                if (this.interactive_effects_speed_label is TextBlock)
                    this.interactive_effects_speed_label.Text = "x " + this.interactive_effects_speed_slider.Value;
            }
        }

        private void interactive_effects_width_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && settingsset && this.DataContext is InteractiveLayerHandler && sender is Slider)
            {
                (this.DataContext as InteractiveLayerHandler).Properties._EffectWidth = (int)(sender as Slider).Value;

                if (this.interactive_effects_width_label is TextBlock)
                    this.interactive_effects_width_label.Text = this.interactive_effects_width_slider.Value + " px";
            }
        }

        private void interactive_effects_start_on_key_up_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is InteractiveLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
            {
                (this.DataContext as InteractiveLayerHandler).Properties._WaitOnKeyUp = (sender as CheckBox).IsChecked.Value;
            }
        }

        private void usePressBuffer_Checked(object sender, RoutedEventArgs e) {
            if (IsLoaded && settingsset && this.DataContext is InteractiveLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
                (this.DataContext as InteractiveLayerHandler).Properties._UsePressBuffer = (sender as CheckBox).IsChecked.Value;
        }
    }
}
