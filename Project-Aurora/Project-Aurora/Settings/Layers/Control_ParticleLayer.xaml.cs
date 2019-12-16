using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Aurora.Settings.Layers {
    /// <summary>
    /// Interaction logic for Control_ParticleLayer.xaml
    /// </summary>
    public partial class Control_ParticleLayer : UserControl {

        private readonly ParticleLayerHandler handler;

        public Control_ParticleLayer(ParticleLayerHandler context) {
            InitializeComponent();
            DataContext = handler = context;
            presetsCombo.ItemsSource = ParticleLayerPresets.Presets.Select(kvp => new { Text = kvp.Key, ApplyFunc = kvp.Value });
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            // Create a temporary Media brush from the particle's color stops
            gradientEditor.Brush = handler.Properties._ParticleColorStops.Count == 1
                ? (Brush)new SolidColorBrush(handler.Properties._ParticleColorStops.First().color.ToMediaColor())
                : new LinearGradientBrush(new GradientStopCollection(handler.Properties._ParticleColorStops.Select(t => new GradientStop(t.color.ToMediaColor(), t.offset))));
        }

        private void GradientEditor_BrushChanged(object sender, ColorBox.BrushChangedEventArgs e) {
            // Set the particle's color stops from the media brush. We cannot pass the media brush directly as it causes issues with UI threading
            if (e.Brush is GradientBrush gb)
                handler.Properties._ParticleColorStops = gb.GradientStops.Select(gs => (gs.Color.ToDrawingColor(), (float)gs.Offset)).ToList();
            else if (e.Brush is SolidColorBrush sb)
                handler.Properties._ParticleColorStops = new List<(System.Drawing.Color color, float offset)> { (sb.Color.ToDrawingColor(), 0f) };
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e) {
            // Applies the selected preset to the layer properties
            if (presetsCombo.SelectedValue is Action<ParticleLayerProperties> apply && MessageBox.Show("Do you wish to apply this preset? Your current configuration will be overwritten.", "Apply Preset", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                handler.Properties.Default();
                apply(handler.Properties);
            }
        }
    }

    public class ParticleSpawnLocationsIsRegionConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Equals(value, ParticleSpawnLocations.Region);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
