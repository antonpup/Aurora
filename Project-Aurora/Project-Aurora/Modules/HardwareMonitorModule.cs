using System.Threading.Tasks;
using Aurora.Profiles;
using Lombok.NET;
using Aurora.Modules.HardwareMonitor;

namespace Aurora.Modules;

public sealed partial class HardwareMonitorModule : AuroraModule
{
    protected override async Task Initialize()
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