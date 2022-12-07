using System;
using System.Collections.Concurrent;
using System.Linq;
using CSScripting;
using NAudio.CoreAudioApi;

namespace Aurora.Modules.AudioCapture;

public sealed class AudioDevices : IDisposable, NAudio.CoreAudioApi.Interfaces.IMMNotificationClient
{
    public const string DefaultDeviceId = ""; // special ID to indicate the default device

    private readonly MMDeviceEnumerator _deviceEnumerator = new();

    #region Init

    public AudioDevices()
    {
        _deviceEnumerator.RegisterEndpointNotificationCallback(this);
        RefreshDeviceLists();
    }

    // Refreshes both playback and recording devices lists.
    private void RefreshDeviceLists()
    {
        RefreshDeviceList(PlaybackDevices, DataFlow.Render);
        RefreshDeviceList(RecordingDevices, DataFlow.Capture);
    }

    // Updates the target list with the devices of the given dataflow type.
    private void RefreshDeviceList(ObservableConcurrentDictionary<string, string> target, DataFlow flow)
    {
        // Note: clear the target then repopulate it to make it easier for data binding. If we re-created this, we could not use {x:Static}.
        target.Keys.ForEach(key => target.Remove(key));
        target.Add(DefaultDeviceId, "Default"); // Add default device to to the top of the list
        foreach (var device in _deviceEnumerator.EnumerateAudioEndPoints(flow, DeviceState.Active)
                     .OrderBy(d => d.DeviceFriendlyName))
            target.Add(device.ID, device.FriendlyName);
    }

    #endregion

    #region Device Enumeration

    public static ObservableConcurrentDictionary<string, string> PlaybackDevices { get; } = new();
    public static ObservableConcurrentDictionary<string, string> RecordingDevices { get; } = new();

    public void OnDeviceAdded(string pwstrDeviceId)
    {
        var device = _deviceEnumerator.GetDevice(pwstrDeviceId);
        switch (device.DataFlow)
        {
            case DataFlow.Render:
                AddPlaybackDevice(device);
                break;
            case DataFlow.Capture:
                AddRecordingDevice(device);
                break;
        }
    }

    public void OnDeviceRemoved(string deviceId)
    {
        var device = _deviceEnumerator.GetDevice(deviceId);
        switch (device.DataFlow)
        {
            case DataFlow.Render:
                RemovePlaybackDevice(device);
                break;
            case DataFlow.Capture:
                RemoveRecordingDevice(device);
                break;
        }
    }

    public void OnDeviceStateChanged(string deviceId, DeviceState newState)
    {
        var mmDevice = _deviceEnumerator.GetDevice(deviceId);
        switch (newState)
        {
            case DeviceState.Active:
                switch (mmDevice.DataFlow)
                {
                    case DataFlow.Render:
                        AddPlaybackDevice(mmDevice);
                        break;
                    case DataFlow.Capture:
                        AddRecordingDevice(mmDevice);
                        break;
                }
                break;
            case DeviceState.Disabled:
            case DeviceState.Unplugged:
            case DeviceState.NotPresent:
                switch (mmDevice.DataFlow)
                {
                    case DataFlow.Render:
                        RemovePlaybackDevice(mmDevice);
                        break;
                    case DataFlow.Capture:
                        RemoveRecordingDevice(mmDevice);
                        break;
                }
                break;
        }
    }

    #endregion

    #region unused

    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
    {
        //unused
    }

    public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
    {
        //unused
    }

    #endregion

    #region List Manipulation

    private void AddPlaybackDevice(MMDevice device)
    {
        PlaybackDevices.Add(device.ID, device.FriendlyName);
    }

    private void RemovePlaybackDevice(MMDevice device)
    {
        PlaybackDevices.Remove(device.ID);
    }

    private void AddRecordingDevice(MMDevice device)
    {
        RecordingDevices.Add(device.ID, device.FriendlyName);
    }

    private void RemoveRecordingDevice(MMDevice device)
    {
        RecordingDevices.Remove(device.ID);
    }

    #endregion

    #region IDisposable Implementation

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        _deviceEnumerator.UnregisterEndpointNotificationCallback(this);
        _disposed = true;
    }

    #endregion
}