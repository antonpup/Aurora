using Aurora.Profiles;
using Lombok.NET;
using Aurora.Modules.HardwareMonitor;

namespace Aurora.Modules;

public sealed partial class HardwareMonitorModule : IAuroraModule
{
    
    public override void Initialize()
    {
        if (Global.Configuration.EnableHardwareInfo)
        {
            LocalPcInformation.HardwareMonitor = new HardwareMonitor.HardwareMonitor();
        }
    }


    [Async]
    public override void Dispose()
    {
        LocalPcInformation.HardwareMonitor = new NoopHardwareMonitor();
    }
}