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

namespace Aurora.Profiles.Minecraft.Layers {
    /// <summary>
    /// Interaction logic for Control_MinecraftRainLayer.xaml
    /// </summary>
    public partial class Control_MinecraftRainLayer : UserControl {

        private bool settingsSet = false;

        public Control_MinecraftRainLayer() {
            InitializeComponent();
        }

        public Control_MinecraftRainLayer(MinecraftRainLayerHandler context) {
            InitializeComponent();
            DataContext = context;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            SetSettings();
            Loaded -= UserControl_Loaded;
        }

        private void SetSettings() {
            if (!settingsSet && DataContext is MinecraftRainLayerHandler) {
                settingsSet = true;
                ColorPicker_RainColor.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((DataContext as MinecraftRainLayerHandler).Properties._PrimaryColor ?? System.Drawing.Color.Empty);
                MinimumIntensity_Stepper.Value = (DataContext as MinecraftRainLayerHandler).Properties._MinimumInterval;
                MaximumIntensity_Stepper.Value = (DataContext as MinecraftRainLayerHandler).Properties._MaximumInterval;
            }
        }

        private void ColorPicker_RainColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (IsLoaded && settingsSet && DataContext is MinecraftRainLayerHandler && e.NewValue.HasValue)
                (DataContext as MinecraftRainLayerHandler).Properties._PrimaryColor = Utils.ColorUtils.MediaColorToDrawingColor(e.NewValue.Value);
        }

        private void MinimumIntensity_Stepper_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (IsLoaded && settingsSet && DataContext is MinecraftBackgroundLayerHandler && (sender as IntegerUpDown).Value.HasValue)
                (DataContext as MinecraftRainLayerHandler).Properties._MinimumInterval = (sender as IntegerUpDown).Value.Value;
        }

        private void MaximumIntensity_Stepper_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (IsLoaded && settingsSet && DataContext is MinecraftBackgroundLayerHandler && (sender as IntegerUpDown).Value.HasValue)
                (DataContext as MinecraftRainLayerHandler).Properties._MaximumInterval = (sender as IntegerUpDown).Value.Value;
        }
    }
}
