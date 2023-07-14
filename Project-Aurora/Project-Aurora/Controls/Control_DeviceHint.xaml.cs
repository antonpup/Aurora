using System.Windows;
using System.Windows.Media;

namespace Aurora.Controls;

public partial class Control_DeviceHint
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(Control_DeviceHint), new PropertyMetadata(default(string)));
    
    public static readonly DependencyProperty CircleBackgroundProperty = DependencyProperty.Register(
        nameof(CircleBackground), typeof(Brush), typeof(Control_DeviceHint), new PropertyMetadata(Brushes.Gray));

    public static readonly DependencyProperty HintTooltipProperty = DependencyProperty.Register(
        nameof(HintTooltip), typeof(string), typeof(Control_DeviceHint), new PropertyMetadata(default(string)));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Brush CircleBackground
    {
        get => (Brush)GetValue(CircleBackgroundProperty);
        set => SetValue(CircleBackgroundProperty, value);
    }

    public string HintTooltip
    {
        get => (string)GetValue(HintTooltipProperty);
        set => SetValue(HintTooltipProperty, value);
    }

    public Control_DeviceHint()
    {
        InitializeComponent();
        DataContext = this;
    }
}