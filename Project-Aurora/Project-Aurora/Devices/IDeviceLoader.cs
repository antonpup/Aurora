using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aurora.Utils;

namespace Aurora.Devices;

internal interface IDeviceLoader
{
    IEnumerable<IDevice?> LoadDevices();
}

internal class AssemblyDeviceLoader : IDeviceLoader
{
    public IEnumerable<IDevice> LoadDevices()
    {
        Global.logger.Information("Loading devices from assembly...");
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
}