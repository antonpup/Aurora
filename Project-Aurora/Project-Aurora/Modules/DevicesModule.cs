using System.Threading.Tasks;
using Aurora.Devices;
using RazerSdkReader;

namespace Aurora.Modules;

public sealed class DevicesModule : AuroraModule
{
    public Task<DeviceManager> DeviceManager => _taskSource.Task;

    private readonly Task<ChromaReader?> _rzSdkManager;
    private readonly TaskCompletionSource<DeviceManager> _taskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private DeviceManager? _deviceManager;

    public DevicesModule(Task<ChromaReader?> rzSdkManager)
    {
        _rzSdkManager = rzSdkManager;
    }

    protected override async Task Initialize()
    {
        Global.logger.Information("Loading Device Manager...");

        _deviceManager = new DeviceManager(_rzSdkManager);
        _taskSource.SetResult(_deviceManager);

        _deviceManager.RegisterVariables();
        await Task.Run(() =>
        {
            _deviceManager.InitializeDevices().ContinueWith(_ =>
                Global.logger.Information("Loaded Device Manager"));
        });
    }

    public override async Task DisposeAsync()
    {
        await _deviceManager?.ShutdownDevices();
        _deviceManager?.Dispose();
    }

    public override void Dispose()
    {
        _deviceManager?.ShutdownDevices();
        _deviceManager?.Dispose();
    }
}