using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Profiles;
using Plugin_Example.Layers;

namespace Plugin_Example
{
    

    public class PluginMain : IPlugin
    {
        public string ID { get; private set; } = "PluginExample";

        public string Title { get; private set; } = "Example Plugin";

        public string Author { get; private set; } = "YOU";

        public Version Version { get; private set; } = new Version(0, 1);

        private IPluginHost pluginHost;

        public IPluginHost PluginHost { get { return pluginHost; }
            set {
                pluginHost = value;
                if (value is ProfilesManager)
                {
                    ((ProfilesManager)value).RegisterLayerHandler(new LayerHandlerEntry("ExampleLayer", "Example Layer", typeof(ExampleLayerHandler)));
                }
            }
        }

        public PluginMain()
        {

        }
    }
}
