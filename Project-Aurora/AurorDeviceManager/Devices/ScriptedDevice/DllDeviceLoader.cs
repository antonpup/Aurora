using System.Collections.Immutable;
using System.Reflection;

namespace AurorDeviceManager.Devices.ScriptedDevice;

internal sealed class DllDeviceLoader : IDeviceLoader
{
    private readonly string _dllFolder;

    private readonly List<Assembly> _deviceAssemblies = new();

    public DllDeviceLoader(string dllFolder)
    {
        _dllFolder = dllFolder;
    }

    public IEnumerable<IDevice> LoadDevices()
    {
        if (!Directory.Exists(_dllFolder))
            Directory.CreateDirectory(_dllFolder);

        var files = Directory.GetFiles(_dllFolder, "*.dll");
        if (files.Length == 0)
            return ImmutableList<IDevice>.Empty;

        Global.Logger.Information("Loading devices plugins from {DllFolder}", _dllFolder);

        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

        var devices = new List<IDevice>();
        foreach (var deviceDll in files)
        {
            try
            {
                var deviceAssembly = Assembly.LoadFile(deviceDll);

                foreach (var type in deviceAssembly.GetExportedTypes())
                {
                    if (!typeof(IDevice).IsAssignableFrom(type) || type.IsAbstract) continue;
                    _deviceAssemblies.Add(deviceAssembly);
                    var devDll = (IDevice)Activator.CreateInstance(type);
                    Global.Logger.Information("Loaded device plugin {DeviceDll}", deviceDll);
                    devices.Add(devDll);
                }
            }
            catch (Exception e)
            {
                Global.Logger.Error(e, "Error loading device dll: {DeviceDll}", deviceDll);
            }
        }

        return devices;
    }

    private Assembly CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        if (args.RequestingAssembly == null || !_deviceAssemblies.Contains(args.RequestingAssembly)) return null;
        var searchDir = Path.GetDirectoryName(args.RequestingAssembly.Location);
        foreach (var file in Directory.GetFiles(searchDir, "*.dll"))
        {
            var assemblyName = AssemblyName.GetAssemblyName(file);
            if (assemblyName.FullName == args.Name)
            {
                return AppDomain.CurrentDomain.Load(assemblyName);
            }
        }
        foreach (var file in Directory.GetFiles(Path.Combine(searchDir, "x64"), "*.dll"))
        {
            var assemblyName = AssemblyName.GetAssemblyName(file);
            if (assemblyName.FullName == args.Name)
            {
                return AppDomain.CurrentDomain.Load(assemblyName);
            }
        }
        return null;
    }

    public void Dispose()
    {
    }
}