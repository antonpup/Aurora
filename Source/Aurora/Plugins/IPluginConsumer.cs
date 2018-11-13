using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Aurora.Plugins
{
    public interface IPluginConsumer
    {
        void Visit(List<Type> plugin);
    }
}
