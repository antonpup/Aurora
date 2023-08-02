using System.Threading.Tasks;
using System.Windows;
using Aurora.Settings;
using Lombok.NET;
using RazerSdkWrapper;

namespace Aurora.Modules;

public sealed partial class LayoutsModule : AuroraModule
{
    private readonly Task<RzSdkManager?> _rzSdk;
    
    private KeyboardLayoutManager? _layoutManager;
    private readonly TaskCompletionSource<KeyboardLayoutManager> _taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public LayoutsModule(Task<RzSdkManager?> rzSdk)
    {
        _rzSdk = rzSdk;
    }

    public Task<KeyboardLayoutManager> LayoutManager => _taskCompletionSource.Task;

    public override async Task InitializeAsync()
    {
        await Initialize();
    }

    protected override async Task Initialize()
    {
        Global.logger.Information("Loading KB Layouts");
        _layoutManager = new KeyboardLayoutManager(_rzSdk);
        Global.kbLayout = _layoutManager;
        await Global.kbLayout.LoadBrandDefault();
        Global.logger.Information("Loaded KB Layouts");
        _taskCompletionSource.SetResult(_layoutManager);
    }

    [Async]
    public override void Dispose()
    {
        //noop
    }
}