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

        private readonly SimpleParticleLayerHandler handler;

        public Control_ParticleLayer(SimpleParticleLayerHandler context) {
            InitializeComponent();
            DataContext = handler = context;
            presetsCombo.ItemsSource = ParticleLayerPresets.Presets.Select(kvp => new { Text = kvp.Key, ApplyFunc = kvp.Value });
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) => ApplyGradientToEditor();

        private void ApplyGradientToEditor() {
            // Note that I tried using a binding instead of this but since it'd need a IValueConverter which would have to create a new brush and
            // the _values_ on that brush change not the brush itself, the binding was not actually being triggered.
            gradientEditor.Brush = handler.Properties._ParticleColorStops.ToMediaBrush();
        }

        private void GradientEditor_BrushChanged(object sender, ColorBox.BrushChangedEventArgs e) {
            // Set the particle's color stops from the media brush. We cannot pass the media brush directly as it causes issues with UI threading
            handler.Properties._ParticleColorStops = ColorStopCollection.FromMediaBrush(e.Brush);
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e) {
            // Applies the selected preset to the layer properties
            if (presetsCombo.SelectedValue is Action<SimpleParticleLayerProperties> apply && MessageBox.Show("Do you wish to apply this preset? Your current configuration will be overwritten.", "Apply Preset", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                handler.Properties.Default();
                apply(handler.Properties);
                ApplyGradientToEditor(); // Manually update the gradient editor since this can't be handled by bindings
            }
        }
    }

    public class ParticleSpawnLocationsIsRegionConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Equals(value, ParticleSpawnLocations.Region);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
