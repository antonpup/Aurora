using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Aurora.Modules.Plugins;

public static class PluginCompiler
{
    public static async Task Compile(string scriptPath)
    {
        var scriptChangeTime = File.GetLastWriteTime(scriptPath);
        var dllFile = scriptPath + ".dll";
        var dllCompileTime = File.Exists(dllFile) ? File.GetLastWriteTime(dllFile) : DateTime.UnixEpoch;

        if (scriptChangeTime < dllCompileTime)
        {
            Global.logger.Information("Script {Script} is up to date", scriptPath);
            return;
        }

        Global.logger.Information("Compiling script: {Script}", scriptPath);
        var compilerPath = Path.Combine(Global.ExecutingDirectory, "PluginCompiler.exe");
        var compilerProc = new ProcessStartInfo
        {
            FileName = compilerPath,
            Arguments = scriptPath
        };
        var process = Process.Start(compilerProc);
        if (process == null)
        {
            throw new ApplicationException("PluginCompiler.exe not found!");
        }

        try
        {
            await process.WaitForExitAsync();
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Could not load script: {Script}", scriptPath);
        }
    }
}