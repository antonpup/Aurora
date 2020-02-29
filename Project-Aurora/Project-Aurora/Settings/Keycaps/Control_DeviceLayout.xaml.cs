using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Aurora.Settings.Keycaps
{
    /// <summary>
    /// Interaction logic for Control_DeviceLayout.xaml
    /// </summary>
    public partial class Control_DeviceLayout : UserControl
    {
        public delegate void LayoutUpdatedEventHandler(object sender);

        public event LayoutUpdatedEventHandler DeviceLayoutUpdated;
        public Dictionary<DeviceKey, Control_Keycap> KeyboardMap = new Dictionary<DeviceKey, Control_Keycap>(new DeviceKey.EqualityComparer());
        private Point BaseOffset = new Point(0,0);

        public static readonly DependencyProperty DeviceConfigProperty = DependencyProperty.Register("DeviceConfig", typeof(DeviceConfig), typeof(DeviceConfig));

        public DeviceConfig DeviceConfig
        {
            get { return (DeviceConfig)GetValue(DeviceConfigProperty); }
            set {
                SetValue(DeviceConfigProperty, value);
                DeviceConfig.ConfigurationChanged += ConfigChanged;
                ConfigChanged();
            }
        }


        private string layoutsPath = System.IO.Path.Combine(Global.ExecutingDirectory, "DeviceLayouts");

        public Control_DeviceLayout()
        {
            InitializeComponent();
            DeviceConfig = new DeviceConfig();
        }
        public Control_DeviceLayout(DeviceConfig config)
        {
            InitializeComponent();
            DeviceConfig = config;
            //ConfigChanged();


        }

        private void ConfigChanged()
        {
            Keys = LoadKeys();
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

                CreateUserControl(device_grid);

                this.Width = device_grid.Width;
                this.Height = device_grid.Height;

                RenderTransform = new TranslateTransform(DeviceConfig.Offset.X, DeviceConfig.Offset.Y);

                device_grid.UpdateLayout();
                device_viewbox.UpdateLayout();
                this.UpdateLayout();
                DeviceLayoutUpdated?.Invoke(this);

            } 
        }
        private List<DeviceKeyConfiguration> LoadKeys()
        {
            DeviceLayout layout = new DeviceLayout(DeviceConfig);
            return layout.LoadLayout();
        }

       
        public void CreateUserControl(Canvas deviceControl, bool abstractKeycaps = false)
        {
            KeyboardMap.Clear();
            deviceControl.Children.Clear();
            if (Keys.Count > 0)
            {
                string images_path = System.IO.Path.Combine(layoutsPath, "Images");

                int layout_height = 0;
                int layout_width = 0;
                foreach (DeviceKeyConfiguration key in Keys)
                {
                    string image_path = "";

                    if (!String.IsNullOrWhiteSpace(key.Image))
                        image_path = System.IO.Path.Combine(images_path, key.Image);

                    Control_Keycap keycap = new Control_Keycap(key, image_path);

                    if (key.Tag == (int)Devices.DeviceKeys.ESC)
                    {
                        //var escKey = KeyboardMap[Devices.DeviceKeys.ESC];
                        BaseOffset.X = key.Region.X;
                        BaseOffset.Y = key.Region.Y;

                    }
                    keycap.RenderTransform = new TranslateTransform(key.Region.X, key.Region.Y);
                    //keycap.Margin = new Thickness(key.Region.X, key.Region.Y, 0, 0);
                    deviceControl.Children.Add(keycap);

                    if (!KeyboardMap.ContainsKey((Devices.DeviceKeys)key.Tag) && keycap is IKeycap && !abstractKeycaps)
                        KeyboardMap.Add(key.Key, keycap);

                    if (key.Region.Width + key.Region.X > layout_width)
                        layout_width = key.Region.Width + key.Region.X;

                    if (key.Region.Height + key.Region.Y > layout_height)
                        layout_height = key.Region.Height + key.Region.Y;

                }
                foreach (Control_Keycap k in deviceControl.Children)
                {
                    k.Offset = BaseOffset;
                }
                //Update size
                deviceControl.Width = layout_width;
                deviceControl.Height = layout_height;
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

                double width = key.Region.Width;
                double height = key.Region.Height;
                double x_offset = DeviceConfig.Offset.X + key.Region.X;
                double y_offset = DeviceConfig.Offset.Y + key.Region.Y;

                bitmapMap[key.Key] = new BitmapRectangle(PixelToByte(x_offset), PixelToByte(y_offset), PixelToByte(width), PixelToByte(height));

            }
            return bitmapMap;

        }
        public void SaveLayoutPosition(Point pos)
        {
            DeviceConfig.Offset = pos;
            DeviceConfig.Save();
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
    }
}
