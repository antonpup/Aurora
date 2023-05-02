using System.Reflection;
using Aurora.Profiles;
using Aurora.Profiles.Generic_Application;
using Aurora.Settings;
using MemoryAccessProfiles.Profiles.Borderlands2;
using MemoryAccessProfiles.Profiles.CloneHero;
using MemoryAccessProfiles.Profiles.Dishonored;
using MemoryAccessProfiles.Profiles.ResidentEvil2;

namespace MemoryAccessProfiles;

public class PluginMain : IPlugin
{
    public string ID { get; private set; } = "MemoryAccessProfiles";

    public string Title { get; private set; } = "Memory Access Profiles";

    public string Author { get; private set; } = "Aurora-RGB";

    public Version Version { get; private set; } = new Version(0, 1);

    private IPluginHost pluginHost;
    public IPluginHost PluginHost { get { return pluginHost; }
        set {
            pluginHost = value;
            //Add stuff to the plugin manager
        }
    }

    private List<Application> profiles = new List<Application>()
    {
        new Borderlands2(), new CloneHero(), new Dishonored(), new ResidentEvil2()
    };

    public void ProcessManager(object manager)
    {
        if (manager is not LightingStateManager lightingStateManager) return;
        
        // Register all Application types in the assembly
        foreach (var inst in profiles)
            lightingStateManager.RegisterEvent(inst);
    }
}