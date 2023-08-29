using System.Windows.Media;

namespace Aurora.Settings.Keycaps;

public interface IKeycap
{
    protected static readonly SolidColorBrush DefaultColorBrush = new (Color.FromArgb(255, 0, 0, 0));

    static IKeycap()
    {
        DefaultColorBrush.Freeze();
    }
        
    void SetColor(Color keyColor);

    Devices.DeviceKeys GetKey();
}