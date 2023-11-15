using System.Threading.Tasks;

namespace Aurora.Devices;

public class DeviceContainer
{
    private readonly DeviceManager _deviceManager;
    public MemorySharedDevice Device { get; }

    public DeviceContainer(MemorySharedDevice device, DeviceManager deviceManager)
    {
        _deviceManager = deviceManager;
        Device = device;
    }

    public async Task EnableDevice()
    {
        await _deviceManager.EnableDevice(Device.DeviceName);
    }

    public async Task DisableDevice()
    {
        await _deviceManager.DisableDevice(Device.DeviceName);
    }
}