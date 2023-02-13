using System.Threading.Tasks;
using Aurora.Settings;
using Lombok.NET;

namespace Aurora.Modules;

public sealed partial class LayoutsModule : IAuroraModule
{
    private KeyboardLayoutManager _layoutManager;
    private readonly TaskCompletionSource<KeyboardLayoutManager> _taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<KeyboardLayoutManager> LayoutManager => _taskCompletionSource.Task;

    [Async]
    public void Initialize()
    {
        Global.logger.Info("Loading KB Layouts");
        _layoutManager = new KeyboardLayoutManager();
        Global.kbLayout = _layoutManager;
        Global.kbLayout.LoadBrandDefault();
        Global.logger.Info("Loaded KB Layouts");
        _taskCompletionSource.SetResult(_layoutManager);
    }

    [Async]
    public void Dispose()
    {
        //noop
    }
}