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
    public partial class Control_DeviceLayout : ItemsControl
    {
        public delegate void LayoutUpdatedEventHandler(object sender);

        //private FrameworkElement device_layout = new FrameworkElement();
        public event LayoutUpdatedEventHandler DeviceLayoutUpdated;
        public Dictionary<DeviceKey, Control_Keycap> KeyboardMap => KeycapLayouts.ToDictionary(k => k.GetKey(), k => k, new DeviceKey.EqualityComparer());

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

        public ObservableCollection<Control_Keycap> KeycapLayouts { get; } = new ObservableCollection<Control_Keycap>();
        private void HandleChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (Control_Keycap item in e?.NewItems)
                {
                    item.Config.PropertyChanged += KeycapPositionChanged;
                }
            }
            
            RenderTransform = new TranslateTransform(DeviceConfig.Offset.X, DeviceConfig.Offset.Y);
            ResizeLayout();
        }
        private void KeycapPositionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "X" || e.PropertyName == "Y" || e.PropertyName == "Width" || e.PropertyName == "Height")
            {
                ResizeLayout();
            }
        }
        public void ResizeLayout()
        {
            int layout_height = 10;
            int layout_width = 10;
            foreach (Control_Keycap key in KeycapLayouts)
            {
                var keyConfig = key.Config;
                if (keyConfig.Width + keyConfig.X > layout_width)
                    layout_width = (int)keyConfig.Width + keyConfig.X;

                if (keyConfig.Height + keyConfig.Y > layout_height)
                    layout_height = (int)keyConfig.Height + keyConfig.Y;

            }

            //Update size
            DeviceWidth = layout_width;
            DeviceHeight = layout_height;
            this.Width = DeviceWidth;
            this.Height = DeviceHeight;
            if (KeycapLayouts.Count == 0)
            {
                this.Width = 450;
                this.Height = 200;
            }
            UpdateLayout();
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
                ConfigChanged();
            }
        }
        public Control_DeviceLayout()
        {
            InitializeComponent();
            //DeviceConfig = new DeviceConfig();
            KeycapLayouts.CollectionChanged += HandleChange;
            DataContext = KeycapLayouts;
            //DataContext = KeycapLayouts;

        }
        public Control_DeviceLayout(DeviceConfig config)
        {
            InitializeComponent();
            //device_grid = FindName("device_grid") as Canvas;
            if (config.Offset.X < 0 || config.Offset.Y < 0)
                config.Offset = new Point(0, 0);
            DeviceConfig = config;
            KeycapLayouts.CollectionChanged += HandleChange;
            //DataContext = KeycapLayouts;
            DataContext = KeycapLayouts;

            //device_layout = contentPresenter.ContentTemplate;
            ConfigChanged();


        }

        public void ConfigChanged()
        {
            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    DeviceLayout layout = new DeviceLayout(DeviceConfig);
                    Keys = layout.LoadLayout();
                    foreach (var key in Keys)
                    {
                        key.Key.DeviceId = DeviceConfig.Id.ViewPort;
                    }
                });
            });
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

                //DeviceLayoutUpdated?.Invoke(this);

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
            if (DeviceConfig.LightingEnabled)
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
        }
    }
}
