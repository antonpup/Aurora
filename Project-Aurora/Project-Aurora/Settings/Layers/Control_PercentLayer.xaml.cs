using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Aurora.Settings.Layers {

    public partial class Control_PercentLayer : UserControl {

        public Control_PercentLayer() {
            InitializeComponent();
        }

        public Control_PercentLayer(PercentLayerHandler context) : this() {
            DataContext = context;
        }

        internal void SetProfile(Profiles.Application profile) {
            variablePicker.Application = maxVariablePicker.Application = profile;
        }
    }

    public class IntegerDoublePercentConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is double d ? System.Convert.ToInt32(d * 100) : 0;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is int i ? i / 100d : 0;
    }
}
