using System;
using System.Collections.Generic;
using System.Text;
using Aurora.Applications;

namespace Aurora.Plugins
{
    public abstract class PluginBase
    {
        public abstract string Name { get; }
        public abstract string Author { get; }
        public abstract string PluginSite { get; }

        public virtual void Process(IPluginConsumer plugin) { }
        //Create additional ones for each IPluginConsumer
        public virtual void Process(ApplicationManager plugin) { }
    }
}
