using Aurora.Devices.Layout;
using System.Drawing;

namespace Aurora.Settings.Keycaps
{
    public interface IKeycap
    {
        void SetColor(Color key_color);

        DeviceLED GetKey();
    }
}
