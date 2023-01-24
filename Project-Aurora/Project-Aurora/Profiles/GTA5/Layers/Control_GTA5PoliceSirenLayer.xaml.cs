using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Aurora.Controls;
using Aurora.Utils;
using Xceed.Wpf.Toolkit;

namespace Aurora.Profiles.GTA5.Layers
{
    /// <summary>
    /// Interaction logic for Control_GTA5PoliceSirenLayer.xaml
    /// </summary>
    public partial class Control_GTA5PoliceSirenLayer : UserControl
    {
        private bool settingsset;

        public Control_GTA5PoliceSirenLayer()
        {
            InitializeComponent();
        }

        public Control_GTA5PoliceSirenLayer(GTA5PoliceSirenLayerHandler datacontext)
        {
            InitializeComponent();

            DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (DataContext is GTA5PoliceSirenLayerHandler && !settingsset)
            {
                ColorPicker_LeftSiren.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as GTA5PoliceSirenLayerHandler).Properties.LeftSirenColor);
                ColorPicker_RightSiren.SelectedColor = ColorUtils.DrawingColorToMediaColor((DataContext as GTA5PoliceSirenLayerHandler).Properties.RightSirenColor);
                ComboBox_SirenEffectType.SelectedValue = (DataContext as GTA5PoliceSirenLayerHandler).Properties.SirenType;
                Checkbox_DisplayOnPeripherals.IsChecked = (DataContext as GTA5PoliceSirenLayerHandler).Properties.PeripheralUse;
                KeySequence_LeftSiren.Sequence = (DataContext as GTA5PoliceSirenLayerHandler).Properties.LeftSirenSequence;
                KeySequence_RightSiren.Sequence = (DataContext as GTA5PoliceSirenLayerHandler).Properties.RightSirenSequence;

                settingsset = true;
            }
        }

        internal void SetProfile(Application profile)
        {
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            Loaded -= UserControl_Loaded;
        }

        private void ColorPicker_LeftSiren_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && DataContext is GTA5PoliceSirenLayerHandler && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
                (DataContext as GTA5PoliceSirenLayerHandler).Properties.LeftSirenColor = ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_RightSiren_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && DataContext is GTA5PoliceSirenLayerHandler && sender is ColorPicker && (sender as ColorPicker).SelectedColor.HasValue)
                (DataContext as GTA5PoliceSirenLayerHandler).Properties.RightSirenColor = ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
        }

        private void Checkbox_DisplayOnPeripherals_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && DataContext is GTA5PoliceSirenLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
                (DataContext as GTA5PoliceSirenLayerHandler).Properties.PeripheralUse = (sender as CheckBox).IsChecked.Value;
        }

        private void KeySequence_LeftSiren_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && DataContext is GTA5PoliceSirenLayerHandler && sender is KeySequence)
            {
                (DataContext as GTA5PoliceSirenLayerHandler).Properties.LeftSirenSequence = (sender as KeySequence).Sequence;
            }
        }

        private void KeySequence_RightSiren_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && DataContext is GTA5PoliceSirenLayerHandler && sender is KeySequence)
            {
                (DataContext as GTA5PoliceSirenLayerHandler).Properties.RightSirenSequence = (sender as KeySequence).Sequence;
            }
        }

        private void ComboBox_SirenEffectType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && settingsset && DataContext is GTA5PoliceSirenLayerHandler && sender is ComboBox)
            {
                (DataContext as GTA5PoliceSirenLayerHandler).Properties.SirenType = (GTA5_PoliceEffects)((sender as ComboBox).SelectedValue);
            }
        }
    }
}
