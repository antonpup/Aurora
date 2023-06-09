using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using Aurora.Settings;

namespace Aurora.Devices;

public abstract class DefaultDevice : IDevice, IDisposable
{
    private readonly Stopwatch _updateWatch = new();
    protected readonly Stopwatch Watch = new();
    private long _lastUpdateTime;
    private long _updateTime;

    public abstract string DeviceName { get; }

    protected virtual string DeviceInfo => "";

    public virtual string DeviceDetails => DeviceInfo;

    public string DeviceUpdatePerformance => IsInitialized
        ? _lastUpdateTime + "(" + _updateTime + ")" + " ms"
        : "";

    public virtual bool isDoingWork { get; protected set; }

    public virtual bool IsInitialized { get; protected set; }

    public async Task<bool> Initialize() {
        isDoingWork = true;
        try
        {
            IsInitialized = await DoInitialize();
        }
        finally
        {
            isDoingWork = false;
        }

        return IsInitialized;
    }

    public async Task ShutdownDevice()
    {
        try
        {
            isDoingWork = true;
            if (IsInitialized)
            {
                await Shutdown();
            }
        }
        finally
        {
            isDoingWork = false;
        }
    }

    protected abstract Task Shutdown();

    public virtual async Task Reset()
    {
        await Shutdown()
            .ContinueWith(_ => Initialize())
            .ConfigureAwait(false);
    }

    public async Task<bool> UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
    {
        if (isDoingWork)
        {
            return false;
        }
        _updateWatch.Restart();
        var updateResult = await UpdateDevice(colorComposition.KeyColors, e, forced);

        if (!updateResult) return false;
        _lastUpdateTime = Watch.ElapsedMilliseconds;
        _updateTime = _updateWatch.ElapsedMilliseconds;
        Watch.Restart();

        return true;
    }

    public virtual IEnumerable<string> GetDevices()
    {
        yield break;
    }

    protected abstract Task<bool> DoInitialize();

    protected abstract Task<bool> UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false);

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #region Variables
    private VariableRegistry? _variableRegistry;

    public VariableRegistry RegisteredVariables
    {
        get
        {
            if (_variableRegistry is not null)
            {
                return _variableRegistry;
            }

            _variableRegistry = new VariableRegistry();
            RegisterVariables(_variableRegistry);
            return _variableRegistry;
        }
    }

    protected virtual void RegisterVariables(VariableRegistry variableRegistry) { }

    protected void LogInfo(string s) => Global.logger.Info($"[Device][{DeviceName}] {s}");

    protected void LogError(string s) => Global.logger.Error($"[Device][{DeviceName}] {s}");

    protected void LogError(string s, Exception e) => Global.logger.Error(e, $"[Device][{DeviceName}] {s}");
    #endregion
}