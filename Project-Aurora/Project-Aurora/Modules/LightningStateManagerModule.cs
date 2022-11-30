using System.Threading.Tasks;
using Aurora.Profiles;
using Aurora.Settings;
using Aurora.Utils;
using Lombok.NET;

namespace Aurora.Modules;

[RequiredArgsConstructor]
public sealed partial class LightningStateManagerModule : IAuroraModule
{
    private readonly Task<PluginManager> _pluginManager;
    private readonly Task<IpcListener> _ipcListener;
    private readonly Task<AuroraHttpListener> _httpListener;

    private TaskCompletionSource<LightingStateManager> _taskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private LightingStateManager _manager;

    public Task<LightingStateManager> LightningStateManager => _taskSource.Task;

    [Async]
    public void Initialize()
    {
        Global.logger.Info("Loading Applications");
        var lightingStateManager = new LightingStateManager(_pluginManager, _ipcListener);
        Global.LightingStateManager = lightingStateManager;
        lightingStateManager.Initialize();
        
        _taskSource.SetResult(lightingStateManager);

        var ipcListener = _ipcListener.Result;
        ipcListener.NewGameState += lightingStateManager.GameStateUpdate;
        ipcListener.WrapperConnectionClosed += lightingStateManager.ResetGameState;

        var httpListener = _httpListener.Result;
        httpListener.NewGameState += lightingStateManager.GameStateUpdate;
    }

    [Async]
    public void Dispose()
    {
        _manager?.SaveAll();
        Global.LightingStateManager?.Dispose();
        Global.LightingStateManager = null;
        _manager = null;
    }
}