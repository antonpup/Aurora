using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora.Devices;
using Aurora.Utils;

namespace Aurora.Settings.Keycaps
{
    /// <summary>
    /// Interaction logic for Control_ColorizedKeycap.xaml
    /// </summary>
    public partial class Control_ColorizedKeycap : IKeycap
    {
        private Color _currentColor;
        private readonly DeviceKeys _associatedKey = DeviceKeys.NONE;
        private readonly bool _isImage;

        public Control_ColorizedKeycap()
        {
            InitializeComponent();
        }

        public Control_ColorizedKeycap(KeyboardKey key, string imagePath)
        {
            InitializeComponent();

            _associatedKey = key.Tag;

            Width = key.Width;
            Height = key.Height;

            //Keycap adjustments
            keyBorder.BorderThickness = new Thickness(string.IsNullOrWhiteSpace(key.Image) ? 1.5 : 0.0);
            keyBorder.IsEnabled = key.Enabled.GetValueOrDefault(true);

            if (!key.Enabled.GetValueOrDefault(true))
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

                if (!File.Exists(imagePath)) return;
                var memStream = new MemoryStream(File.ReadAllBytes(imagePath));
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = memStream;
                image.EndInit();

                if (key.Tag == DeviceKeys.NONE)
                    keyBorder.Background = new ImageBrush(image);
                else
                {
                    keyBorder.Background = IKeycap.DefaultColorBrush;
                    keyBorder.OpacityMask = new ImageBrush(image);
                }

                _isImage = true;
            }
        }

        public DeviceKeys GetKey()
        {
            return _associatedKey;
        }

        public void SetColor(Color keyColor)
        {
            if (!keyColor.Equals(_currentColor))
            {
                if (!_isImage)
                {
                    keyBorder.Background = new SolidColorBrush(ColorUtils.MultiplyColorByScalar(keyColor, 0.6));
                    keyBorder.BorderBrush = new SolidColorBrush(keyColor);
                }
                else
                {
                    if (_associatedKey != DeviceKeys.NONE)
                        keyBorder.Background = new SolidColorBrush(keyColor);
                }
                _currentColor = keyColor;
            }

            if (Global.key_recorder.HasRecorded(_associatedKey))
                keyBorder.Background = new SolidColorBrush(
                    Color.FromArgb(255, 0, (byte)(Math.Min(Math.Pow(Math.Cos(Time.GetMilliSeconds() / 1000.0 * Math.PI) + 0.05, 2.0), 1.0) * 255), 0));
            else
            {
                if (keyBorder.IsEnabled)
                {
                    if (_isImage)
                        keyBorder.Background = new SolidColorBrush(keyColor);
                    else
                        keyBorder.Background = new SolidColorBrush(ColorUtils.MultiplyColorByScalar(keyColor, 0.6));
                }
                else
                {
                    keyBorder.Background = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
                }
            }
            UpdateText();
        }

        private void keyBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border)
                virtualkeyboard_key_selected(_associatedKey);
        }

        private void keyBorder_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void virtualkeyboard_key_selected(DeviceKeys key)
        {
            if (key == DeviceKeys.NONE) return;
            if (Global.key_recorder.HasRecorded(key))
                Global.key_recorder.RemoveKey(key);
            else
                Global.key_recorder.AddKey(key);
        }

        private void keyBorder_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        private void keyBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is Border)
                virtualkeyboard_key_selected(_associatedKey);
        }

        private void UpdateText()
        {
            if (!Global.kbLayout.LoadedLocalization.IsAutomaticGeneration()) return;
            StringBuilder sb = new StringBuilder(2);
            var scanCode = KeyUtils.GetScanCode(_associatedKey);
            if (scanCode == -1)
                return;

            keyCap.Text = sb.ToString();
        }
    }
}
