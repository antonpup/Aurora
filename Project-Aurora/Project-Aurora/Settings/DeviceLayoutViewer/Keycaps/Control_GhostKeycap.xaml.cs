using Aurora.Devices;
using System;
using System.Collections.Generic;
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

namespace Aurora.Settings.DeviceLayoutViewer.Keycaps
{
    /// <summary>
    /// Interaction logic for Control_GhostKeycap.xaml
    /// </summary>
    public partial class Control_GhostKeycap : KeycapViewer
    {
        private Color current_color = Color.FromArgb(0, 0, 0, 0);
        private Devices.DeviceKeys associatedKey = DeviceKeys.NONE;
        private bool isImage = false;

        public Control_GhostKeycap()
        {
            InitializeComponent();
        }

        public Control_GhostKeycap(KeyboardKey key, string image_path)
        {
            InitializeComponent();

            associatedKey = key.tag;

            this.Width = key.width.Value;
            this.Height = key.height.Value;

            //Keycap adjustments
            if (string.IsNullOrWhiteSpace(key.image))
                keyBorder.BorderThickness = new Thickness(1.5);
            else
                keyBorder.BorderThickness = new Thickness(0.0);
            keyBorder.IsEnabled = key.enabled.Value;

            if (!key.enabled.Value)
            {
                ToolTipService.SetShowOnDisabled(keyBorder, true);
                keyBorder.ToolTip = new ToolTip { Content = "Changes to this key are not supported" };
            }

            if (string.IsNullOrWhiteSpace(key.image))
            {
                keyCap.Text = key.visualName;
                keyCap.Tag = key.tag;
                if (key.font_size != null)
                    keyCap.FontSize = key.font_size.Value;
            }
            else
            {
                keyCap.Visibility = System.Windows.Visibility.Hidden;

                if (System.IO.File.Exists(image_path))
                {
                    var memStream = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(image_path));
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = memStream;
                    image.EndInit();

                    if (key.tag == DeviceKeys.NONE)
                        keyBorder.Background = new ImageBrush(image);
                    else
                    {
                        keyBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                        keyBorder.OpacityMask = new ImageBrush(image);
                    }

                    isImage = true;
                }
            }
        }

        public override void SetColor(Color key_color, bool isSelected)
        {
            key_color = Color.FromArgb(255, 255, 255, 255); //No colors allowed!

            if (!current_color.Equals(key_color))
            {
                if (isImage)
                {
                    if (associatedKey != DeviceKeys.NONE)
                        keyBorder.Background = new SolidColorBrush(key_color);
                }
                current_color = key_color;
            }

            if (isSelected)
                keyBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)255, (byte)0, (byte)(Math.Min(Math.Pow(Math.Cos((double)(Utils.Time.GetMilliSeconds() / 1000.0) * Math.PI) + 0.05, 2.0), 1.0) * 255), (byte)0));
            else
            {
                if (keyBorder.IsEnabled)
                {
                    if (isImage)
                        keyBorder.Background = new SolidColorBrush(key_color);
                    else
                        keyBorder.Background = new SolidColorBrush(Utils.ColorUtils.MultiplyColorByScalar(key_color, 0.6));
                }
                else
                {
                    keyBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 100, 100, 100));
                    keyBorder.BorderThickness = new Thickness(0);
                }
            }
        }
    }
}
