using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Aurora.Settings.Keycaps
{
    public interface IKeycap
    {
        void SetColor(Color key_color);

        Devices.DeviceKeys GetKey();
    }
}
