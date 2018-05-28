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

namespace Aurora.Profiles.ETS2.Layers {
    /// <summary>
    /// Interaction logic for Control_ETS2BlinkerLayer.xaml
    /// </summary>
    public partial class Control_ETS2BlinkerLayer : UserControl {

        private bool settingsset = false;

        public Control_ETS2BlinkerLayer() {
            InitializeComponent();
        }

        public Control_ETS2BlinkerLayer(ETS2BlinkerLayerHandler datacontext) {
            InitializeComponent();
            this.DataContext = datacontext;
        }

        private ETS2BlinkerLayerHandler context => (ETS2BlinkerLayerHandler)this.DataContext;

        public void SetSettings() {
            if (this.DataContext is ETS2BlinkerLayerHandler && !settingsset) {
                this.ColorPicker_BlinkerOn.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(context.Properties._BlinkerOnColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_BlinkerOff.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(context.Properties._BlinkerOffColor ?? System.Drawing.Color.Empty);
                this.LeftBlinker_keys.Sequence = context.Properties._LeftBlinkerSequence;
                this.RightBlinker_keys.Sequence = context.Properties._RightBlinkerSequence;
                settingsset = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            SetSettings();
            this.Loaded -= UserControl_Loaded;
        }

        private void ColorPicker_BlinkerOn_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (IsLoaded && settingsset && this.DataContext is ETS2BlinkerLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                context.Properties._BlinkerOnColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_BlinkerOff_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (IsLoaded && settingsset && this.DataContext is ETS2BlinkerLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                context.Properties._BlinkerOffColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void LeftBlinker_keys_SequenceUpdated(object sender, EventArgs e) {
            if (IsLoaded && settingsset && this.DataContext is ETS2BlinkerLayerHandler && sender is Aurora.Controls.KeySequence)
                context.Properties._LeftBlinkerSequence = (sender as Aurora.Controls.KeySequence).Sequence;
        }

        private void RightBlinker_keys_SequenceUpdated(object sender, EventArgs e) {
            if (IsLoaded && settingsset && this.DataContext is ETS2BlinkerLayerHandler && sender is Aurora.Controls.KeySequence)
                context.Properties._RightBlinkerSequence = (sender as Aurora.Controls.KeySequence).Sequence;
        }
    }
}
