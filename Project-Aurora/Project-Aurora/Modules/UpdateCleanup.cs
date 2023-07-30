using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Lombok.NET;

namespace Aurora.Modules;

public partial class UpdateCleanup : AuroraModule
{
    protected override async Task Initialize()
    {
        CleanOldLogiDll();
        CleanLogs();
    }

    private static void CleanOldLogiDll()
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Directory.SetCurrentDirectory(path);
        var logiDll = Path.Combine(path, "LogitechLed.dll");
        if (File.Exists(logiDll))
            File.Delete(logiDll);
    }

    private static void CleanLogs()
    {
        var logFolder = Path.Combine(Global.AppDataDirectory, "Logs");
        var files = from file in Directory.EnumerateFiles(logFolder) select file;
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            if (fileName.StartsWith("2023-"))
            {
                File.Delete(file);
            }
        }
    }

    [Async]
    public override void Dispose()
    {
        //noop
    }
}