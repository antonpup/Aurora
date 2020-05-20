using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Aurora.Settings.DeviceLayoutViewer
{
    /// <summary>
    /// Interaction logic for Control_DeviceLayout.xaml
    /// </summary>
    public partial class Control_DeviceLayout : ItemsControl, INotifyPropertyChanged
    {
        public delegate void LayoutUpdatedEventHandler(object sender);
        public event PropertyChangedEventHandler PropertyChanged;

        //private FrameworkElement device_layout = new FrameworkElement();
        public event LayoutUpdatedEventHandler DeviceLayoutUpdated;
        public Dictionary<DeviceKey, Control_Keycap> KeyboardMap = new Dictionary<DeviceKey, Control_Keycap>(new DeviceKey.EqualityComparer());

        public static readonly DependencyProperty DeviceHeightProperty = DependencyProperty.Register("DeviceHeight", typeof(int), typeof(Control_DeviceLayout), new PropertyMetadata(0));
        public int DeviceHeight
        {
            get { return (int)GetValue(DeviceHeightProperty); }
            set{ SetValue(DeviceHeightProperty, value); }
        }
        public static readonly DependencyProperty DeviceWidthProperty = DependencyProperty.Register("DeviceWidth", typeof(int), typeof(Control_DeviceLayout), new PropertyMetadata(0));
        public int DeviceWidth
        {
            get { return (int)GetValue(DeviceWidthProperty); }
            set { SetValue(DeviceWidthProperty, value); }
        }

        //public static readonly DependencyProperty KeycapLayoutsProperty = DependencyProperty.Register("KeycapLayouts", typeof(ObservableCollection<Control_Keycap>), typeof(Control_DeviceLayout), new PropertyMetadata(new ObservableCollection<Control_Keycap>()));
        //private Canvas device_grid;
        private ObservableCollection<Control_Keycap> _keycapLayouts = new ObservableCollection<Control_Keycap>();
        public ObservableCollection<Control_Keycap> KeycapLayouts// = new ObservableCollection<Control_Keycap>();
        {
            get { return _keycapLayouts; }
            /*set {
                SetValue(KeycapLayoutsProperty, value);
                int layout_height = 0;
                int layout_width = 0;
                foreach (Control_Keycap key in KeycapLayouts)
                {
                    var keyConfig = key.GetConfiguration();
                    if (keyConfig.Width + keyConfig.X > layout_width)
                        layout_width = (int)keyConfig.Width + keyConfig.X;

                    if (keyConfig.Height + keyConfig.Y > layout_height)
                        layout_height = (int)keyConfig.Height + keyConfig.Y;

                }
                //var myContentPresenter = container.Template;

                //device_layout = container.Template.FindName("device_layout", this) as Canvas;
                //Update size
                //Width = layout_width;
                //Height = layout_height;
                //this.Width = device_layout.Width + 5;
                //this.Height = device_layout.Height;

                new TranslateTransform(DeviceConfig.Offset.X, DeviceConfig.Offset.Y);

                //device_layout.UpdateLayout();
                //device_viewbox.UpdateLayout();
                this.UpdateLayout();
                DeviceConfig.ConfigurationChanged += ConfigChanged;
                OnPropertyChanged();
                //(GetValue(KeycapLayoutsProperty) as ObservableCollection<Control_Keycap>).p
                DeviceLayoutUpdated?.Invoke(this);

            }*/
        }
        private void HandleChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            int layout_height = 0;
            int layout_width = 0;
            foreach (Control_Keycap key in KeycapLayouts)
            {
                var keyConfig = key.Config;
                if (keyConfig.Width + keyConfig.X > layout_width)
                    layout_width = (int)keyConfig.Width + keyConfig.X;

                if (keyConfig.Height + keyConfig.Y > layout_height)
                    layout_height = (int)keyConfig.Height + keyConfig.Y;

            }
            //var myContentPresenter = container.Template;

            //device_layout = container.Template.FindName("device_layout", this) as Canvas;
            //Update size
            DeviceWidth = layout_width;
            DeviceHeight = layout_height;
            this.Width = DeviceWidth + 5;
            this.Height = DeviceHeight;

            RenderTransform = new TranslateTransform(DeviceConfig.Offset.X, DeviceConfig.Offset.Y);
            //ItemsSource = KeycapLayouts;
            //var device_viewbox = this.Template.FindVisualChild<Viewbox>(this); //FindName("device_viewbox") as Viewbox;
            /*device_viewbox.Height = Height;
            device_viewbox.Width = Width;*/
            UpdateLayout();
            //device_viewbox.UpdateLayout();
            //this.UpdateLayout();
            DeviceConfig.ConfigurationChanged += ConfigChanged;
            OnPropertyChanged();
            //(GetValue(KeycapLayoutsProperty) as ObservableCollection<Control_Keycap>).p
            DeviceLayoutUpdated?.Invoke(this);
        }

        public static readonly DependencyProperty DeviceConfigProperty = DependencyProperty.Register("DeviceConfig", typeof(DeviceConfig), typeof(Control_DeviceLayout));

        public DeviceConfig DeviceConfig
        {
            get { return (DeviceConfig)GetValue(DeviceConfigProperty); }
            set
            {
                SetValue(DeviceConfigProperty, value);
                DeviceConfig.ConfigurationChanged += ConfigChanged;
                //ConfigChanged();
            }
        }
        public Control_DeviceLayout()
        {
            InitializeComponent();
            DeviceConfig = new DeviceConfig();
            KeycapLayouts.CollectionChanged += HandleChange;
            ItemsSource = KeycapLayouts;
            //DataContext = KeycapLayouts;

        }
        public Control_DeviceLayout(DeviceConfig config)
        {
            InitializeComponent();
            //device_grid = FindName("device_grid") as Canvas;
            DeviceConfig = config;
            KeycapLayouts.CollectionChanged += HandleChange;
            //DataContext = KeycapLayouts;
            ItemsSource = KeycapLayouts;

            //device_layout = contentPresenter.ContentTemplate;
            ConfigChanged();


        }

        public void ConfigChanged()
        {
            DeviceLayout layout = new DeviceLayout(DeviceConfig);
            Keys = layout.LoadLayout();
            foreach(var key in Keys)
            {
                key.Key.DeviceId = DeviceConfig.Id;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DeviceLayoutUpdated?.Invoke(this);
        }

        private List<DeviceKeyConfiguration> _Keys = new List<DeviceKeyConfiguration>();
        public List<DeviceKeyConfiguration> Keys 
        {
            get { return _Keys;} 
            set
            {
                _Keys = value;

                KeycapLayouts.Clear();
                Keys.ForEach(k => KeycapLayouts.Add(new Control_Keycap(k)));

                //this.Width = device_layout.Width + 5;
                //this.Height = device_layout.Height;

                //new TranslateTransform(DeviceConfig.Offset.X, DeviceConfig.Offset.Y);

                //device_grid.UpdateLayout();
                //device_viewbox.UpdateLayout();
                //this.UpdateLayout();
                DeviceLayoutUpdated?.Invoke(this);

            } 
        }
        public void CreateUserControl(Canvas deviceControl, bool abstractKeycaps = false)
        {
            KeyboardMap.Clear();
            deviceControl.Children.Clear();
            if (Keys.Count > 0)
            {

                int layout_height = 0;
                int layout_width = 0;
                foreach (DeviceKeyConfiguration key in Keys)
                {
                    KeycapLayouts.Add(new Control_Keycap(key));
                    //AddDeviceKey(key);

                    if (key.Width + key.X > layout_width)
                        layout_width = key.Width + key.X;

                    if (key.Height + key.Y > layout_height)
                        layout_height = key.Height + key.Y;

                }
                //Update size
                Width = layout_width;
                Height = layout_height;
                //this.Width = device_layout.Width + 5;
                //this.Height = device_layout.Height;

                new TranslateTransform(DeviceConfig.Offset.X, DeviceConfig.Offset.Y);

                //device_grid.UpdateLayout();
                //device_viewbox.UpdateLayout();
                //this.UpdateLayout();
                DeviceConfig.ConfigurationChanged += ConfigChanged;
                OnPropertyChanged();
                DeviceLayoutUpdated?.Invoke(this);
            }
            else
            {
                Label error_message = new Label();

                DockPanel info_panel = new DockPanel();

                TextBlock info_message = new TextBlock()
                {
                    Text = "No Device selected\r\nPlease doubleclick on this box",
                    TextAlignment = TextAlignment.Center,
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0)),
                };

                DockPanel.SetDock(info_message, Dock.Top);
                info_panel.Children.Add(info_message);

                error_message.Content = info_panel;

                error_message.FontSize = 16.0;
                error_message.FontWeight = FontWeights.Bold;
                error_message.HorizontalContentAlignment = HorizontalAlignment.Center;
                error_message.VerticalContentAlignment = VerticalAlignment.Center;

                deviceControl.Children.Add(error_message);
                //Update size
                deviceControl.Width = 450;
                deviceControl.Height = 200;
            }
        }
        public static int PixelToByte(int pixel)
        {
            return PixelToByte((double)pixel);
        }

        public static int PixelToByte(double pixel)
        {
            return (int)Math.Round(pixel / (double)(Global.Configuration.BitmapAccuracy));
        }
        public Dictionary<DeviceKey, BitmapRectangle> GetBitmap()
        {
            Dictionary<DeviceKey, BitmapRectangle> bitmapMap = new Dictionary<DeviceKey, BitmapRectangle>();

            foreach (var key in Keys)
            {

                double width = key.Width;
                double height = key.Height;
                double x_offset = DeviceConfig.Offset.X + key.X;
                double y_offset = DeviceConfig.Offset.Y + key.Y;

                bitmapMap[key.Key] = new BitmapRectangle(PixelToByte(x_offset), PixelToByte(y_offset), PixelToByte(width), PixelToByte(height));

            }
            return bitmapMap;

        }
        public void SetKeyboardColors(Dictionary<DeviceKey, System.Drawing.Color> keylights)
        {
            foreach (var kvp in keylights)
            {
                if (KeyboardMap.ContainsKey(kvp.Key))
                {
                    System.Drawing.Color key_color = kvp.Value;
                    KeyboardMap[kvp.Key].SetColor(Utils.ColorUtils.DrawingColorToMediaColor(System.Drawing.Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(key_color, key_color.A / 255.0D))));
                }
            }
        }
        public Control_Keycap AddDeviceKey(DeviceKeyConfiguration key)
        {
            Control_Keycap keycap = new Control_Keycap(key);

            //keycap.Margin = new Thickness(key.Region.X, key.Region.Y, 0, 0);
            //device_grid.Children.Add(keycap);

            if (!KeyboardMap.ContainsKey((Devices.DeviceKeys)key.Tag))// && !abstractKeycaps)
                KeyboardMap.Add(key.Key, keycap);

            return keycap;
        }
        public void RemoveDeviceKey(DeviceKeyConfiguration key)
        {
            /*foreach (var child in device_grid.Children)
            {
                if (child is Control_Keycap keycap)
                {
                    if (keycap.GetConfiguration() == key)
                    {
                        device_grid.Children.Remove(keycap);
                        return;
                    }
                }
            }*/
        }
        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
