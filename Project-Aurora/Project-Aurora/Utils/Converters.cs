using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Aurora.Utils {

    /// <summary>
    /// Converter that returns a boolean based on whether or not the given value is null or not.
    /// Does not support "ConvertBack".
    /// </summary>
    public class IsNullToBooleanConverter : IValueConverter {

        /// <summary>This is the value to return when the given value is null. Will return the opposite if the value is non-null.</summary>
        public bool ReturnValWhenNull { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !(value == null ^ ReturnValWhenNull);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    /// <summary>Simple converter that returns true if the given value is non-null.</summary>
    public class IsNullToVisibilityConverter : IValueConverter {

        public Visibility ReturnValWhenNull { get; set; } = Visibility.Collapsed;
        public Visibility ReturnValWhenNonNull { get; set; } = Visibility.Visible;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value == null ? ReturnValWhenNull : ReturnValWhenNonNull;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    /// <summary>
    /// Converter that allows specification of multiple other converters.
    /// Does not support "ConvertBack".
    /// </summary>
    /// <remarks>Code by Garath Evans (https://bit.ly/2HAdFvW)</remarks>
    public class ValueConverterGroup : System.Collections.Generic.List<IValueConverter>, IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => this.Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
