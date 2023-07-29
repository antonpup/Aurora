using Aurora.Utils;
using System.Windows.Controls;

namespace Aurora.Settings.Layers.Controls {

    public partial class Control_RadialLayer
    {

        private readonly RadialLayerHandler handler;

        public Control_RadialLayer(RadialLayerHandler context) {
            DataContext = handler = context;
            InitializeComponent();
        }

        private void UserControl_Loaded(object? sender, System.Windows.RoutedEventArgs e) {
            GradientPicker.Brush = handler.Properties.Brush.Colors.ToMediaBrush();
            Loaded -= UserControl_Loaded;
        }

        private void GradientPicker_BrushChanged(object? sender, ColorBox.BrushChangedEventArgs e) {
            handler.Properties.Brush.Colors = ColorStopCollection.FromMediaBrush(GradientPicker.Brush);
            handler.Properties.OnPropertiesChanged(this);
        }
    }
}
