using Aurora.Devices;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    /// Interaction logic for Control_ColorizedKeycap.xaml
    /// </summary>
    public partial class Control_ImageKeycap : KeycapViewer
    {
        private Color? current_color = null;
        private string layoutsPath = System.IO.Path.Combine(Global.ExecutingDirectory, "DeviceLayouts");

        public Control_ImageKeycap()
        {
            InitializeComponent();
        }

        public Control_ImageKeycap(DeviceKeyConfiguration key) : base(key)
        {
            InitializeComponent();

            string image_path = System.IO.Path.Combine(layoutsPath, "Images", Config.Image);

            keyBorder.IsEnabled = key.Enabled.Value;

            if (!key.Enabled.Value)
            {
                ToolTipService.SetShowOnDisabled(keyBorder, true);
                keyBorder.ToolTip = new ToolTip { Content = "Changes to this key are not supported" };
            }

            if (System.IO.File.Exists(image_path))
            {
                var memStream = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(image_path));
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = memStream;
                image.EndInit();

                if (key.Tag == (int)DeviceKeys.NONE)
                    keyBorder.Background = new ImageBrush(image);
                else
                {
                    keyBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
                    keyBorder.OpacityMask = new ImageBrush(image);
                }

            }
        }

        override public void SetColor(Color key_color, bool isSelected)
        {
            //Static image
            if (GetKey() == DeviceKeys.NONE)
                return;

            if (!key_color.Equals(current_color))
            {
                keyBorder.Background = new SolidColorBrush(key_color);
                current_color = key_color;
            }

            if (isSelected)
                keyBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)255, (byte)0, (byte)(Math.Min(Math.Pow(Math.Cos((double)(Utils.Time.GetMilliSeconds() / 1000.0) * Math.PI) + 0.05, 2.0), 1.0) * 255), (byte)0));
        }
    }
}
