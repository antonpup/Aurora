using System.Reflection;
using AurorDeviceManager.Utils;

namespace AurorDeviceManager.Devices;

internal interface IDeviceLoader : IDisposable
{
    IEnumerable<IDevice?> LoadDevices();
}

internal sealed class AssemblyDeviceLoader : IDeviceLoader
{
    public IEnumerable<IDevice> LoadDevices()
    {
        Global.Logger.Information("Loading devices from assembly...");
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(LoadAssemblyDevices);
    }

    private static IEnumerable<IDevice> LoadAssemblyDevices(Assembly assembly)
    {
        return from type in assembly.GetLoadableTypes()
            where typeof(IDevice).IsAssignableFrom(type)
                  && !type.IsAbstract
                  && type != typeof(ScriptedDevice.ScriptedDevice)
            let inst = (IDevice)Activator.CreateInstance(type)
            orderby inst.DeviceName
            select inst;
    }

    public void Dispose()
    {
    }
}