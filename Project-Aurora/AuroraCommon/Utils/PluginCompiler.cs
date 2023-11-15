using System.Diagnostics;
using Serilog;

namespace Common.Utils;

public class PluginCompiler
{
    private readonly ILogger _logger;
    private readonly string _compilerPath;

    public PluginCompiler(ILogger logger, string compilerPath)
    {
        _logger = logger;
        _compilerPath = compilerPath;
    }

    public async Task Compile(string scriptPath)
    {
        var scriptChangeTime = File.GetLastWriteTime(scriptPath);
        var dllFile = scriptPath + ".dll";
        var dllCompileTime = File.Exists(dllFile) ? File.GetLastWriteTime(dllFile) : DateTime.UnixEpoch;

        if (scriptChangeTime < dllCompileTime)
        {
            _logger.Information("Script {Script} is up to date", scriptPath);
            return;
        }

        _logger.Information("Compiling script: {Script}", scriptPath);
        var compilerPath = Path.Combine(_compilerPath, "PluginCompiler.exe");
        var compilerProc = new ProcessStartInfo
        {
            WorkingDirectory = Path.GetDirectoryName(Environment.ProcessPath),
            FileName = compilerPath,
            Arguments = scriptPath,
        };
        var process = Process.Start(compilerProc);
        if (process == null)
        {
            throw new ApplicationException("PluginCompiler.exe not found!");
        }

        process.ErrorDataReceived += (_, args) =>
        {
            _logger.Error("Compiler: {}", args.Data);
        };

        try
        {
            await process.WaitForExitAsync();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Could not load script: {Script}", scriptPath);
        }
    }
}