using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Utils {

    /// <summary>
    /// A GridHelper utility that provides "Columns" and "Rows" attached properties to the Grid control.
    /// These properties are shorthand for adding row and column definitions.
    /// <para>Usage: <code>&lt;Grid u:GridHelper.Columns="Auto,1*,2*,30px"&gt;</code></para>
    /// </summary>
    public class GridHelper {

        // Converter that XAML uses to convert a string to a GridLength (e.g. "1*", "230px", "Auto", etc)
        private static TypeConverter gridLengthConverter = TypeDescriptor.GetConverter(typeof(GridLength));
        private static GridLength ParseGridLength(string s) => (GridLength)gridLengthConverter.ConvertFromString(s);


        // Columns
        public static string GetColumns(DependencyObject obj) => (string)obj.GetValue(ColumnsProperty);
        public static void SetColumns(DependencyObject obj, string value) => obj.SetValue(ColumnsProperty, value);

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.RegisterAttached("Columns", typeof(string), typeof(GridHelper), new PropertyMetadata("", ColumnsChanged));

        public static void ColumnsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            if (!(obj is Grid grid)) return;
            grid.ColumnDefinitions.Clear();
            foreach (var str in ((string)e.NewValue).Split(','))
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = ParseGridLength(str) });
        }


        // Rows
        public static string GetRows(DependencyObject obj) => (string)obj.GetValue(RowsProperty);
        public static void SetRows(DependencyObject obj, string value) => obj.SetValue(RowsProperty, value);

        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.RegisterAttached("Rows", typeof(string), typeof(GridHelper), new PropertyMetadata("", RowsChanged));

        public static void RowsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            if (!(obj is Grid grid)) return;
            grid.RowDefinitions.Clear();
            foreach (var str in ((string)e.NewValue).Split(','))
                grid.RowDefinitions.Add(new RowDefinition { Height = ParseGridLength(str) });
        }
    }
}
