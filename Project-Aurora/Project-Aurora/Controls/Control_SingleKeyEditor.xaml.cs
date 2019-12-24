using Aurora.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using Keys = System.Windows.Forms.Keys;

namespace Aurora.Controls {
    /// <summary>
    /// Interaction logic for Control_SingleKeyEditor.xaml
    /// </summary>
    public partial class Control_SingleKeyEditor : UserControl {

        /// <summary>A reference to the editor that is currently listening for a keypress</summary>
        private static Control_SingleKeyEditor listeningEditor;

        // Static constructor so that we only have to add a input event listener once.
        static Control_SingleKeyEditor() {
            Global.InputEvents.KeyDown += InputEvents_KeyDown;
        }

        // Instance constructor to create UI elements
        public Control_SingleKeyEditor() {
            InitializeComponent();
            DataContext = this;
        }

        // Assign or unassign the `listeningEditor` from this UserControl
        private void AssignButton_Click(object sender, RoutedEventArgs e) {
            var assigning = listeningEditor != this;
            listeningEditor?.UpdateButtonText(false);
            UpdateButtonText(assigning);
            listeningEditor = assigning ? this : null;
        }

        private void UpdateButtonText(bool assigning) {
            assignButton.Content = assigning ? "Press a key" : "Assign";
        }

        private static void InputEvents_KeyDown(object sender, SharpDX.RawInput.KeyboardInputEventArgs e) {
            if (listeningEditor != null)
                listeningEditor.Dispatcher.Invoke(() => {
                    listeningEditor.SelectedKey = e.Key;
                    listeningEditor.UpdateButtonText(false);
                    listeningEditor = null;
                });
        }

        // Dependency Property
        public static readonly DependencyProperty SelectedKeyProperty = DependencyProperty.Register("SelectedKey", typeof(Keys), typeof(Control_SingleKeyEditor), new FrameworkPropertyMetadata(default(Keys), FrameworkPropertyMetadataOptions.AffectsRender));

        public Keys SelectedKey {
            get => (Keys)GetValue(SelectedKeyProperty);
            set => SetValue(SelectedKeyProperty, value);
        }
    }
}
