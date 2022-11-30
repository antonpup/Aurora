using Aurora.Devices;
using Aurora.Utils;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Settings.Keycaps
{
    /// <summary>
    /// Interaction logic for Control_ColorizedKeycap.xaml
    /// </summary>
    public partial class Control_ColorizedKeycap : IKeycap
    {
        private Color current_color;
        private Devices.DeviceKeys associatedKey = DeviceKeys.NONE;
        private bool isImage = false;

        public Control_ColorizedKeycap()
        {
            InitializeComponent();
        }

        public Control_ColorizedKeycap(KeyboardKey key, string image_path)
        {
            InitializeComponent();

            associatedKey = key.Tag;

            this.Width = key.Width;
            this.Height = key.Height;

            //Keycap adjustments
            if (string.IsNullOrWhiteSpace(key.Image))
                keyBorder.BorderThickness = new Thickness(1.5);
            else
                keyBorder.BorderThickness = new Thickness(0.0);
            keyBorder.IsEnabled = key.Enabled.Value;

            if (!key.Enabled.Value)
            {
                ToolTipService.SetShowOnDisabled(keyBorder, true);
                keyBorder.ToolTip = new ToolTip { Content = "Changes to this key are not supported" };
            }

            if (string.IsNullOrWhiteSpace(key.Image))
            {
                keyCap.Text = key.VisualName;
                keyCap.Tag = key.Tag;
                keyCap.FontSize = key.FontSize;
                keyCap.Visibility = Visibility.Visible;
            }
            else
            {
                keyCap.Visibility = Visibility.Hidden;

                if (System.IO.File.Exists(image_path))
                {
                    var memStream = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(image_path));
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = memStream;
                    image.EndInit();

                    if (key.Tag == DeviceKeys.NONE)
                        keyBorder.Background = new ImageBrush(image);
                    else
                    {
                        keyBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
                        keyBorder.OpacityMask = new ImageBrush(image);
                    }

                    isImage = true;
                }
            }
        }

        public DeviceKeys GetKey()
        {
            return associatedKey;
        }

        public void SetColor(Color key_color)
        {
            if (!key_color.Equals(current_color))
            {
                if (!isImage)
                {
                    keyBorder.Background = new SolidColorBrush(Utils.ColorUtils.MultiplyColorByScalar(key_color, 0.6));
                    keyBorder.BorderBrush = new SolidColorBrush(key_color);
                }
                else
                {
                    if (associatedKey != DeviceKeys.NONE)
                        keyBorder.Background = new SolidColorBrush(key_color);
                }
                current_color = key_color;
            }

            if (Global.key_recorder.HasRecorded(associatedKey))
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
            UpdateText();
        }

        private void keyBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border)
                virtualkeyboard_key_selected(associatedKey);
        }

        private void keyBorder_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void virtualkeyboard_key_selected(Devices.DeviceKeys key)
        {
            if (key != DeviceKeys.NONE)
            {
                if (Global.key_recorder.HasRecorded(key))
                    Global.key_recorder.RemoveKey(key);
                else
                    Global.key_recorder.AddKey(key);
            }
        }

        private void keyBorder_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        private void keyBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is Border)
                virtualkeyboard_key_selected(associatedKey);

            
        }

        public void UpdateText()
        {
            if (Global.kbLayout.LoadedLocalization.IsAutomaticGeneration())
            {

                //if (keyCap.Text.Length > 1)
                //    return;

                StringBuilder sb = new StringBuilder(2);
                var scan_code = KeyUtils.GetScanCode(associatedKey);
                if (scan_code == -1)
                    return;
                /*var key = KeyUtils.GetFormsKey((KeyboardKeys)associatedKey.LedID);
                var scan_code = KeyUtils.MapVirtualKeyEx((uint)key, KeyUtils.MapVirtualKeyMapTypes.MapvkVkToVsc, (IntPtr)0x8090809);*/

                int ret = KeyUtils.GetKeyNameTextW((uint)scan_code << 16, sb, 2);
                keyCap.Text = sb.ToString();
            }
        }
    }
}
