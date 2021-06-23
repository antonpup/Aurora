using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private bool isHorizontal = true;

       // private Control_Keycap SelectedKey;

        public ObservableCollection<Control_Keycap> KeycapCollection
        {
            get => (ObservableCollection<Control_Keycap>)GetValue(KeycapCollectionProperty);
            set => SetValue(KeycapCollectionProperty, value);
        }

        public static readonly DependencyProperty KeycapCollectionProperty =
            DependencyProperty.Register("KeycapCollection", typeof(ObservableCollection<Control_Keycap>), typeof(Control_LayerList), new PropertyMetadata(null));

        public Control_Keycap SelectedKeycap
        {
            get => (Control_Keycap)GetValue(SelectedKeycapProperty);
            set => SetValue(SelectedKeycapProperty, value);
        }

        public static readonly DependencyProperty SelectedKeycapProperty =
            DependencyProperty.Register("SelectedKeycap", typeof(Control_Keycap), typeof(Control_LayerList), new PropertyMetadata(null, SelectedKeycapPropertyChanged));

        private static void SelectedKeycapPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

            // if (e.NewValue is Control_Keycap layer)
            //     layer.SetProfile(((Control_LayerList)sender).FocusedApplication);
            if (sender is ListBox listView)
            {
                if (listView.SelectedItem is Control_Keycap key)
                {

                }
            }
        }
        public Window_DeviceConfig(Control_DeviceLayout config)
        {
            InitializeComponent();
            originalDeviceLayout = config;
            deviceLayout.DeviceConfig = config.DeviceConfig;
            
            KeycapCollection = deviceLayout.KeycapLayouts;//.CollectionChanged += HandleChange;
            KeycapCollection.CollectionChanged += HandleChange;
            deviceLayout.ConfigChanged();
           /* foreach (var keycap in KeycapCollection)
            {
                keycap.MouseDown += KeyMouseDown;
                keycap.MouseMove += KeyMouseMove;
                keycap.MouseUp += KeyMouseUp;
            }*/

            LoadDeviceType(Config.Type);
            var deviceIdList = Global.dev_manager.IndividualDevices.Where(d => d.id.ViewPort == null).Select(d => d.id).ToList();
            deviceIdList.Insert(0, new Devices.UniqueDeviceId());
            int selectedIndex = int.MaxValue;
            for (int i = 0; i < deviceIdList.Count; i++)
            {
                if (deviceIdList[i] == Config.Id)
                    selectedIndex = i;
            }
            if (selectedIndex == int.MaxValue)
            {
                deviceIdList.Insert(0, Config.Id);
                selectedIndex = 0;
            }
                
            this.device_view.ItemsSource = deviceIdList;
            this.device_view.SelectedIndex = selectedIndex;

            this.device_type.ItemsSource = new string[3]{"Keyboard", "Mouse", "Other Devices"};
            this.device_type.IsEnabled = Config.TypeChangeEnabled;

            this.device_layout.SelectedItem = Config.SelectedLayout;

            layoutName.Text = Config.SelectedLayout;
            if (Config is KeyboardConfig keyboardConfig) 
                this.keyboard_layout.SelectedValue = keyboardConfig.SelectedKeyboardLayout;
            this.devices_disable_lighting.IsChecked = !Config.LightingEnabled;
            this.device_invisible_background.IsChecked = Config.InvisibleBackgroundEnabled;
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

                }
            }
        }
        private void LoadDeviceType(int type)
        {
            this.device_layout.SelectedValue = "None";
            switch (type)
            {
                case 0:
                    this.device_type.SelectedItem = "Keyboard";
                    this.device_layout.ItemsSource = Global.devicesLayout.GetLayoutsForType(Devices.AuroraDeviceType.Keyboard);
                    this.keyboard_layout.Visibility = Visibility.Visible;
                    this.keyboard_layout_tb.Visibility = Visibility.Visible;
                    deviceLayout.DeviceConfig = new KeyboardConfig(Config);
                    this.keyboard_layout.SelectedItem = (Config as KeyboardConfig).SelectedKeyboardLayout;
                    break;
                case 1:
                    this.device_type.SelectedItem = "Mouse";
                    this.device_layout.ItemsSource = Global.devicesLayout.GetLayoutsForType(Devices.AuroraDeviceType.Mouse);
                    this.keyboard_layout.Visibility = Visibility.Collapsed;
                    this.keyboard_layout_tb.Visibility = Visibility.Collapsed;
                    Config.Type = 1;
                    deviceLayout.DeviceConfig = new MouseConfig(Config);
                    break;
                default:
                    Config.Type = 2;
                    this.device_type.SelectedItem = "Other Devices";
                    this.device_layout.ItemsSource = Global.devicesLayout.GetLayoutsForType(Devices.AuroraDeviceType.Unkown);
                    this.keyboard_layout.Visibility = Visibility.Collapsed;
                    this.keyboard_layout_tb.Visibility = Visibility.Collapsed;
                    deviceLayout.DeviceConfig = new DeviceConfig(Config);
                    break;
            }
            this.device_layout.SelectedValue = "None";
        }
        
        private void device_view_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                var selectedDeviceId = (Devices.UniqueDeviceId)this.device_view.SelectedItem;
                Global.dev_manager.RegisterViewPort(ref selectedDeviceId, (int)Config.Id.ViewPort);
                Config.Id = selectedDeviceId;
                deviceLayout.ConfigChanged();
            }
        }
        private void device_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Config.Type = this.device_type.SelectedIndex;
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
        private void device_invisible_background_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox checkBox)
            {
                Config.InvisibleBackgroundEnabled = (checkBox.IsChecked.HasValue) ? checkBox.IsChecked.Value : true;
            }
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
                Global.devicesLayout.RemoveDeviceLayout(originalDeviceLayout.DeviceConfig);
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
                var escIndex= deviceLayout.KeyboardMap.Where(lp => lp.Key == Devices.DeviceKeys.ESC);
                if (Config.Type == 0 && escIndex.Any())
                {
                    var escConfig = deviceLayout.KeycapLayouts[escIndex.First().Value].Config;
                    offset.X = -escConfig.X;
                    offset.Y = -escConfig.Y;
                }
                new DeviceLayout(Config).SaveLayout(deviceLayout.KeycapLayouts.ToList(), offset);
                LoadDeviceType(Config.Type);
                this.device_layout.SelectedItem = Config.SelectedLayout;
            }

        }
        private void KeyMouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectedKeycap = sender as Control_Keycap;
            keycap_list.SelectedItem = SelectedKeycap;

            // when the mouse is down, get the position within the current control. (so the control top/left doesn't move to the mouse position)
            _positionInBlock = Mouse.GetPosition(sender as UIElement);

            // capture the mouse (so the mouse move events are still triggered (even when the mouse is not above the control)
            SelectedKeycap?.CaptureMouse();

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
            if (e.Key == Key.Delete && SelectedKeycap != null)
            {
                KeycapCollection.Remove(SelectedKeycap);
            }
        }

        private void keycap_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is ListBox listView)
            {
                if (listView.SelectedItem is Control_Keycap key)
                {
                    SelectedKeycap = key;
                }
            }
        }
        private int getValidTag(int tag)
        {
            if(KeycapCollection.Where(l => l.Config.Tag == tag).Any())
            {
                return getValidTag(tag + 1);
            }
            return tag;
        }
        private void addKey_Click(object sender, RoutedEventArgs e)
        {
            var keyConf = new DeviceKeyConfiguration();
            keyConf.Height = 30;
            keyConf.Width = 30;
            keyConf.Tag = 0;
            keyConf.VisualName = "";
            if (SelectedKeycap != null)
            {
                if (isHorizontal)
                {
                    keyConf.X = SelectedKeycap.Config.X + SelectedKeycap.Config.Width + 7;
                    keyConf.Y = SelectedKeycap.Config.Y;
                }
                else
                {
                    keyConf.X = SelectedKeycap.Config.X;
                    keyConf.Y = SelectedKeycap.Config.Y + SelectedKeycap.Config.Height + 7;
                }
                keyConf.Height = (int)SelectedKeycap.Height;
                keyConf.Width = (int)SelectedKeycap.Width; ;
                keyConf.Tag = SelectedKeycap.Config.Tag + 1;
                keyConf.VisualName = SelectedKeycap.Config.VisualName;
            }
            keyConf.Tag = getValidTag(keyConf.Tag);
            //if 
            var keycap = new Control_Keycap(keyConf);
            keycap.MouseDown += KeyMouseDown;
            keycap.MouseMove += KeyMouseMove;
            keycap.MouseUp += KeyMouseUp;
            keycap.UpdateLayout();
            SelectedKeycap = keycap;
            deviceLayout.KeycapLayouts.Add(keycap);
        }
        private void addGhostKey_Click(object sender, RoutedEventArgs e)
        {
            var keyConf = new DeviceKeyConfiguration();
            keyConf.Height = 30;
            keyConf.Width = 30;
            keyConf.VisualName = "";
            if (SelectedKeycap != null)
            {
                if (isHorizontal)
                {
                    keyConf.X = SelectedKeycap.Config.X + SelectedKeycap.Config.Width + 7;
                    keyConf.Y = SelectedKeycap.Config.Y;
                }
                else
                {
                    keyConf.X = SelectedKeycap.Config.X;
                    keyConf.Y = SelectedKeycap.Config.Y + SelectedKeycap.Config.Height + 7;
                }
                keyConf.VisualName = SelectedKeycap.Config.VisualName;
            }
            keyConf.Tag = -1;
            var keycap = new Control_Keycap(keyConf);
            keycap.MouseDown += KeyMouseDown;
            keycap.MouseMove += KeyMouseMove;
            keycap.MouseUp += KeyMouseUp;
            keycap.UpdateLayout();
            SelectedKeycap = keycap;
            deviceLayout.KeycapLayouts.Add(keycap);
        }
        private void removeKey_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedKeycap != null)
            {
                deviceLayout.KeycapLayouts.Remove(SelectedKeycap);
                SelectedKeycap = null;
            }
        }
        private void changeNewKey_Click(object sender, RoutedEventArgs e)
        {
            isHorizontal = !isHorizontal;
            if (isHorizontal)
            {
                (sender as Button).Content = "Horizontal";
            }
            else
            {
                (sender as Button).Content = "Vertical";
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
                        deviceLayout.KeycapLayouts[i].SetColor(Color.FromRgb(200, 200, 200), deviceLayout.KeycapLayouts[i] == SelectedKeycap);
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
