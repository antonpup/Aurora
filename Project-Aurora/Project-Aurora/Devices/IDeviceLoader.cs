using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aurora.Devices;

internal interface IDeviceLoader
{
    IEnumerable<IDevice> LoadDevices();
}

internal class AssemblyDeviceLoader : IDeviceLoader
{
    public IEnumerable<IDevice> LoadDevices()
    {
        Global.logger.Info("Loading devices from assembly...");
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(LoadAssemblyDevices);
    }

    private static IEnumerable<IDevice> LoadAssemblyDevices(Assembly assembly)
    {
        return from type in assembly.GetTypes()
            where typeof(IDevice).IsAssignableFrom(type)
                  && !type.IsAbstract
                  && type != typeof(ScriptedDevice.ScriptedDevice)
            let inst = (IDevice)Activator.CreateInstance(type)
            orderby inst.DeviceName
            select inst;
    }
}