using System;
using Lombok.NET;
using RazerSdkHelper;
using RazerSdkWrapper;
using RazerSdkWrapper.Data;

namespace Aurora.Modules;

public partial class RazerSdkModule : IAuroraModule
{
    [Async]
    public void Initialize()
    {
        Global.logger.Info("Loading RazerSdkManager");
        if (RzHelper.IsSdkVersionSupported(RzHelper.GetSdkVersion()))
        {
            try
            {
                Global.razerSdkManager = new RzSdkManager
                {
                    KeyboardEnabled = true,
                    MouseEnabled = true,
                    MousepadEnabled = true,
                    AppListEnabled = true,
                };

                Global.razerSdkManager.DataUpdated += RzHelper.OnDataUpdated;

                var appList = Global.razerSdkManager.GetDataProvider<RzAppListDataProvider>();
                appList.Update();
                RzHelper.CurrentAppExecutable = appList.CurrentAppExecutable;

                Global.logger.Info("RazerSdkManager loaded successfully!");
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
            Global.razerSdkManager?.Dispose();
            Global.razerSdkManager = null;
        }
        catch (Exception exc)
        {
            Global.logger.Fatal("RazerManager failed to dispose!");
            Global.logger.Fatal(exc.ToString());
        }
    }
}