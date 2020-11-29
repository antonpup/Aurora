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

    /// <summary>
    /// Formats the passed number based on the given format string.
    /// </summary>
    public class StringFormatConverter : IValueConverter {

        /// <summary>The format to pass to <see cref="string.Format(string, object)"/> along with the value.</summary>
        public string Format { get; set; } = "{0}";

        /// <summary>A multiplier that the incoming number will be multiplied by.</summary>
        public double Multiplier { get; set; } = 1;

        // Returns an empty string if passed null or non-numeric value
        // If a string parameter is passed, that will be used instead of the format property
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => TypeUtils.IsNumericType(value) == true ? string.Format(parameter is string s ? s : Format, System.Convert.ToDouble(value) * Multiplier) : ""; 

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
