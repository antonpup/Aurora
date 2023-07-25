using System.Threading.Tasks;
using Aurora.Modules.Logitech;
using Lombok.NET;

namespace Aurora.Modules;

public sealed partial class LogitechSdkModule : AuroraModule
{
    public static LogitechSdkListener LogitechSdkListener { get; private set; }
    
    private LogitechSdkListener _logitechSdkListener;

    protected override Task Initialize()
    {
        _logitechSdkListener = new LogitechSdkListener();
        LogitechSdkListener = _logitechSdkListener;
        return Task.CompletedTask;
    }


    [Async]
    public override void Dispose()
    {
        _logitechSdkListener.Dispose();
    }
}