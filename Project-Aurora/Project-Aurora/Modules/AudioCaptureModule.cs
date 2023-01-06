using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using Aurora.Modules.AudioCapture;
using Aurora.Settings;
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
        Thread thread = new Thread(InitializeLocalInfoProxies);
        thread.SetApartmentState(ApartmentState.MTA);
        thread.Name = "3rd aprty API spooler";
        thread.Start();
        
        Application.Current.Dispatcher.InvokeAsync(InitializeDeviceListProxy).Wait();
        
        thread.Join();
        
        Global.Configuration.PropertyChanged += DefaultDeviceChanged;
    }

    private void DefaultDeviceChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Configuration.GsiAudioRenderDevice):
                _renderProxy.DeviceId = Global.Configuration.GsiAudioRenderDevice;
                break;
            case nameof(Configuration.GsiAudioCaptureDevice):
                _captureProxy.DeviceId = Global.Configuration.GsiAudioCaptureDevice;
                break;
        }
    }

    private void InitializeDeviceListProxy()
    {
        _audioDevices = new AudioDevices();
    }

    private void InitializeLocalInfoProxies()
    {
        try
        {
            _renderProxy = new AudioDeviceProxy(Global.Configuration.GsiAudioRenderDevice, DataFlow.Render);
            Global.RenderProxy = _renderProxy;
        }
        catch (Exception e)
        {
            MessageBox.Show("Audio device could not be loaded.\n" +
                            "Audio information such as output level won't be updated.\n" +
                            "Cause of this could be bad drivers or bad implementation", 
                "Aurora - Warning");
            Global.logger.Error("AudioCapture error", e);
        }
        if (!Global.Configuration.EnableAudioCapture) return;
        try
        {
            _captureProxy = new AudioDeviceProxy(Global.Configuration.GsiAudioCaptureDevice, DataFlow.Capture);
            Global.CaptureProxy = _captureProxy;
        }
        catch (Exception e)
        {
            MessageBox.Show("Input audio device could not be loaded.\n" +
                            "Audio capture information such as microphone level won't be updated.\n" +
                            "Cause of this could be bad drivers or bad implementation", 
                "Aurora - Warning");
            Global.logger.Error("AudioCapture error", e);
        }
    }

    [Async]
    public void Dispose()
    {
        Global.Configuration.PropertyChanged -= DefaultDeviceChanged;
        
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