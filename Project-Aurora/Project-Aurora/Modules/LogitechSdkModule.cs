using System.Threading.Tasks;
using Aurora.Modules.Logitech;
using Lombok.NET;

namespace Aurora.Modules;

public sealed partial class LogitechSdkModule : AuroraModule
{
    public static LogitechSdkListener LogitechSdkListener { get; } = new();

    protected override Task Initialize()
    {
        LogitechSdkListener.Initialize();
        return Task.CompletedTask;
    }


    [Async]
    public override void Dispose()
    {
        LogitechSdkListener.Dispose();
    }
}