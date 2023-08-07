using System;
using System.Threading.Tasks;
using Aurora.Modules.Razer;
using Aurora.Profiles;
using Lombok.NET;
using RazerSdkWrapper;

namespace Aurora.Modules;

public sealed partial class RazerSdkModule : AuroraModule
{
    private readonly Task<LightingStateManager> _lsm;
    private readonly TaskCompletionSource<RzSdkManager?> _sdkTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private RzSdkManager? _razerSdkManager;

    public RazerSdkModule(Task<LightingStateManager> lsm)
    {
        _lsm = lsm;
    }

    public Task<RzSdkManager?> RzSdkManager => _sdkTaskSource.Task;

    protected override async Task Initialize()
    {
        Global.logger.Information("Loading RazerSdkManager");
        if (RzHelper.IsSdkVersionSupported(RzHelper.GetSdkVersion()))
        {
            try
            {
                await _lsm; //wait for ChromaApplication.Settings to load
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
            HeadsetEnabled = true,
            ChromaLinkEnabled = true,
        };
        Global.razerSdkManager = _razerSdkManager;
        _razerSdkManager.DataUpdated += RzHelper.OnDataUpdated;
        _razerSdkManager.Start();

        RzHelper.Initialize();
    }

    [Async]
    public override void Dispose()
    {
        try
        {
            if (_razerSdkManager == null)
            {
                return;
            }
            _razerSdkManager.DataUpdated -= RzHelper.OnDataUpdated;
            _razerSdkManager.Dispose();
            Global.razerSdkManager = null;
        }
        catch (Exception exc)
        {
            Global.logger.Fatal(exc, "RazerManager failed to dispose!");
        }
    }
}