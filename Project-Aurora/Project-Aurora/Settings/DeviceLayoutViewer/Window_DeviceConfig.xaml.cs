using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Aurora.Settings.DeviceLayoutViewer
{
    /// <summary>
    /// Interaction logic for Window_DeviceConfig.xaml
    /// </summary>
    public partial class Window_DeviceConfig : Window
    {
        private Control_DeviceLayout originalDeviceLayout;
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        public DeviceConfig Config => deviceLayout.DeviceConfig;

        public bool DeleteDevice = false;
        private System.Windows.Point _positionInBlock;

        private Control_Keycap SelectedKey;
        public Window_DeviceConfig(Control_DeviceLayout config)
        {
            InitializeComponent();
            originalDeviceLayout = config;
            deviceLayout.DeviceConfig = config.DeviceConfig;
            deviceLayout.KeycapLayouts.CollectionChanged += HandleChange;
            deviceLayout.ConfigChanged();
            

            LoadDeviceType(Config.Type);

            this.device_type.ItemsSource = new string[2]{"Keyboard", "Other Devices"};
            this.device_layout.SelectedItem = Config.SelectedLayout;

            layoutName.Text = Config.SelectedLayout;
            if (Config is KeyboardConfig keyboardConfig) 
                this.keyboard_layout.SelectedItem = keyboardConfig.SelectedKeyboardLayout;
            this.devices_disable_lighting.IsChecked = !Config.LightingEnabled;
            DataContext = this;

            this.KeyDown += OnKeyDownHandler;
            Task.Run(() => UpdateKeysThread(tokenSource.Token));
        }
        private void HandleChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Replace || e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Control_Keycap key in e.NewItems)
                {
                    //keycap_list.Items.Clear();
                    key.MouseDown += KeyMouseDown;
                    key.MouseMove += KeyMouseMove;
                    key.MouseUp += KeyMouseUp;
                    key.UpdateLayout();
                    keycap_list.Items.Add(key);
                    
                    //keycap_list.InvalidateProperty(ListView.ItemsSourceProperty);
                    keycap_list.InvalidateVisual();
                    keycap_list.UpdateLayout();
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Control_Keycap layout in e.OldItems)
                {
                    keycap_list.Items.Remove(layout);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                keycap_list.Items.Clear();
            }

        }
        private List<string> GetBrandsName(string dicName)
        {
            string layoutsPath = Path.Combine(Global.ExecutingDirectory, "DeviceLayouts", dicName);
            List<string> FilesName = new List<string>() { "None" };
            if (Directory.Exists(layoutsPath))
            {
                foreach (var name in Directory.GetFiles(layoutsPath))
                {
                    FilesName.Add(name.Split('\\').Last().Split('.').First());
                }
            }
            else
            {
                Directory.CreateDirectory(layoutsPath);
            }
            return FilesName;
        }
        private void LoadDeviceType(int type)
        {
            switch (type)
            {
                case 0:
                    this.device_type.SelectedItem = "Keyboard";
                    this.device_layout.ItemsSource = GetBrandsName("Keyboard");
                    this.keyboard_layout.Visibility = Visibility.Visible;
                    this.keyboard_layout_tb.Visibility = Visibility.Visible;
                    deviceLayout.DeviceConfig = new KeyboardConfig(Config);
                    this.keyboard_layout.SelectedItem = (Config as KeyboardConfig).SelectedKeyboardLayout;
                    break;
                default:
                    this.device_type.SelectedItem = "Other Devices";
                    this.device_layout.ItemsSource = GetBrandsName("Mouse");
                    this.keyboard_layout.Visibility = Visibility.Collapsed;
                    this.keyboard_layout_tb.Visibility = Visibility.Collapsed;
                    deviceLayout.DeviceConfig = new DeviceConfig(Config);
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
                deviceLayout.ConfigChanged();
                layoutName.Text = this.device_layout.SelectedItem.ToString();

            }
        }
        private void keyboard_layout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && Config is KeyboardConfig keyboardConfig)
            {
                keyboardConfig.SelectedKeyboardLayout = (KeyboardPhysicalLayout)keyboard_layout.SelectedValue;
                deviceLayout.ConfigChanged();
            }
        }
        private void device_disable_lighting_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox checkBox)
            {
                Config.LightingEnabled = (checkBox.IsChecked.HasValue) ? !checkBox.IsChecked.Value : true;
            }
            //deviceLayout = new Control_DeviceLayout(Config);
            
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Config.Offset = originalDeviceLayout.DeviceConfig.Offset;
            originalDeviceLayout.DeviceConfig = Config;
            Global.devicesLayout.SaveConfiguration(Config);
            tokenSource.Cancel();
            Close();
        }
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            tokenSource.Cancel();
            Close();
        }
        private void deviceDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure that remove the device layout?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Global.devicesLayout.DeviceLayouts.Remove(originalDeviceLayout);
                tokenSource.Cancel();
                Close();
            }

        }
        private void saveLayout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure that save the device layout?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Config.SelectedLayout = layoutName.Text;
                var offset = new Point();
                if(Config.Type == 0 && deviceLayout.KeyboardMap.ContainsKey(Devices.DeviceKeys.ESC))
                {
                    var escConfig = deviceLayout.KeyboardMap[Devices.DeviceKeys.ESC].Config;
                    offset.X = -escConfig.X;
                    offset.Y = -escConfig.Y;
                }
                new DeviceLayout(Config).SaveLayout(deviceLayout.KeycapLayouts.ToList(), offset);
            }

        }
        private void KeyMouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectedKey = sender as Control_Keycap;

            // when the mouse is down, get the position within the current control. (so the control top/left doesn't move to the mouse position)
            _positionInBlock = Mouse.GetPosition(sender as UIElement);

            // capture the mouse (so the mouse move events are still triggered (even when the mouse is not above the control)
            (sender as Control_Keycap)?.CaptureMouse();

        }

        private void KeyMouseMove(object sender, MouseEventArgs e)
        {
            // if the mouse is captured. you are moving it. (there is your 'real' boolean)
            if ((sender as Control_Keycap).IsMouseCaptured)
            {
                // get the parent container
                var container = VisualTreeHelper.GetParent(sender as UIElement) as UIElement;

                // get the position within the container
                var mousePosition = e.GetPosition(container);

                // move the usercontrol.
                (sender as Control_Keycap).Keycap.Config.X = (int)(mousePosition.X - _positionInBlock.X);
                (sender as Control_Keycap).Keycap.Config.Y = (int)(mousePosition.Y - _positionInBlock.Y);
                //keycap_x.Text = ((int)(mousePosition.X - _positionInBlock.X)).ToString();
                //keycap_y.Text = ((int)(mousePosition.Y - _positionInBlock.Y)).ToString();
            }
        }

        private void KeyMouseUp(object sender, MouseButtonEventArgs e)
        {
            // release this control.
            (sender as Control_Keycap)?.ReleaseMouseCapture();

        }
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            /*if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
            {
                MessageBox.Show("CTRL + C Pressed!");
            }
            else if (e.Key == Key.Z && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                MessageBox.Show("CTRL + Z Pressed!");
            }
            if (e.Key == Key.Up)
            {
                MessageBox.Show("Up Pressed!");
            }*/
        }

        private void keycap_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is ListBox listView)
            {
                if (listView.SelectedItem is Control_Keycap key)
                {
                    SelectedKey = key;
                }
            }
        }
        private void addKey_Click(object sender, RoutedEventArgs e)
        {
            var keyConf = new DeviceKeyConfiguration();
            keyConf.Height = 30;
            keyConf.Width = 30;
            if(SelectedKey != null)
            {
                keyConf.X = SelectedKey.Config.X + SelectedKey.Config.Width + 7;
                keyConf.Y = SelectedKey.Config.Y;
                keyConf.Tag = SelectedKey.Config.Tag + 1;
            }
            var keycap = new Control_Keycap(keyConf);
            keycap.MouseDown += KeyMouseDown;
            keycap.MouseMove += KeyMouseMove;
            keycap.MouseUp += KeyMouseUp;
            keycap.UpdateLayout();
            SelectedKey = keycap;
            deviceLayout.KeycapLayouts.Add(keycap);
        }
        private void removeKey_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedKey != null)
            {
                deviceLayout.KeycapLayouts.Remove(SelectedKey);
                SelectedKey = null;
            }
        }
        private void UpdateKeysThread(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Dispatcher.Invoke(() =>
                {
                    for (int i = 0; i < deviceLayout.KeycapLayouts.Count; i++)
                    {
                        deviceLayout.KeycapLayouts[i].SetColor(Color.FromRgb(200, 200, 200), deviceLayout.KeycapLayouts[i] == SelectedKey);
                    }
                });
                Thread.Sleep(100);
            }
        }

        private void enable_layout_preview_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
