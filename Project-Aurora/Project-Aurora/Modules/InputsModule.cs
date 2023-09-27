using System.ComponentModel;
using System.Threading.Tasks;
using Lombok.NET;
using Aurora.Modules.Inputs;
using Common.Utils;

namespace Aurora.Modules;

public sealed partial class InputsModule : AuroraModule
{
    public override async Task InitializeAsync()
    {
        await Initialize();
    }

    protected override Task Initialize()
    {
        if (Global.Configuration.EnableInputCapture)
        {
            Global.logger.Information("Loading Input Hooking");
            Global.InputEvents = new InputEvents();
            Global.Configuration.PropertyChanged += SetupVolumeAsBrightness;
            SetupVolumeAsBrightness(Global.Configuration,
                new PropertyChangedEventArgs(nameof(Global.Configuration.UseVolumeAsBrightness)));
            Global.logger.Information("Loaded Input Hooking");
        }

        DesktopUtils.StartSessionWatch();

        Global.key_recorder = new KeyRecorder(Global.InputEvents);
        return Task.CompletedTask;
    }

    [Async]
    public override void Dispose()
    {
        Global.key_recorder?.Dispose();
        Global.InputEvents.Dispose();
    }

    private static void SetupVolumeAsBrightness(object? sender, PropertyChangedEventArgs eventArgs)
    {
        //if (eventArgs.PropertyName != nameof(Global.Configuration.UseVolumeAsBrightness)) return;
        //if (Global.Configuration.UseVolumeAsBrightness)
        //{
        //    InputInterceptor = new InputInterceptor();
        //    InputInterceptor.Input += InterceptVolumeAsBrightness;
        //}
        //else if (InputInterceptor != null)
        //{
        //    InputInterceptor.Input -= InterceptVolumeAsBrightness;
        //    InputInterceptor.Dispose();
        //}
    }
}