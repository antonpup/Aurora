using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Settings;
using Aurora.Utils;
using Lombok.NET;
using System.Windows.Forms;
using Aurora.Modules.Inputs;

namespace Aurora.Modules;

public sealed partial class InputsModule : IAuroraModule
{
    private static TaskCompletionSource<IInputEvents> tcs = new();
    private static Lazy<Task<IInputEvents>> l = new(() => tcs.Task, LazyThreadSafetyMode.ExecutionAndPublication);
    public static Task<IInputEvents> Instance => l.Value;

    private static InputInterceptor? InputInterceptor;

    public override void Initialize()
    {
        if (!Global.Configuration.EnableInputCapture)
        {
            tcs.SetResult(new NoopInputEvents());
        }
        else
        {
            Global.logger.Info("Loading Input Hooking");
            tcs.SetResult(new InputEvents());
            Global.Configuration.PropertyChanged += SetupVolumeAsBrightness;
            SetupVolumeAsBrightness(Global.Configuration,
                new PropertyChangedEventArgs(nameof(Global.Configuration.UseVolumeAsBrightness)));
            Global.logger.Info("Loaded Input Hooking");
        }

        DesktopUtils.StartSessionWatch();

        Global.key_recorder = new KeyRecorder(Global.InputEvents);
    }

    [Async]
    public override void Dispose()
    {
        Global.key_recorder?.Dispose();
        Global.InputEvents?.Dispose();
        InputInterceptor?.Dispose();
    }

    private static void SetupVolumeAsBrightness(object sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName != nameof(Global.Configuration.UseVolumeAsBrightness)) return;
        if (Global.Configuration.UseVolumeAsBrightness)
        {
            InputInterceptor = new InputInterceptor();
            InputInterceptor.Input += InterceptVolumeAsBrightness;
        }
        else if (InputInterceptor != null)
        {
            InputInterceptor.Input -= InterceptVolumeAsBrightness;
            InputInterceptor.Dispose();
        }
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