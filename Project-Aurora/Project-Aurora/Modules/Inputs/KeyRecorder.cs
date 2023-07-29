using System;
using System.Collections.Generic;
using Aurora.Devices;
using Aurora.Utils;

namespace Aurora.Modules.Inputs;

public sealed class KeyRecorder : IDisposable
{
    private readonly IInputEvents _inputEvents;
    private string _recordingType = "";
    private bool _isSingleKey;
    private List<DeviceKeys> _recordedKeys = new();
    public delegate void RecordingFinishedHandler(DeviceKeys[] resultingKeys);
    public event RecordingFinishedHandler? FinishedRecording;

    public KeyRecorder(IInputEvents inputEvents)
    {
        _inputEvents = inputEvents;
        Reset();

        inputEvents.KeyUp += InputEventsOnKeyUp;
    }
        
    private void InputEventsOnKeyUp(object? sender, KeyEvent e)
    {
        if (!IsRecording()) return;
        var key = e.GetDeviceKey();

        if (key == DeviceKeys.NONE) return;
        if (HasRecorded(key))
            RemoveKey(key);
        else
            AddKey(key);
    }

    public void AddKey(DeviceKeys key)
    {
        if (!IsRecording())
            return;

        if (HasRecorded(key)) return;
        _recordedKeys.Add(key);

        if(_isSingleKey)
        {
            StopRecording();
        }
    }

    public void RemoveKey(DeviceKeys key)
    {
        if (!IsRecording())
            return;

        if (HasRecorded(key))
        {
            _recordedKeys.Remove(key);
        }
    }

    public bool HasRecorded(DeviceKeys key)
    {
        return _recordedKeys.Contains(key);
    }

    public DeviceKeys[] GetKeys()
    {
        return _recordedKeys.ToArray();
    }

    public void StartRecording(string type, bool isSingleKey = false)
    {
        Reset();

        _recordingType = type;
        _isSingleKey = isSingleKey;
    }

    public void StopRecording()
    {
        _recordingType = "";
        _isSingleKey = false;

        FinishedRecording?.Invoke(GetKeys());
    }

    public bool IsRecording(string type = "")
    {
        if(string.IsNullOrWhiteSpace(type))
        {
            return !string.IsNullOrWhiteSpace(_recordingType);
        }

        return _recordingType.Equals(type);
    }

    public string GetRecordingType()
    {
        return _recordingType;
    }

    public void Reset()
    {
        _recordingType = "";
        _isSingleKey = false;
        _recordedKeys = new List<DeviceKeys>();
    }

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _inputEvents.KeyUp -= InputEventsOnKeyUp;
    }
}