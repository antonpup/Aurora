using Common.Devices;

namespace Common;

public static class Constants
{
    public const string DeviceManagerPipe = "Aurora\\DeviceManager";
    public const string AuroraInterfacePipe = "aurora\\interface";

    public static readonly int MaxKeyId = Enum.GetValues(typeof(DeviceKeys)).Cast<int>().Max() + 1;
    
    public const string DeviceLedMap = "DeviceLedMap";
    public const string DeviceInformations = "DeviceInformations";

    public const char StringSplit = '~';
}