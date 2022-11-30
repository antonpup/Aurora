using System;
using System.Threading.Tasks;
using Lombok.NET;
using RazerSdkHelper;
using RazerSdkWrapper;
using RazerSdkWrapper.Data;

namespace Aurora.Modules;

public sealed partial class RazerSdkModule : IAuroraModule
{
    private readonly TaskCompletionSource<RzSdkManager> _sdkTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private RzSdkManager _razerSdkManager;

    public Task<RzSdkManager> RzSdkManager => _sdkTaskSource.Task;

    [Async]
    public void Initialize()
    {
        Global.logger.Info("Loading RazerSdkManager");
        if (RzHelper.IsSdkVersionSupported(RzHelper.GetSdkVersion()))
        {
            try
            {
                _razerSdkManager = new RzSdkManager
                {
                    KeyboardEnabled = true,
                    MouseEnabled = true,
                    MousepadEnabled = true,
                    AppListEnabled = true,
                };
                Global.razerSdkManager = _razerSdkManager;

                _razerSdkManager.DataUpdated += RzHelper.OnDataUpdated;

                var appList = _razerSdkManager.GetDataProvider<RzAppListDataProvider>();
                appList.Update();
                RzHelper.CurrentAppExecutable = appList.CurrentAppExecutable;

                Global.logger.Info("RazerSdkManager loaded successfully!");
                _sdkTaskSource.SetResult(_razerSdkManager);
            }
            catch (Exception exc)
            {
                Global.logger.Fatal("RazerSdkManager failed to load!");
                Global.logger.Fatal(exc.ToString());
            }
        }
        else
        {
            Global.logger.Warn("Currently installed razer sdk version \"{0}\" is not supported by the RazerSdkManager!", RzHelper.GetSdkVersion());
        }
    }


    [Async]
    public void Dispose()
    {
        try
        {
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