using Aurora.Controls;
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
    /// Interaction logic for Control_ETS2BeaconLayer.xaml
    /// </summary>
    public partial class Control_ETS2BeaconLayer : UserControl {

        private bool settingsset = false;

        public Control_ETS2BeaconLayer() {
            InitializeComponent();
        }

        public Control_ETS2BeaconLayer(ETS2BeaconLayerHandler datacontext) {
            this.DataContext = datacontext;
            InitializeComponent();
        }

        private ETS2BeaconLayerHandler context => (ETS2BeaconLayerHandler)this.DataContext;
        private bool isReady => IsLoaded && settingsset && this.DataContext is ETS2BeaconLayerHandler;

        public void SetSettings() {
            if (this.DataContext is ETS2BeaconLayerHandler && !settingsset) {
                this.lightMode.SelectedItem = context.Properties._BeaconStyle;
                this.speedSlider.Value = (double)context.Properties._Speed;
                this.speedSlider.IsEnabled = context.Properties._BeaconStyle == ETS2_BeaconStyle.Simple_Flash;
                this.beaconColorPicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(context.Properties._PrimaryColor ?? System.Drawing.Color.Empty);
                this.keyPicker.Sequence = context.Properties._Sequence;
                settingsset = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            SetSettings();
            /*lightMode.Items.Add(ETS2_BeaconStyle.Simple_Flash);
            lightMode.Items.Add(ETS2_BeaconStyle.Two_Half);
            lightMode.Items.Add(ETS2_BeaconStyle.Fancy_Flash);
            lightMode.Items.Add(ETS2_BeaconStyle.Flip_Flop);*/
            this.Loaded -= UserControl_Loaded;
        }

        private void lightMode_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (isReady && sender is ComboBox) {
                context.Properties._BeaconStyle = (ETS2_BeaconStyle)(sender as ComboBox).SelectedItem;
                speedSlider.IsEnabled = context.Properties._BeaconStyle == ETS2_BeaconStyle.Simple_Flash;
            }
        }

        private void speedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (isReady && sender is Slider)
                context.Properties._Speed = (float)(sender as Slider).Value;
        }

        private void beaconColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (isReady && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                context.Properties._PrimaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void keyPicker_SequenceUpdated(object sender, EventArgs e) {
            if (isReady && sender is KeySequence)
                context.Properties._Sequence = (sender as KeySequence).Sequence;
        }
    }
}
