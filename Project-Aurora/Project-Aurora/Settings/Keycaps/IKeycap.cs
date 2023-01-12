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
        protected static readonly SolidColorBrush DefaultColorBrush = new (Color.FromArgb(255, 0, 0, 0));
        
        void SetColor(Color keyColor);

        Devices.DeviceKeys GetKey();
    }
}
