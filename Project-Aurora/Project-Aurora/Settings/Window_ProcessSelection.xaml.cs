using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aurora.Settings {
    public partial class Window_ProcessSelection : Window {

        public Window_ProcessSelection() {
            InitializeComponent();

            // Scan running processes and add them to a list
            List<RunningProcess> processList = new List<RunningProcess>();
            foreach (var p in Process.GetProcesses())
                try {
                    // Get the exe name
                    string name = System.IO.Path.GetFileName(p.MainModule.FileName);
                    // Check if we've already got an exe by that name, if not add it
                    if (!processList.Any(x => x.Name == name))
                        processList.Add(new RunningProcess {
                            Name = name,
                            Path = p.MainModule.FileName,
                            Icon = System.Drawing.Icon.ExtractAssociatedIcon(p.MainModule.FileName)
                        });
                } catch { }

            // Sort the list, set the ListBox control to use that list
            RunningProcessList.ItemsSource = processList.OrderBy(p => p.Name);
            RunningProcessList.SelectedIndex = 0;

            // CollectionViewSorce to provide search/filter feature
            CollectionViewSource.GetDefaultView(RunningProcessList.ItemsSource).Filter = RunningProcessFilterPredicate;
            RunningProcessListFilterText.Focus();
        }

        /// <summary>Gets or sets the okay button's label. Default: "Select process".</summary>
        public string ButtonLabel {
            get { return okayButton.Content.ToString(); }
            set { okayButton.Content = value; }
        }

        /// <summary>Dictates whether to check if a path entered by the user in the "Browse for executable" tab exists.
        /// The user can type text here and so may point to an exe that does not exist. Default: false.</summary>
        public bool CheckCustomPathExists { get; set; } = false;

        /// <summary>The name and extension of the application the user has chosen (e.g. 'Aurora.exe').</summary>
        public string ChosenExecutableName { get; private set; } = "";

        /// <summary>The full path of the process the user has chosen (e.g. 'C:\Program Files\Aurora\Aurora.exe').</summary>
        public string ChosenExecutablePath { get; private set; } = "";

        /// <summary>
        /// Handler for the browse button on the custom exe path tab. Sets
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseButton_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog() {
                AddExtension = true,
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                Multiselect = false
            };
            if (dialog.ShowDialog() == true) // requires "== true" because ShowDialog is a bool?, so doing "if (dialog.ShowDialog())" is invalid
                ProcessBrowseResult.Text = dialog.FileName;
        }

        /// <summary>
        /// Updates the running process filter when the textbox is changed.
        /// </summary>
        private void RunningListFilter_TextChanged(object sender, TextChangedEventArgs e) {
            CollectionViewSource.GetDefaultView(RunningProcessList.ItemsSource).Refresh();
            if (RunningProcessList.SelectedIndex == -1)
                RunningProcessList.SelectedIndex = 0;
        }

        /// <summary>
        /// Method that makes Up/Down arrow keys when focussed on the RunningListFilter change the selection of the running list element.
        /// This means you don't have to click on the item when you are typing in a filter.
        /// We do not need to handle Enter key here as it is done by setting the OK button "IsDefault" to true.
        /// </summary>
        private void RunningProcessListFilterText_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Up)
                RunningProcessList.SelectedIndex = Math.Max(RunningProcessList.SelectedIndex - 1, 0);
            else if (e.Key == Key.Down)
                RunningProcessList.SelectedIndex = RunningProcessList.SelectedIndex + 1; // Automatically clamped
        }

        /// <summary>
        /// Filter that is run on each item in the running process list (List&lt;RunningProcess&gt;) and returns a bool
        /// indicating whether it should appear on the list.
        /// </summary>
        private bool RunningProcessFilterPredicate(object item) {
            return ((RunningProcess)item).Name.IndexOf(RunningProcessListFilterText.Text, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        /// <summary>
        /// Handler for when the confimation button is clicked. Handles closing and informing the result of the dialog.
        /// </summary>
        private void OkayButton_Click(object sender, RoutedEventArgs e) {
            // If the user is on the running process list tab
            if (MainTabControl.SelectedIndex == 0) {
                if (RunningProcessList.SelectedItem == null) return; // Cannot OK if there is no item selected
                ChosenExecutableName = ((RunningProcess)RunningProcessList.SelectedItem).Name;
                ChosenExecutablePath = ((RunningProcess)RunningProcessList.SelectedItem).Path;

                // Else if user is on browse tab
            } else {
                string exe = ProcessBrowseResult.Text;
                if (string.IsNullOrWhiteSpace(exe)) return; // Cannot OK if there is no text entered
                if (CheckCustomPathExists && !File.Exists(exe)) return; // Cannot OK if we require validation and the file doesn't exist
                ChosenExecutableName = exe.Substring(exe.LastIndexOfAny(new[] { '/', '\\' }) + 1); // Get just the exe name
                ChosenExecutablePath = exe;
            }

            // Close the window and set result as successful
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Handler for when the cancel button is clicked. Closes the window.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }


    /// <summary>
    /// Converts an Icon into a WPF-compatible BitmapSource.
    /// </summary>
    class IconToImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            Icon ico = (Icon)value;
            // Taken from https://stackoverflow.com/a/51438725/1305670
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }

    /// <summary>
    /// Container for a Running Process definition.
    /// </summary>
    struct RunningProcess {
        public string Name { get; set; }
        public string Path { get; set; }
        public Icon Icon { get; set; }
    }
}
