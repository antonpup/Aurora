using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Devices.Omen
{
    class OmenKeys
    {
        private static int KEYBOARD_KEY_LOGO = 128;

        static public int GetKey(DeviceKeys key)
        {
            if (key == DeviceKeys.HOME)
            {
                return KEYBOARD_KEY_LOGO;
            }
            return (int)key;
        }
    }
}
