using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using SharpDX.RawInput;

namespace Aurora.Controls
{
    /// <summary>
    /// Interaction logic for Control_Keybind.xaml
    /// </summary>
    public partial class Control_Keybind : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty KeybindProperty = DependencyProperty.Register("Keybind", typeof(Keybind), typeof(Control_Keybind));

        public Keybind ContextKeybind
        {
            get
            {
                return (Keybind)GetValue(KeybindProperty);
            }
            set
            {
                SetValue(KeybindProperty, value);

                if (value != null)
                {
                    textBoxKeybind.Text = value.ToString();
                    KeybindUpdated?.Invoke(this, value);
                }
            }
        }

        public delegate void NewKeybindArgs(object sender, Keybind newKeybind);

        public event NewKeybindArgs KeybindUpdated;

        public static Control_Keybind _ActiveKeybind { get; private set; } = null; //Makes sure that only one keybind can be set at a time

        public Control_Keybind()
        {
            InitializeComponent();

            Global.InputEvents.KeyDown += InputEventsKeyDown;
        }

        private void InputEventsKeyDown(object sender, KeyboardInputEventArgs e)
        {
            Dispatcher.Invoke(
                () =>
                {

                    if (this.Equals(_ActiveKeybind))
                    {
                        System.Windows.Forms.Keys[] _PressedKeys = Global.InputEvents.PressedKeys;

                        if (ContextKeybind != null)
                        {
                            ContextKeybind.SetKeys(_PressedKeys);
                            textBoxKeybind.Text = ContextKeybind.ToString();
                            KeybindUpdated?.Invoke(this, ContextKeybind);
                        }
                        else
                            textBoxKeybind.Text = "ERROR (No KeybindProperty set)";
                    }
                });
        }

        private bool isRecording = false;
        public void Start()
        {
            if (_ActiveKeybind != null)
                _ActiveKeybind.Stop();

            isRecording = true;
            _ActiveKeybind = this;

            buttonToggleAssign.Content = "Stop";
        }

        public void Stop()
        {
            buttonToggleAssign.Content = "Assign";
            isRecording = false;
            _ActiveKeybind = null;
            KeybindUpdated?.Invoke(this, ContextKeybind);
        }

        private void buttonToggleAssign_Click(object sender, RoutedEventArgs e)
        {
            if (isRecording)
                Stop();
            else
                Start();
        }

        private void Grid_LostFocus(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key.Equals(Key.Down) || e.Key.Equals(Key.Up) || e.Key.Equals(Key.Left) || e.Key.Equals(Key.Right)) && isRecording)
                e.Handled = true;
        }
    }
}
