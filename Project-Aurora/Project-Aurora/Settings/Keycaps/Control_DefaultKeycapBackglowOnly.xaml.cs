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
using Common.Devices;

namespace Aurora.Settings.Keycaps;

/// <summary>
/// Interaction logic for Control_DefaultKeycapBackglowOnly.xaml
/// </summary>
public partial class Control_DefaultKeycapBackglowOnly : IKeycap
{
    private Color _currentColor = Color.FromArgb(0, 0, 0, 0);
    private readonly DeviceKeys _associatedKey = DeviceKeys.NONE;
    private readonly bool _isImage;

    public Control_DefaultKeycapBackglowOnly()
    {
        InitializeComponent();
    }

    public Control_DefaultKeycapBackglowOnly(KeyboardKey key, string imagePath)
    {
        InitializeComponent();

        _associatedKey = key.Tag;

        Width = key.Width;
        Height = key.Height;

        //Keycap adjustments
        KeyBorder.BorderThickness = new Thickness(string.IsNullOrWhiteSpace(key.Image) ? 1.5 : 0.0);
        KeyBorder.IsEnabled = key.Enabled.Value;

        if (!key.Enabled.Value)
        {
            ToolTipService.SetShowOnDisabled(KeyBorder, true);
            KeyBorder.ToolTip = new ToolTip { Content = "Changes to this key are not supported" };
        }

        if (string.IsNullOrWhiteSpace(key.Image))
        {
            KeyCap.Text = KeyUtils.GetAutomaticText(_associatedKey) ?? key.VisualName;
            KeyCap.Tag = key.Tag;
            KeyCap.FontSize = key.FontSize;
            KeyCap.Visibility = Visibility.Visible;
        }
        else
        {
            KeyCap.Visibility = Visibility.Hidden;
            GridBackglow.Visibility = Visibility.Hidden;

            if (File.Exists(imagePath))
            {
                var memStream = new MemoryStream(File.ReadAllBytes(imagePath));
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = memStream;
                image.EndInit();

                if (key.Tag == DeviceKeys.NONE)
                    KeyBorder.Background = new ImageBrush(image);
                else
                {
                    KeyBorder.Background = IKeycap.DefaultColorBrush;
                    KeyBorder.OpacityMask = new ImageBrush(image);
                }

                _isImage = true;
            }
        }
    }

    public DeviceKeys GetKey()
    {
        return _associatedKey;
    }

    public void SetColor(Color key_color)
    {
        if (!_currentColor.Equals(key_color))
        {
            if (!_isImage)
                GridBackglow.Background = new SolidColorBrush(key_color);
            else
            {
                if (_associatedKey != DeviceKeys.NONE)
                    KeyBorder.Background = new SolidColorBrush(key_color);
            }
            _currentColor = key_color;
        }

        if (Global.key_recorder.HasRecorded(_associatedKey))
            KeyBorder.Background = new SolidColorBrush(Color.FromArgb(255, 0, (byte)(Math.Min(Math.Pow(Math.Cos(Time.GetMilliSeconds() / 1000.0 * Math.PI) + 0.05, 2.0), 1.0) * 255), 0));
        else
        {
            if (KeyBorder.IsEnabled)
            {
                if(!_isImage)
                    KeyBorder.Background = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30));
                else
                    KeyBorder.Background = new SolidColorBrush(key_color);
            }
            else
            {
                KeyBorder.Background = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
            }
        }
    }

    private void keyBorder_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (sender is Border)
            virtualkeyboard_key_selected(_associatedKey);
    }

    private void virtualkeyboard_key_selected(DeviceKeys key)
    {
        if (key == DeviceKeys.NONE || Global.key_recorder == null) return;
        if (Global.key_recorder.HasRecorded(key))
            Global.key_recorder.RemoveKey(key);
        else
            Global.key_recorder.AddKey(key);
    }

    private void keyBorder_MouseEnter(object? sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && sender is Border)
            virtualkeyboard_key_selected(_associatedKey);
    }
}