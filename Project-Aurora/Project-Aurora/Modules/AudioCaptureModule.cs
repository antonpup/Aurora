using Aurora.Modules.AudioCapture;
using Aurora.Utils;
using Lombok.NET;
using NAudio.CoreAudioApi;

namespace Aurora.Modules;

public sealed partial class AudioCaptureModule : IAuroraModule
{

    private AudioDevices _audioDevices;
    
    [Async]
    public void Initialize()
    {
        _audioDevices = new AudioDevices();
        Global.RenderProxy = new AudioDeviceProxy(Global.Configuration.GsiAudioRenderDevice, DataFlow.Render);
        if (Global.Configuration.EnableAudioCapture)
            Global.CaptureProxy = new AudioDeviceProxy(Global.Configuration.GsiAudioCaptureDevice, DataFlow.Capture);
    }


    [Async]
    public void Dispose()
    {
        Global.RenderProxy = null;
        Global.CaptureProxy = null;
        _audioDevices?.Dispose();
        _audioDevices = null;
    }
}