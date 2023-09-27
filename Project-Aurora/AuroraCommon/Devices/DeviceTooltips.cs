namespace Common.Devices;

public readonly record struct DeviceTooltips
{
    public readonly bool Recommended;
    public readonly bool Beta;
    public readonly string? Info;
    public readonly string? SdkLink;

    public DeviceTooltips()
    {
    }

    public DeviceTooltips(bool recommended, bool beta, string? info, string? sdkLink)
    {
        Recommended = recommended;
        Beta = beta;
        Info = info;
        SdkLink = sdkLink;
    }
}