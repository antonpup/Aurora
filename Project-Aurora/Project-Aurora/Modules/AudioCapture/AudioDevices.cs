using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        PlaybackDevices.Remove(deviceId);
        RecordingDevices.Remove(deviceId);
    }

    public void OnDeviceStateChanged(string deviceId, DeviceState newState)
    {
        switch (newState)
        {
            case DeviceState.Active:
                var addedDevice = _deviceEnumerator.GetDevice(deviceId);
                switch (addedDevice.DataFlow)
                {
                    case DataFlow.Render:
                        AddPlaybackDevice(addedDevice);
                        break;
                    case DataFlow.Capture:
                        AddRecordingDevice(addedDevice);
                        break;
                }
                break;
            case DeviceState.Disabled:
            case DeviceState.Unplugged:
                var removedDevice = _deviceEnumerator.GetDevice(deviceId);
                switch (removedDevice.DataFlow)
                {
                    case DataFlow.Render:
                        RemovePlaybackDevice(removedDevice);
                        break;
                    case DataFlow.Capture:
                        RemoveRecordingDevice(removedDevice);
                        break;
                }
                break;
            case DeviceState.NotPresent:
                RecordingDevices.Remove(deviceId);
                PlaybackDevices.Remove(deviceId);
                break;
        }
    }

    #endregion

    #region unused

    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string? defaultDeviceId)
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
        PlaybackDevices.TryAdd(device.ID, device.FriendlyName);
    }

    private void RemovePlaybackDevice(MMDevice device)
    {
        PlaybackDevices.Remove(device.ID);
    }

    private void AddRecordingDevice(MMDevice device)
    {
        RecordingDevices.TryAdd(device.ID, device.FriendlyName);
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
        _deviceEnumerator.Dispose();
        _disposed = true;
    }

    #endregion
}