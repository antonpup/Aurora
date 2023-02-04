using Aurora.Profiles;
using Lombok.NET;
using Aurora.Modules.HardwareMonitor;

namespace Aurora.Modules;

public sealed partial class HardwareMonitorModule : IAuroraModule
{
    
    [Async]
    public void Initialize()
    {
        if (Global.Configuration.EnableHardwareInfo)
        {
            LocalPcInformation.HardwareMonitor = new HardwareMonitor.HardwareMonitor();
        }
    }


    [Async]
    public void Dispose()
    {
        LocalPcInformation.HardwareMonitor = new NoopHardwareMonitor();
    }
}