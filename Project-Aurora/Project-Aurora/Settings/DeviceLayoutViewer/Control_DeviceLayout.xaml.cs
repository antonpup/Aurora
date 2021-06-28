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
        /*public class Wrap<T> where T : struct
        {
            public T Value;

            public static implicit operator Wrap<T>(T v) { return new Wrap<T> { Value = v }; }
            public static implicit operator T(Wrap<T> w) { return w.Value; }

            public override string ToString() { return Value.ToString(); }
            public override int GetHashCode() { return Value.GetHashCode(); }
            // TODO other delegating operators/overloads
        }*/
        public bool IsUpdateEnabled { get; set; }

        public delegate void LayoutUpdatedEventHandler(object sender);

        public Dictionary<DeviceKey, int> KeyboardMap = new Dictionary<DeviceKey, int>();

        public static readonly DependencyProperty DeviceHeightProperty = DependencyProperty.Register("DeviceHeight", typeof(int), typeof(Control_DeviceLayout), new PropertyMetadata(0));
        public int DeviceHeight
        {
            get { return (int)GetValue(DeviceHeightProperty); }
            set { SetValue(DeviceHeightProperty, value); }
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
        }
        private void KeycapPositionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "X" || e.PropertyName == "Y" || e.PropertyName == "Width" || e.PropertyName == "Height")
            {
                ResizeLayout();
            }
        }
        public static readonly DependencyProperty BackgroundVisibilityProperty = DependencyProperty.Register("BackgroundVisibility", typeof(bool), typeof(Control_DeviceLayout), new PropertyMetadata(false));
        public bool BackgroundVisibility
        {
            get { return (bool)GetValue(BackgroundVisibilityProperty); }
            set { SetValue(BackgroundVisibilityProperty, value); }
        }

        public void ResizeLayout()
        {
            int i = 0;
            KeyboardMap = KeycapLayouts.ToDictionary(k => k.GetKey(), k => i++, new DeviceKey.EqualityComparer());
            int layout_height = 10;
            int layout_width = 10;
            int offset_x = int.MaxValue;
            int offset_y = int.MaxValue;
            foreach (Control_Keycap key in KeycapLayouts)
            {
                var keyConfig = key.Config;
                if (keyConfig.Width + keyConfig.X > layout_width)
                    layout_width = (int)keyConfig.Width + keyConfig.X;
                if (keyConfig.X < offset_x)
                    offset_x = keyConfig.X;
                if (keyConfig.Height + keyConfig.Y > layout_height)
                    layout_height = (int)keyConfig.Height + keyConfig.Y;
                if (keyConfig.Y < offset_y)
                    offset_y = keyConfig.Y;
            }
            if(offset_x != 0 || offset_y != 0)
            {
                foreach (Control_Keycap key in KeycapLayouts)
                {
                    key.Config.PropertyChanged -= KeycapPositionChanged;
                    key.Config.X -= offset_x;
                    key.Config.Y -= offset_y;
                    key.Config.PropertyChanged += KeycapPositionChanged;
                }
            }
            //Update size
            DeviceWidth = layout_width - offset_x;
            DeviceHeight = layout_height - offset_y;
            if (KeycapLayouts.Count == 0)
            {
                this.Width = 450;
                this.Height = 200;
            }
            else
            {
                this.Width = DeviceWidth;
                this.Height = DeviceHeight;
            }
            UpdateLayout();
        }

        public static readonly DependencyProperty DeviceConfigProperty = DependencyProperty.Register("DeviceConfig", typeof(DeviceConfig), typeof(Control_DeviceLayout));

        public DeviceConfig DeviceConfig
        {
            get { return (DeviceConfig)GetValue(DeviceConfigProperty); }
            set
            {
                SetValue(DeviceConfigProperty, value);
                //DeviceConfig.ConfigurationChanged += ConfigChanged;
                ConfigChanged();

            }
        }
        public Control_DeviceLayout()
        {
            InitializeComponent();
            DataContext = this;
            KeycapLayouts.CollectionChanged += HandleChange;

        }
        public Control_DeviceLayout(DeviceConfig config)
        {
            InitializeComponent();
            //device_grid = FindName("device_grid") as Canvas;
            if (config.Offset.X < 0 || config.Offset.Y < 0)
                config.Offset = new Point(0, 0);
            DeviceConfig = config;

            DataContext = this;
            KeycapLayouts.CollectionChanged += HandleChange;

            //device_layout = contentPresenter.ContentTemplate;


        }

        public void ConfigChanged()
        {
            BackgroundVisibility = DeviceConfig.InvisibleBackgroundEnabled;
            DeviceLayout layout = new DeviceLayout(DeviceConfig);
            Keys = layout.LoadLayout();
            foreach (var key in Keys)
            {
                key.Key.DeviceId = DeviceConfig.Id.ViewPort;
            }
            ResizeLayout();
        }

        private List<DeviceKeyConfiguration> _Keys = new List<DeviceKeyConfiguration>();
        public List<DeviceKeyConfiguration> Keys
        {
            get { return _Keys; }
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
                        KeycapLayouts[KeyboardMap[kvp.Key]].SetColor(Utils.ColorUtils.DrawingColorToMediaColor(System.Drawing.Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(key_color, key_color.A / 255.0D))));
                    }
                }
            }
        }
    }
}
