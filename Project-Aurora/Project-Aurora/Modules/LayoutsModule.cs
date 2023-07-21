using System.Threading.Tasks;
using System.Windows;
using Aurora.Settings;
using Lombok.NET;

namespace Aurora.Modules;

public sealed partial class LayoutsModule : IAuroraModule
{
    private KeyboardLayoutManager _layoutManager;
    private readonly TaskCompletionSource<KeyboardLayoutManager> _taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<KeyboardLayoutManager> LayoutManager => _taskCompletionSource.Task;

    public override Task InitializeAsync()
    {
        Initialize();
        return Task.CompletedTask;
    }

    public override void Initialize()
    {
        Global.logger.Info("Loading KB Layouts");
        _layoutManager = new KeyboardLayoutManager();
        Global.kbLayout = _layoutManager;
        Global.kbLayout.LoadBrandDefault();
        Global.logger.Info("Loaded KB Layouts");
        _taskCompletionSource.SetResult(_layoutManager);
    }

    [Async]
    public override void Dispose()
    {
        //noop
    }
}