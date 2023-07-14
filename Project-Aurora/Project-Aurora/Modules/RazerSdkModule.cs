using System;
using System.Threading.Tasks;
using Aurora.Modules.Razer;
using Lombok.NET;
using RazerSdkWrapper;

namespace Aurora.Modules;

public sealed partial class RazerSdkModule : IAuroraModule
{
    private readonly TaskCompletionSource<RzSdkManager?> _sdkTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private RzSdkManager? _razerSdkManager;

    public Task<RzSdkManager?> RzSdkManager => _sdkTaskSource.Task;

    [Async]
    public void Initialize()
    {
        Global.logger.Info("Loading RazerSdkManager");
        if (RzHelper.IsSdkVersionSupported(RzHelper.GetSdkVersion()))
        {
            try
            {
                TryLoadChroma();

                Global.logger.Info("RazerSdkManager loaded successfully!");
                _sdkTaskSource.SetResult(_razerSdkManager);
            }
            catch (Exception exc)
            {
                Global.logger.Fatal("RazerSdkManager failed to load!");
                Global.logger.Fatal(exc.ToString());
                _sdkTaskSource.SetResult(null);
            }
        }
        else
        {
            Global.logger.Warn("Currently installed razer sdk version \"{RzVersion}\" is not supported by the RazerSdkManager!",
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
        Global.razerSdkManager = _razerSdkManager;

        _razerSdkManager.DataUpdated += RzHelper.OnDataUpdated;

        RzHelper.Initialize();
    }


    [Async]
    public void Dispose()
    {
        try
        {
            _razerSdkManager.DataUpdated -= RzHelper.OnDataUpdated;
            _razerSdkManager?.Dispose();
            Global.razerSdkManager = null;
        }
        catch (Exception exc)
        {
            Global.logger.Fatal("RazerManager failed to dispose!");
            Global.logger.Fatal(exc.ToString());
        }
    }
}