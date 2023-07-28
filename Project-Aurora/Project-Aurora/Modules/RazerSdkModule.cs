using System;
using System.Threading.Tasks;
using Aurora.Modules.Razer;
using Lombok.NET;
using RazerSdkWrapper;
using RazerSdkWrapper.Data;

namespace Aurora.Modules;

public sealed partial class RazerSdkModule : AuroraModule
{
    private readonly TaskCompletionSource<RzSdkManager?> _sdkTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private RzSdkManager? _razerSdkManager;

    public Task<RzSdkManager?> RzSdkManager => _sdkTaskSource.Task;

    protected override async Task Initialize()
    {
        Global.logger.Information("Loading RazerSdkManager");
        if (RzHelper.IsSdkVersionSupported(RzHelper.GetSdkVersion()))
        {
            try
            {
                TryLoadChroma();

                Global.logger.Information("RazerSdkManager loaded successfully!");
                _sdkTaskSource.SetResult(_razerSdkManager);
            }
            catch (Exception exc)
            {
                Global.logger.Fatal(exc, "RazerSdkManager failed to load!");
                _sdkTaskSource.SetResult(null);
            }
        }
        else
        {
            Global.logger.Warning("Currently installed razer sdk version \"{RzVersion}\" is not supported by the RazerSdkManager!",
                RzHelper.GetSdkVersion());
            _sdkTaskSource.SetResult(null);
        }
    }

    private void TryLoadChroma()
    {
        _razerSdkManager = new RzSdkManager
        {
            KeyboardEnabled = true,
            MouseEnabled = true,
            MousepadEnabled = true,
            AppListEnabled = true,
            HeadsetEnabled = true,
            ChromaLinkEnabled = true,
        };
        var rzKeyboardDataProvider = _razerSdkManager.GetDataProvider<RzKeyboardDataProvider>();
        Global.razerSdkManager = _razerSdkManager;

        _razerSdkManager.DataUpdated += RzHelper.OnDataUpdated;

        RzHelper.Initialize();
    }

    [Async]
    public override void Dispose()
    {
        try
        {
            _razerSdkManager.DataUpdated -= RzHelper.OnDataUpdated;
            _razerSdkManager?.Dispose();
            Global.razerSdkManager = null;
        }
        catch (Exception exc)
        {
            Global.logger.Fatal(exc, "RazerManager failed to dispose!");
        }
    }
}