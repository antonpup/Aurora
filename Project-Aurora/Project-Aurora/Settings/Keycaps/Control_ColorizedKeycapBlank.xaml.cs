using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora.Devices;
using Aurora.Utils;

namespace Aurora.Settings.Keycaps;

/// <summary>
/// Interaction logic for Control_ColorizedKeycapBlank.xaml
/// </summary>
public partial class Control_ColorizedKeycapBlank : IKeycap
{
    private Color _currentColor = Color.FromArgb(0, 0, 0, 0);
    private readonly DeviceKeys _associatedKey = DeviceKeys.NONE;
    private readonly bool _isImage;
    private readonly SolidColorBrush _keyBorderBorderBrush = new(Colors.Gray);
    private readonly SolidColorBrush _keyBorderBackground = new(Colors.Black);

    public Control_ColorizedKeycapBlank()
    {
        InitializeComponent();
    }

    public Control_ColorizedKeycapBlank(KeyboardKey key, string imagePath)
    {
        InitializeComponent();

        _associatedKey = key.Tag;

        Width = key.Width;
        Height = key.Height;

        //Keycap adjustments
        KeyBorder.BorderThickness = new Thickness(string.IsNullOrWhiteSpace(key.Image) ? 1.5 : 0.0);

        var keyEnabled = key.Enabled.GetValueOrDefault(true);
        KeyBorder.IsEnabled = keyEnabled;

        if (!keyEnabled)
        {
            ToolTipService.SetShowOnDisabled(KeyBorder, true);
            KeyBorder.ToolTip = new ToolTip { Content = "Changes to this key are not supported" };
        }

        KeyBorder.BorderBrush = _keyBorderBorderBrush;
        KeyBorder.Background = _keyBorderBackground;

        if (string.IsNullOrWhiteSpace(key.Image)) return;
        if (!File.Exists(imagePath)) return;
        var memStream = new MemoryStream(File.ReadAllBytes(imagePath));
        var image = new BitmapImage();
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

    public DeviceKeys GetKey()
    {
        return _associatedKey;
    }

    public void SetColor(Color keyColor)
    {
        if (Global.key_recorder?.HasRecorded(_associatedKey) ?? false)
        {
            var g = (byte)(Math.Min(Math.Pow(Math.Cos(Time.GetMilliSeconds() / 1000.0 * Math.PI) + 0.05, 2.0), 1.0) * 255);
            _keyBorderBackground.Color = Color.FromArgb(255, 0, g, 0);
            return;
        }

        if (keyColor.Equals(_currentColor))
        {
            return;
        }

        if (!KeyBorder.IsEnabled) return;

        if (_isImage)
        {
            _keyBorderBackground.Color = keyColor;
        }
        else
        {
            _keyBorderBackground.Color = keyColor * 0.6f;
            _keyBorderBorderBrush.Color = keyColor;
        }

        _currentColor = keyColor;
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