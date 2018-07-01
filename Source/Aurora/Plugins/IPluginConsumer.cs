using System;
using System.Collections.Generic;
using System.Text;

namespace Aurora.Plugins
{
    public interface IPluginConsumer
    {
        void Visit(PluginBase plugin);
    }
}
