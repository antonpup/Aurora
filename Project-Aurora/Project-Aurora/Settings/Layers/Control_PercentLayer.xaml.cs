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
    /// Interaction logic for Control_PercentLayer.xaml
    /// </summary>
    public partial class Control_PercentLayer : UserControl
    {
        private bool settingsset = false;
        private bool profileset = false;

        public Control_PercentLayer()
        {
            InitializeComponent();
        }

        public Control_PercentLayer(PercentLayerHandler datacontext) : this()
        {
            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is PercentLayerHandler && !settingsset)
            {
                this.ComboBox_variable.Text = (this.DataContext as PercentLayerHandler).Properties._VariablePath;
                this.ComboBox_max_variable.Text = (this.DataContext as PercentLayerHandler).Properties._MaxVariablePath;
                this.ColorPicker_progressColor.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as PercentLayerHandler).Properties._PrimaryColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_backgroundColor.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as PercentLayerHandler).Properties._SecondaryColor ?? System.Drawing.Color.Empty);
                this.ComboBox_effect_type.SelectedIndex = (int)(this.DataContext as PercentLayerHandler).Properties._PercentType;
                this.updown_blink_value.Value = (int)((this.DataContext as PercentLayerHandler).Properties._BlinkThreshold * 100);
                this.CheckBox_threshold_reverse.IsChecked = (this.DataContext as PercentLayerHandler).Properties._BlinkDirection;
                this.KeySequence_keys.Sequence = (this.DataContext as PercentLayerHandler).Properties._Sequence;
                settingsset = true;
            }
        }

        internal void SetProfile(Profiles.Application profile)
        {
            if (profile != null && !profileset && profile.ParameterLookup != null)
            {
                var var_types_numerical = profile.ParameterLookup?.Where(kvp => Utils.TypeUtils.IsNumericType(kvp.Value.Item1));

                this.ComboBox_variable.Items.Clear();
                foreach (var item in var_types_numerical)
                    this.ComboBox_variable.Items.Add(item.Key);

                this.ComboBox_max_variable.Items.Clear();
                foreach (var item in var_types_numerical)
                    this.ComboBox_max_variable.Items.Add(item.Key);

                profileset = true;
            }
            settingsset = false;
            this.SetSettings();
        }

        private void KeySequence_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is PercentLayerHandler && sender is Aurora.Controls.KeySequence)
                (this.DataContext as PercentLayerHandler).Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }

        private void ComboBox_variable_TextChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is PercentLayerHandler && sender is ComboBox)
                (this.DataContext as PercentLayerHandler).Properties._VariablePath = (sender as ComboBox).Text;
        }

        private void ComboBox_max_variable_TextChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is PercentLayerHandler && sender is ComboBox)
                (this.DataContext as PercentLayerHandler).Properties._MaxVariablePath = (sender as ComboBox).Text;
        }

        private void ColorPicker_progressColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is PercentLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as PercentLayerHandler).Properties._PrimaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_backgroundColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is PercentLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as PercentLayerHandler).Properties._SecondaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void updown_blink_value_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && settingsset && this.DataContext is PercentLayerHandler && sender is Xceed.Wpf.Toolkit.IntegerUpDown && (sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value.HasValue)
                (this.DataContext as PercentLayerHandler).Properties._BlinkThreshold = (sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value.Value / 100.0D;
        }

        private void ComboBox_effect_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is PercentLayerHandler && sender is ComboBox)
            {
                (this.DataContext as PercentLayerHandler).Properties._PercentType = (PercentEffectType)Enum.Parse(typeof(PercentEffectType), (sender as ComboBox).SelectedIndex.ToString());
            }
        }

        private void CheckBox_threshold_reverse_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is PercentLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
            {
                (this.DataContext as PercentLayerHandler).Properties._BlinkDirection = (sender as CheckBox).IsChecked.Value;
            }
        }

        private void CheckBox_blinkbackground_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is PercentLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
            {
                (this.DataContext as PercentLayerHandler).Properties._BlinkBackground = (sender as CheckBox).IsChecked.Value;
            }
        }
    }
}
