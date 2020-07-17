using Aurora.EffectsEngine;
using Aurora.Utils;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aurora.Settings.Layers {

    public partial class Control_RadialLayer : UserControl {

        private readonly RadialLayerHandler handler;

        public Control_RadialLayer(RadialLayerHandler context) {
            DataContext = handler = context;
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            GradientPicker.Brush = new LinearEffectBrush(handler.Properties.Brush.Colors.ToMediaBrush());
            Loaded -= UserControl_Loaded;
        }

        private void GradientPicker_BrushChanged(object sender, ColorBox.BrushChangedEventArgs e) {
            handler.Properties.Brush.Colors = ColorStopCollection.FromMediaBrush(GradientPicker.Brush.GetMediaBrush());
        }
    }
}
