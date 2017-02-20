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

        private static Control_Keybind _ActiveKeybind = null; //Makes sure that only one keybind can be set at a time

        public Control_Keybind()
        {
            InitializeComponent();

            Global.input_subscriptions.KeyDown += Input_subscriptions_KeyDown;
        }

        private void Input_subscriptions_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            Dispatcher.Invoke(
                () =>
                {

                    if (this.Equals(_ActiveKeybind))
                    {
                        System.Windows.Forms.Keys[] _PressedKeys = Global.input_subscriptions.PressedKeys;

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

        public void Start()
        {
            if (_ActiveKeybind != null)
                _ActiveKeybind.Stop();

            _ActiveKeybind = this;

            buttonToggleAssign.Content = "Stop";
        }

        public void Stop()
        {
            buttonToggleAssign.Content = "Assign";
            _ActiveKeybind = null;
            KeybindUpdated?.Invoke(this, ContextKeybind);
        }

        private void buttonToggleAssign_Click(object sender, RoutedEventArgs e)
        {
            if (this.Equals(_ActiveKeybind))
                Stop();
            else
                Start();
        }
    }
}
