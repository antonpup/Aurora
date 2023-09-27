using System.Threading.Tasks;

namespace Aurora.Devices;

public class DeviceContainer
{
    public MemorySharedDevice Device { get; }

    public DeviceContainer(MemorySharedDevice device)
    {
        Device = device;
    }

    public async Task EnableDevice()
    {
    }

    public async Task DisableDevice()
    {
    }
}