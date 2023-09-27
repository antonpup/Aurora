using Common.Devices;

namespace Common;

public static class Constants
{
    public const string DeviceManagerPipe = "Aurora\\DeviceManager";
    public static readonly int MaxKeyId = Enum.GetValues(typeof(DeviceKeys)).Cast<int>().Max() + 1;
    
    public const string DeviceLedMap = "DeviceLedMap";
    public const string DeviceInformations = "DeviceInformations";
    public const string DeviceVariables = "DeviceVariables";

    public const char StringSplit = '~';
}