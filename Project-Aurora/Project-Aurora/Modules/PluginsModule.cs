using System.Threading.Tasks;
using Aurora.Settings;
using Lombok.NET;

namespace Aurora.Modules;

public sealed partial class PluginsModule : IAuroraModule
{
    private readonly TaskCompletionSource<PluginManager> _pluginManagerSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<PluginManager> PluginManager => _pluginManagerSource.Task;

    private PluginManager _pluginManager;

    [Async]
    public void Initialize()
    {
        Global.logger.Info("Loading Plugins");
        _pluginManager = new PluginManager();
        _pluginManager.Initialize();
        _pluginManagerSource.SetResult(_pluginManager);
        Global.logger.Info("Loaded Plugins");
    }
    
    [Async]
    public void Dispose()
    {
        _pluginManager.SaveSettings();
    }
}