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
using Xceed.Wpf.Toolkit;

namespace Aurora.Settings.Layers {
    /// <summary>
    /// Interaction logic for Control_ToggleKeyLayer.xaml
    /// </summary>
    public partial class Control_ToggleKeyLayer : UserControl {

        private bool settingsset = false;

        public Control_ToggleKeyLayer() {
            InitializeComponent();
        }

        public Control_ToggleKeyLayer(ToggleKeyLayerHandler context) {
            InitializeComponent();
            this.DataContext = context;
        }

        public void SetSettings() {
            if (DataContext is ToggleKeyLayerHandler && !settingsset) {
                ToggleKeyLayerHandler context = (ToggleKeyLayerHandler)DataContext;

                defaultColor.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(context.Properties._PrimaryColor ?? System.Drawing.Color.Empty);
                toggleColor.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(context.Properties._SecondaryColor ?? System.Drawing.Color.Empty);
                triggerKeyList.Keybinds = context.Properties._TriggerKeys;
                KeySequence_Keys.Sequence = context.Properties._Sequence;

                settingsset = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            SetSettings();
            Loaded -= UserControl_Loaded;
        }

        private void defaultColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (IsLoaded && settingsset && DataContext is ToggleKeyLayerHandler && (sender as ColorPicker).SelectedColor.HasValue)
                (DataContext as ToggleKeyLayerHandler).Properties._PrimaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
        }

        private void toggleColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (IsLoaded && settingsset && DataContext is ToggleKeyLayerHandler && (sender as ColorPicker).SelectedColor.HasValue)
                (DataContext as ToggleKeyLayerHandler).Properties._SecondaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
        }

        private void triggerKeyList_KeybindsChanged(object sender) {
            if (IsLoaded && settingsset && DataContext is ToggleKeyLayerHandler && sender is Controls.KeyBindList)
                (DataContext as ToggleKeyLayerHandler).Properties._TriggerKeys = (sender as Controls.KeyBindList).Keybinds;

        }

        private void KeySequence_Keys_SequenceUpdated(object sender, EventArgs e) {
            if (IsLoaded && settingsset && DataContext is ToggleKeyLayerHandler && sender is Controls.KeySequence)
                (DataContext as ToggleKeyLayerHandler).Properties._Sequence = (sender as Controls.KeySequence).Sequence;
        }
    }
}
