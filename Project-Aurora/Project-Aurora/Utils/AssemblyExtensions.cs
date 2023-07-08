using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aurora.Utils;

public static class AssemblyExtensions
{
    public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        Type?[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            types = e.Types;
        }

        return types.Where(type => type != null);
    }
}