using System.IO;
using System.Reflection;
using Lombok.NET;

namespace Aurora.Modules;

public partial class UpdateCleanup : IAuroraModule
{
    public override void Initialize()
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Directory.SetCurrentDirectory(path);
        var logiDll = Path.Combine(path, "LogitechLed.dll");
        if (File.Exists(logiDll))
            File.Delete(logiDll);
    }

    [Async]
    public override void Dispose()
    {
        //noop
    }
}