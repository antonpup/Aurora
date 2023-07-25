using System.Threading.Tasks;
using System.Windows;
using Aurora.Settings;
using Lombok.NET;

namespace Aurora.Modules;

public sealed partial class LayoutsModule : AuroraModule
{
    private KeyboardLayoutManager _layoutManager;
    private readonly TaskCompletionSource<KeyboardLayoutManager> _taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<KeyboardLayoutManager> LayoutManager => _taskCompletionSource.Task;

    public override Task InitializeAsync()
    {
        Initialize();
        return Task.CompletedTask;
    }

    protected override async Task Initialize()
    {
        Global.logger.Information("Loading KB Layouts");
        _layoutManager = new KeyboardLayoutManager();
        Global.kbLayout = _layoutManager;
        Global.kbLayout.LoadBrandDefault();
        Global.logger.Information("Loaded KB Layouts");
        _taskCompletionSource.SetResult(_layoutManager);
    }

    [Async]
    public override void Dispose()
    {
        //noop
    }
}