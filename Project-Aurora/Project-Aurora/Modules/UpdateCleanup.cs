using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
        //TODO regex match
        var logFile = new Regex(".*\\.log");
        var files = from file in Directory.EnumerateFiles(logFolder)
            where logFile.IsMatch(Path.GetFileName(file))
            orderby File.GetCreationTime(file) descending
            select file;
        foreach (var file in files.Skip(8))
        {
            File.Delete(file);
        }
    }

    [Async]
    public override void Dispose()
    {
        //noop
    }
}