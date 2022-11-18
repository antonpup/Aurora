using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Aurora.Settings;
using Aurora.Utils;
using Lombok.NET;
using System.Windows.Forms;

namespace Aurora.Modules;

public partial class InputsModule : IAuroraModule
{
    private static InputInterceptor InputInterceptor;
    
    [Async]
    public void Initialize()
    {
        Global.logger.Info("Loading Input Hooking");
        Global.InputEvents = new InputEvents();
        Global.Configuration.PropertyChanged += SetupVolumeAsBrightness;
        SetupVolumeAsBrightness(Global.Configuration,
            new PropertyChangedEventArgs(nameof(Global.Configuration.UseVolumeAsBrightness)));
        DesktopUtils.StartSessionWatch();

        Global.key_recorder = new KeyRecorder(Global.InputEvents);
        Global.logger.Info("Loaded Input Hooking");
    }


    [Async]
    public void Dispose()
    {
        Global.key_recorder?.Dispose();
        Global.InputEvents?.Dispose();
        InputInterceptor?.Dispose();
    }

    private static void SetupVolumeAsBrightness(object sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName == nameof(Global.Configuration.UseVolumeAsBrightness))
        {
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
    }

    private static void InterceptVolumeAsBrightness(object sender, InputInterceptor.InputEventData e)
    {
        var keys = (Keys)e.Data.VirtualKeyCode;
        if ((keys.Equals(Keys.VolumeDown) || keys.Equals(Keys.VolumeUp))
            && Global.InputEvents.Alt)
        {
            e.Intercepted = true;
            Task.Factory.StartNew(() =>
                {
                    if (e.KeyDown)
                    {
                        float brightness = Global.Configuration.GlobalBrightness;
                        brightness += keys == Keys.VolumeUp ? 0.05f : -0.05f;
                        Global.Configuration.GlobalBrightness = Math.Max(0f, Math.Min(1f, brightness));

                        ConfigManager.Save(Global.Configuration);
                    }
                }
            );
        }
    }
}