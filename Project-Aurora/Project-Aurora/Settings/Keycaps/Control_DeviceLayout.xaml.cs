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
        public bool IsLayoutMoveEnabled = false;
        public Dictionary<DeviceKey, Control_Keycap> KeyboardMap = new Dictionary<DeviceKey, Control_Keycap>(new DeviceKey.EqualityComparer());

        private System.Windows.Point _positionInBlock;

        private DeviceConfig DeviceConfig;

        private string layoutsPath = System.IO.Path.Combine(Global.ExecutingDirectory, "DeviceLayouts");

        public Control_DeviceLayout(DeviceConfig config)
        {
            InitializeComponent();
            DeviceConfig = config;
 
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Keys = LoadKeys();
        }

        private List<DeviceKeyConfiguration> _Keys = new List<DeviceKeyConfiguration>();
        public List<DeviceKeyConfiguration> Keys 
        {
            get { return _Keys;} 
            set
            {
                _Keys = value;
                device_grid.Children.Clear();
                Grid deviceControl = CreateUserControl();
                device_grid.Children.Add(deviceControl);

                this.Width = deviceControl.Width;
                this.Height = deviceControl.Height;

                RenderTransform = new TranslateTransform(DeviceConfig.Offset.X, DeviceConfig.Offset.Y);
                device_grid.UpdateLayout();
                device_viewbox.UpdateLayout();
                DeviceLayoutUpdated?.Invoke(this);

            } 
        }
        private List<DeviceKeyConfiguration> LoadKeys()
        {
            DeviceLayout layout;
            switch (DeviceConfig.Type)
            {
                case 0:
                    layout = new KeyboardDeviceLayout(DeviceConfig);
                    break;
                case 1:
                    layout = new GeneralDeviceLayout(DeviceConfig);
                    break;
                default:
                    layout = new GeneralDeviceLayout(DeviceConfig);
                    break;
            }
            return layout.LoadLayout();
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            Window_DeviceConfig configWindow = new Window_DeviceConfig(DeviceConfig);
            if(configWindow.ShowDialog() == true)
            {
                DeviceConfig = configWindow.Config;
                Keys = LoadKeys();
                DeviceConfig.Save();
            }

        }
        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2 && IsLayoutMoveEnabled)
            {
                // when the mouse is down, get the position within the current control. (so the control top/left doesn't move to the mouse position)
                _positionInBlock = Mouse.GetPosition(this);

                // capture the mouse (so the mouse move events are still triggered (even when the mouse is not above the control)
                this.CaptureMouse();
            }

        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            // if the mouse is captured. you are moving it. (there is your 'real' boolean)
            if (this.IsMouseCaptured)
            {
                // get the parent container
                var container = VisualTreeHelper.GetParent(this) as UIElement;

                // get the position within the container
                var mousePosition = e.GetPosition(container);

                // move the usercontrol.
                this.RenderTransform = new TranslateTransform(mousePosition.X - _positionInBlock.X, mousePosition.Y - _positionInBlock.Y);

            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // release this control.
            this.ReleaseMouseCapture();
            
            DeviceLayoutUpdated?.Invoke(this);

            SaveLayoutPosition(TranslatePoint(new Point(0, 0), VisualTreeHelper.GetParent(this) as UIElement));
        }
        public Grid CreateUserControl(bool abstractKeycaps = false)
        {
            KeyboardMap.Clear();

            Grid deviceControl = new Grid();
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


                    keycap.Margin = new Thickness(key.Region.X, key.Region.Y, 0, 0);
                    deviceControl.Children.Add(keycap);

                    if (!KeyboardMap.ContainsKey((Devices.DeviceKeys)key.Tag) && keycap is IKeycap && !abstractKeycaps)
                        KeyboardMap.Add(key.Key, keycap);

                    if (key.Region.Width + key.Region.X > layout_width)
                        layout_width = key.Region.Width + key.Region.X;

                    if (key.Region.Height + key.Region.Y > layout_height)
                        layout_height = key.Region.Height + key.Region.Y;

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

                DockPanel info_instruction = new DockPanel();

                info_instruction.Children.Add(new TextBlock()
                {
                    Text = "Press (",
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0)),
                    VerticalAlignment = VerticalAlignment.Center
                });

                info_instruction.Children.Add(new System.Windows.Controls.Image()
                {
                    Source = new BitmapImage(new Uri(@"Resources/settings_icon.png", UriKind.Relative)),
                    Stretch = Stretch.Uniform,
                    Height = 40.0,
                    VerticalAlignment = VerticalAlignment.Center
                });

                info_instruction.Children.Add(new TextBlock()
                {
                    Text = ") and go into \"Devices & Wrappers\" tab",
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0)),
                    VerticalAlignment = VerticalAlignment.Center
                });

                DockPanel.SetDock(info_instruction, Dock.Bottom);
                info_panel.Children.Add(info_instruction);

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
            


            return deviceControl;
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
