using System;
using Aurora.Settings;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

namespace Aurora.Devices;

public abstract class DefaultDevice : IDevice, IDisposable
{
    protected readonly Stopwatch Watch = new();
    private long _lastUpdateTime;
    private long _updateTime;

    public abstract string DeviceName { get; }

    protected virtual string DeviceInfo => "";

    public virtual string DeviceDetails => IsInitialized
        ? $"Initialized{(string.IsNullOrWhiteSpace(DeviceInfo) ? "" : ": " + DeviceInfo)}"
        : "Not Initialized";

    public string DeviceUpdatePerformance => IsInitialized
        ? _lastUpdateTime + "(" + _updateTime + ")" + " ms"
        : "";

    public Task InitializeTask
    {
        get => _initializeTask;
        set
        {
            _initializeTask?.Wait();
            _initializeTask = value;
        }
    }

    public virtual bool IsInitialized { get; protected set; }

    public abstract bool Initialize();
    public abstract void Shutdown();

    public virtual void Reset()
    {
        Shutdown();
        Initialize();
    }

    protected abstract bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false);

    private readonly Stopwatch _tempStopWatch = new();
    public void UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
    {
        _tempStopWatch.Restart();
        var updateResult = UpdateDevice(colorComposition.KeyColors, e, forced);

        if (!updateResult) return;
        _lastUpdateTime = Watch.ElapsedMilliseconds;
        _updateTime = _tempStopWatch.ElapsedMilliseconds;
        Watch.Restart();
    }

    public virtual IEnumerable<string> GetDevices()
    {
        yield break;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _initializeTask?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #region Variables
    private VariableRegistry _variableRegistry;
    private Task _initializeTask;

    public VariableRegistry RegisteredVariables
    {
        get
        {
            if (_variableRegistry is not null) return _variableRegistry;
            _variableRegistry = new VariableRegistry();
            RegisterVariables(_variableRegistry);
            return _variableRegistry;
        }
    }
    protected virtual void RegisterVariables(VariableRegistry variableRegistry) { }

    protected void LogInfo(string s) => Global.logger.Info($"[Device][{DeviceName}] {s}");

    protected void LogError(string s) => Global.logger.Error($"[Device][{DeviceName}] {s}");
    #endregion
}