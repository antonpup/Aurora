using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Aurora.Controls {

    public partial class Window_Prompt : Window {

        public Window_Prompt() {
            InitializeComponent();
            Input.Focus();
        }

        /// <summary>
        /// A function that is executed before the handler of the okay button is executed.
        /// If this method returns false, the dialog box is not closed.
        /// An error message can be set using the <see cref="ErrorMessage"/> dependency property.
        /// </summary>
        public Func<string, bool> Validate { get; set; }

        #region Description Property
        /// <summary>Gets or sets the hint text that is shown to the user.</summary>
        public string Description {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(Window_Prompt), new PropertyMetadata(""));
        #endregion

        #region Text Dependency Property
        /// <summary>Gets or sets the value of the textbox.</summary>
        public string Text {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Window_Prompt), new PropertyMetadata(""));
        #endregion

        #region ErrorMessage Dependency Property
        /// <summary>Gets or sets the value of the error message.</summary>
        public string ErrorMessage {
            get => (string)GetValue(ErrorMessageProperty);
            set => SetValue(ErrorMessageProperty, value);
        }

        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register("ErrorMessage", typeof(string), typeof(Window_Prompt), new PropertyMetadata(""));
        #endregion

        private void OkayButton_Click(object sender, RoutedEventArgs e) {
            if (Validate?.Invoke(Text) != false)
                DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == System.Windows.Input.Key.Enter)
                OkayButton_Click(null, null);
        }
    }


    /// <summary>
    /// If the given string is empty, collapses the element.
    /// </summary>
    public class CollapseIfEmptyConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => string.IsNullOrWhiteSpace(value.ToString()) ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
