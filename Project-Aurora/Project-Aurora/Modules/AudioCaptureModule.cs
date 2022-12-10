using System.Threading;
using System.Windows;
using Aurora.Modules.AudioCapture;
using Lombok.NET;
using NAudio.CoreAudioApi;

namespace Aurora.Modules;

public sealed partial class AudioCaptureModule : IAuroraModule
{
    private AudioDevices _audioDevices;
    private AudioDeviceProxy _renderProxy;
    private AudioDeviceProxy _captureProxy;

    [Async]
    public void Initialize()
    {
        Thread thread = new Thread(F);
        thread.SetApartmentState(ApartmentState.MTA);
        thread.Name = "3rd aprty API spooler";
        thread.Start();
        
        Application.Current.Dispatcher.InvokeAsync(Run).Wait();
        
        thread.Join();
    }

    private void Run()
    {
        _audioDevices = new AudioDevices();
    }

    private void F()
    {
        _renderProxy = new AudioDeviceProxy(Global.Configuration.GsiAudioRenderDevice, DataFlow.Render);
        Global.RenderProxy = _renderProxy;
        if (Global.Configuration.EnableAudioCapture)
        {
            _captureProxy = new AudioDeviceProxy(Global.Configuration.GsiAudioCaptureDevice, DataFlow.Capture);
            Global.CaptureProxy = _captureProxy;
        }
    }

    [Async]
    public void Dispose()
    {
        _renderProxy?.Dispose();
        _renderProxy = null;
        Global.RenderProxy = null;
        _captureProxy?.Dispose();
        _captureProxy = null;
        Global.CaptureProxy = null;
        _audioDevices?.Dispose();
        _audioDevices = null;
        
        foreach (var audioDeviceProxy in AudioDeviceProxy.Instances)
        {
            audioDeviceProxy.Dispose();
        }
        AudioDeviceProxy.Instances.Clear();
    }
}