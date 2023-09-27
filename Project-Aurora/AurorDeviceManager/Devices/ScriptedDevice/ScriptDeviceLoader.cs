using System.Reflection;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace AurorDeviceManager.Devices.ScriptedDevice;

internal sealed class ScriptDeviceLoader : IDeviceLoader
{
    private readonly Lazy<ScriptEngine> _pythonEngine = new(Python.CreateEngine);
    private readonly string _scriptFolder;

    public ScriptDeviceLoader(string scriptFolder)
    {
        _scriptFolder = scriptFolder;
    }

    public IEnumerable<IDevice?> LoadDevices()
    {
        if (!Directory.Exists(_scriptFolder))
            Directory.CreateDirectory(_scriptFolder);

        var files = Directory.GetFiles(_scriptFolder);
        if (files.Length == 0)
            yield break;

        Global.Logger.Information("Loading device scripts from {ScriptFolder}", _scriptFolder);

        foreach (var deviceScript in files.OrderBy(s => s))
        {
            yield return LoadScript(deviceScript);
        }
    }

    private readonly List<string> _loadedScripts = new();

    private IDevice? LoadScript(string deviceScript)
    {
        var ext = Path.GetExtension(deviceScript);
        switch (ext)
        {
            case ".py":
                return LoadPython(deviceScript);
            case ".cs":
                //_loadedScripts.Add(deviceScript + ".dll"); TODO
                //PluginCompiler.Compile(deviceScript).Wait();
                //return LoadDll(deviceScript + ".dll");
            case ".dll":
                return _loadedScripts.Contains(deviceScript) ? null : LoadDll(deviceScript);
            default:
                Global.Logger.Error("Script with path {Path} has an unsupported type/ext! ({Extension})", deviceScript, ext);
                break;
        }
        return null;
    }

    private IDevice? LoadPython(string deviceScript)
    {
        var scope = _pythonEngine.Value.ExecuteFile(deviceScript);
        if (scope.TryGetVariable("main", out var mainType))
        {
            var script = _pythonEngine.Value.Operations.CreateInstance(mainType);

            IDevice scriptedDevice = new ScriptedDevice(script);
            Global.Logger.Information("Loaded device script {DeviceScript}", deviceScript);
            return scriptedDevice;
        }

        Global.Logger.Error("Script \"{Script}\" does not contain a public 'main' class", deviceScript);
        return null;
    }

    private static IDevice? LoadDll(string deviceScript)
    {
        var data = File.ReadAllBytes(deviceScript);
        try
        {
            var scriptAssembly = Assembly.Load(data);
            var typ = scriptAssembly.ExportedTypes.First(type => type.FullName?.StartsWith("css_root+") ?? false);
            var constructorInfo = typ.GetConstructor(Type.EmptyTypes);
            if (constructorInfo == null)
            {
                Global.Logger.Information(
                    "Script {DeviceScript} does not have parameterless constructor or device class isn\'t the first one",
                    deviceScript);
                return null;
            }

            dynamic script = Activator.CreateInstance(typ);
            IDevice scriptedDevice = new ScriptedDevice(script);
            Global.Logger.Information("Loaded device script {DeviceScript}", deviceScript);
            return scriptedDevice;
        }
        catch (Exception e)
        {
            Global.Logger.Error(e, "Failed to load script {Script}", deviceScript);
            return null;
        }
    }

    public void Dispose()
    {
        if (_pythonEngine.IsValueCreated)
        {
            _pythonEngine.Value.Runtime.Shutdown();
        }
    }
}