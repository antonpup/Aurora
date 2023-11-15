using System.Windows.Media;
using Common.Devices;

namespace Aurora.Settings.Controls.Keycaps;

public interface IKeycap
{
    protected static readonly SolidColorBrush DefaultColorBrush = new (Color.FromArgb(255, 0, 0, 0));

    static IKeycap()
    {
        DefaultColorBrush.Freeze();
    }
        
    void SetColor(Color keyColor);

    DeviceKeys GetKey();
}