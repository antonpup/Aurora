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

namespace Aurora.Profiles.CSGO.Layers
{
    /// <summary>
    /// Interaction logic for Control_CSGOBombLayer.xaml
    /// </summary>
    public partial class Control_CSGOBombLayer : UserControl
    {
        private bool settingsset = false;

        public Control_CSGOBombLayer()
        {
            InitializeComponent();
        }

        public Control_CSGOBombLayer(CSGOBombLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is CSGOBombLayerHandler && !settingsset)
            {
                this.ColorPicker_Flash.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as CSGOBombLayerHandler).Properties._FlashColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Primed.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as CSGOBombLayerHandler).Properties._PrimedColor ?? System.Drawing.Color.Empty);
                this.Checkbox_GradualEffect.IsChecked = (this.DataContext as CSGOBombLayerHandler).Properties._GradualEffect;
                this.Checkbox_DisplayOnPeripherals.IsChecked = (this.DataContext as CSGOBombLayerHandler).Properties._PeripheralUse;
                this.KeySequence_keys.Sequence = (this.DataContext as CSGOBombLayerHandler).Properties._Sequence;

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

        private void ColorPicker_Flash_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOBombLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as CSGOBombLayerHandler).Properties._FlashColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Primed_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOBombLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as CSGOBombLayerHandler).Properties._PrimedColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void Checkbox_GradualEffect_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOBombLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
                (this.DataContext as CSGOBombLayerHandler).Properties._GradualEffect = (sender as CheckBox).IsChecked.Value;
        }

        private void Checkbox_DisplayOnPeripherals_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOBombLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
                (this.DataContext as CSGOBombLayerHandler).Properties._PeripheralUse = (sender as CheckBox).IsChecked.Value;
        }

        private void KeySequence_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOBombLayerHandler && sender is Aurora.Controls.KeySequence)
            {
                (this.DataContext as CSGOBombLayerHandler).Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
            }
        }
    }
}
