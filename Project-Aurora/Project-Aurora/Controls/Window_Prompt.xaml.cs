using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Controls {

    public partial class Window_Prompt : Window {

        public Window_Prompt() {
            InitializeComponent();
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
    }
}
