using Common.Devices;

namespace AuroraDeviceManager.Devices.Omen
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
