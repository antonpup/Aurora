using System;
using System.Threading.Tasks;
using System.Windows;
using Aurora.Modules.AudioCapture;
using Aurora.Settings;
using Lombok.NET;
using NAudio.CoreAudioApi;

namespace Aurora.Modules;

public sealed partial class AudioCaptureModule : AuroraModule
{
    private AudioDevices? _audioDevices;
    private AudioDeviceProxy? _renderProxy;
    private AudioDeviceProxy? _captureProxy;

    public override Task<bool> InitializeAsync()
    {
        Initialize();
        return Task.FromResult(true);
    }

    protected override Task Initialize()
    {
        InitializeLocalInfoProxies();
        return Task.CompletedTask;
    }

    private void InitializeDeviceListProxy()
    {
        _audioDevices = new AudioDevices();
    }

    private void InitializeLocalInfoProxies()
    {
        InitializeDeviceListProxy();
        try
        {
            InitRender();
        }
        catch (Exception e)
        {
            MessageBox.Show("Audio device could not be loaded.\n" +
                            "Audio information such as output level won't be updated.\n" +
                            "Cause of this could be bad drivers or bad implementation", 
                "Aurora - Warning");
            Global.logger.Error(e, "AudioCapture error");
        }
        if (!Global.Configuration.EnableAudioCapture) return;
        try
        {
            InitCapture();
        }
        catch (Exception e)
        {
            MessageBox.Show("Input audio device could not be loaded.\n" +
                            "Audio capture information such as microphone level won't be updated.\n" +
                            "Cause of this could be bad drivers or bad implementation", 
                "Aurora - Warning");
            Global.logger.Error(e, "AudioCapture error");
        }
    }

    private void InitRender()
    {
        _renderProxy = new AudioDeviceProxy(Global.Configuration.GsiAudioRenderDevice, DataFlow.Render);
        Global.Configuration.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Configuration.GsiAudioRenderDevice))
                _renderProxy.DeviceId = Global.Configuration.GsiAudioRenderDevice;
        };
        Global.RenderProxy = _renderProxy;
    }

    private void InitCapture()
    {
        _captureProxy = new AudioDeviceProxy(Global.Configuration.GsiAudioCaptureDevice, DataFlow.Capture);
        Global.Configuration.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Configuration.GsiAudioCaptureDevice))
                _captureProxy.DeviceId = Global.Configuration.GsiAudioCaptureDevice;
        };
        Global.CaptureProxy = _captureProxy;
    }

    [Async]
    public override void Dispose()
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