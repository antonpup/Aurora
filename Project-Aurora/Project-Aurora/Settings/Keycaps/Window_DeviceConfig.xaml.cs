using System;
using System.Collections.Generic;
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

namespace Aurora.Settings.Keycaps
{
    /// <summary>
    /// Interaction logic for Window_DeviceConfig.xaml
    /// </summary>
    public partial class Window_DeviceConfig : Window
    {
        private DeviceConfig _config;
        public DeviceConfig Config
        {
            get { return _config; }
            set
            {
                _config = value;
                LoadDeviceLayout();
            }
        }
        public bool DeleteDevice = false;
        private System.Windows.Point _positionInBlock;
        public Control_Keycap SelectedKey;
        public Window_DeviceConfig(DeviceConfig config)
        {
            InitializeComponent();
            Config = config;
            LoadDeviceType(config.Type);
            
            this.device_type.ItemsSource = new string[2]{"Keyboard", "Other Devices"};
            this.device_layout.SelectedItem = Config.SelectedLayout;
            if (config.SelectedKeyboardLayout != null) 
                this.keyboard_layout.SelectedItem = Config.SelectedKeyboardLayout;
        }
        private List<string> GetBrandsName(string dicName)
        {
            string layoutsPath = Path.Combine(Global.ExecutingDirectory, "DeviceLayouts", dicName);
            List<string> FilesName = new List<string>() { "None" };
            foreach (var name in Directory.GetFiles(layoutsPath))
            {
                FilesName.Add(name.Split('\\').Last().Split('.').First());
            }
            return FilesName;
        }
        private void LoadDeviceType(int type)
        {
            switch (type)
            {
                case 0:
                    this.device_layout.ItemsSource = GetBrandsName("Keyboard");
                    this.keyboard_layout.ItemsSource = GetBrandsName("Keyboard\\Plain Keyboard");
                    this.keyboard_layout.Visibility = Visibility.Visible;
                    this.keyboard_layout_tb.Visibility = Visibility.Visible;
                    break;
                default:
                    this.device_layout.ItemsSource = GetBrandsName("Mouse");
                    this.keyboard_layout.Visibility = Visibility.Collapsed;
                    this.keyboard_layout_tb.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        private void device_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Config.Type = this.device_type.SelectedIndex;
                this.device_layout.SelectedItem = "None";
                LoadDeviceType(Config.Type);
            }
        }
        private void device_layout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Config.SelectedLayout = this.device_layout.SelectedItem.ToString();
                LoadDeviceLayout();
            }
        }
        private void keyboard_layout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Config.SelectedKeyboardLayout = this.keyboard_layout.SelectedItem.ToString();
                LoadDeviceLayout();
            }
        }
        private void device_disable_lighting_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox)
            {
                Config.LightingEnabled = ((sender as CheckBox).IsChecked.HasValue) ? !(sender as CheckBox).IsChecked.Value : true;
            }
            deviceLayout = new Control_DeviceLayout(Config);
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Config.Save();
            Close();
        }
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void deviceDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure that remove the device layout?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Config.Delete();
                Close();
            }

        }
        private void saveLayout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure that save the device layout?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                new DeviceLayout(Config).SaveLayout(deviceLayout.KeyboardMap.Values.ToList());
            }

        }
        private void KeyMouseDown(object sender, MouseButtonEventArgs e)
        {
            // when the mouse is down, get the position within the current control. (so the control top/left doesn't move to the mouse position)
            _positionInBlock = Mouse.GetPosition(sender as UIElement);

            // capture the mouse (so the mouse move events are still triggered (even when the mouse is not above the control)
            (sender as UIElement).CaptureMouse();

        }

        private void KeyMouseMove(object sender, MouseEventArgs e)
        {
            // if the mouse is captured. you are moving it. (there is your 'real' boolean)
            if ((sender as UIElement).IsMouseCaptured)
            {
                // get the parent container
                var container = VisualTreeHelper.GetParent(sender as UIElement) as UIElement;

                // get the position within the container
                var mousePosition = e.GetPosition(container);

                // move the usercontrol.
                (sender as UIElement).RenderTransform = new TranslateTransform((int)(mousePosition.X - _positionInBlock.X), (int)(mousePosition.Y - _positionInBlock.Y));

            }
        }

        private void KeyMouseUp(object sender, MouseButtonEventArgs e)
        {
            // release this control.
            (sender as UIElement).ReleaseMouseCapture();

        }
        private void LoadDeviceLayout()
        {
            deviceLayout.DeviceConfig = new DeviceConfig(Config);
            foreach (var key in deviceLayout.KeyboardMap.Values)
            {
                key.MouseDown += KeyMouseDown;
                key.MouseMove += KeyMouseMove;
                key.MouseUp += KeyMouseUp;
                key.UpdateLayout();
            }
            deviceLayout.UpdateLayout();
            this.Width = deviceLayout.Width + 340;
        }
    }
}
