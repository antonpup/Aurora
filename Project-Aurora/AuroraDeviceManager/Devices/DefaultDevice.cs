using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using Common.Devices;

namespace AuroraDeviceManager.Devices;

public abstract class DefaultDevice : IDevice, IDisposable
{
    protected readonly Stopwatch Watch = Stopwatch.StartNew();

    private readonly Stopwatch _updateWatch = Stopwatch.StartNew();
    private long _lastUpdateTime;
    private long _updateTime;

    public abstract string DeviceName { get; }

    protected virtual string DeviceInfo => "";

    public virtual string DeviceDetails => DeviceInfo;

    public string DeviceUpdatePerformance => IsInitialized
        ? _lastUpdateTime + "(" + _updateTime + ")" + " ms"
        : "";

    public virtual bool IsDoingWork { get; protected set; }

    public virtual bool IsInitialized { get; protected set; }

    public async Task<bool> Initialize() {
        if (IsInitialized || IsDoingWork)
        {
            Global.Logger.Warning("Device already initialized or initializing");
            return IsInitialized;
        }
        IsDoingWork = true;
        try
        {
            IsInitialized = await DoInitialize();
        }
        finally
        {
            IsDoingWork = false;
        }

        return IsInitialized;
    }

    public async Task ShutdownDevice()
    {
        try
        {
            IsDoingWork = true;
            if (IsInitialized)
            {
                await Shutdown();
            }
        }
        finally
        {
            IsDoingWork = false;
        }
    }

    protected abstract Task Shutdown();

    public virtual async Task Reset()
    {
        await Shutdown()
            .ContinueWith(_ => Initialize())
            .ConfigureAwait(false);
    }

    public Task<bool> UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
    {
        if (IsDoingWork)
        {
            return Task.FromResult(false);
        }
        _updateWatch.Restart();
        var updateResult = UpdateDevice(colorComposition.KeyColors, e, forced);

        if (!updateResult.Result) return Task.FromResult(false);
        _lastUpdateTime = Watch.ElapsedMilliseconds;
        _updateTime = _updateWatch.ElapsedMilliseconds;
        Watch.Restart();

        return Task.FromResult(true);
    }

    public virtual string? GetDevices()
    {
        return null;
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

    protected void LogInfo(string s) => Global.Logger.Information("[Device][{DeviceName}] {S}", DeviceName, s);

    protected void LogError(string s) => Global.Logger.Error("[Device][{DeviceName}] {S}", DeviceName, s);

    protected void LogError(string s, Exception e) => Global.Logger.Error(e, "[Device][{DeviceName}] {S}", DeviceName, s);
    #endregion
}