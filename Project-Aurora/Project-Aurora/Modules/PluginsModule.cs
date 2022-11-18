using Aurora.Settings;
using Lombok.NET;

namespace Aurora.Modules;

public partial class PluginsModule : IAuroraModule
{
    [Async]
    public void Initialize()
    {
        Global.logger.Info("Loading Plugins");
        (Global.PluginManager = new PluginManager()).Initialize();
        Global.logger.Info("Loaded Plugins");
    }
    
    [Async]
    public void Dispose()
    {
        Global.PluginManager.SaveSettings();
    }
}