using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Aurora.Settings;
using Aurora.Utils;
using Lombok.NET;
using System.Windows.Forms;
using Aurora.Modules.Inputs;

namespace Aurora.Modules;

public sealed partial class InputsModule : AuroraModule
{
    private static InputInterceptor? _inputInterceptor;

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
        Global.InputEvents?.Dispose();
        _inputInterceptor?.Dispose();
    }

    private static void SetupVolumeAsBrightness(object sender, PropertyChangedEventArgs eventArgs)
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

    private static void InterceptVolumeAsBrightness(object sender, InputInterceptor.InputEventData e)
    {
        var keys = (Keys)e.Data.VirtualKeyCode;
        if ((!keys.Equals(Keys.VolumeDown) && !keys.Equals(Keys.VolumeUp)) || !Global.InputEvents.Alt) return;
        e.Intercepted = true;
        Task.Factory.StartNew(() =>
            {
                if (!e.KeyDown) return;
                float brightness = Global.Configuration.GlobalBrightness;
                brightness += keys == Keys.VolumeUp ? 0.05f : -0.05f;
                Global.Configuration.GlobalBrightness = Math.Max(0f, Math.Min(1f, brightness));

                ConfigManager.Save(Global.Configuration);
            }
        );
    }
}