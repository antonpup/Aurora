using System.Diagnostics;
using System.Drawing;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using AurorDeviceManager;
using Common.Devices;
using AurorDeviceManager.Devices;
using Common;

Global.Initialize();

AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
{
    Global.Logger.Fatal((Exception)eventArgs.ExceptionObject, "Device Manager crashed");
};

Global.Logger.Information("Loading AurorDeviceManager");
Global.DeviceManager = new DeviceManager();

//Load config
Global.Logger.Information("Loading Configuration");
await ConfigManager.Load();

var securityIdentifier = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

var pipeSecurity = new PipeSecurity();
pipeSecurity.AddAccessRule(new PipeAccessRule(securityIdentifier,
    PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance | PipeAccessRights.FullControl,
    AccessControlType.Allow));
var dmPipe = NamedPipeServerStreamAcl.Create(
    Constants.DeviceManagerPipe, PipeDirection.In,
    NamedPipeServerStream.MaxAllowedServerInstances,
    PipeTransmissionMode.Message, PipeOptions.Asynchronous, 5 * 1024, 5 * 1024, pipeSecurity);

var colors = new MemorySharedArray<SimpleColor>(Constants.DeviceLedMap);

var endTaskSource = new TaskCompletionSource();

await Task.Run(() =>
{
    dmPipe.BeginWaitForConnection(ReceiveCommand, null);
});

var deviceKeys = new Dictionary<DeviceKeys, Color>();

var updateStopwatch = Stopwatch.StartNew();
colors.Updated += (_, _) =>
{
    for (var i = 0; i < colors.Count; i++)
    {
        var color = colors.ReadElement(i);
        deviceKeys[(DeviceKeys)i] = (Color)color;
    }
    Global.DeviceManager.UpdateDevices(deviceKeys);
    updateStopwatch.Restart();
};

await endTaskSource.Task;

Global.Logger.Information("Closing DeviceManager");
Stop();
return;

void ReceiveCommand(IAsyncResult ar)
{
    Global.Logger.Information("Pipe connection established");

    using var sr = new StreamReader(dmPipe);
    while (sr.ReadLine() is { } command)
    {
        Global.Logger.Information("Received close command");
        Stop();
        return;
    }

    dmPipe = NamedPipeServerStreamAcl.Create(
        Constants.DeviceManagerPipe, PipeDirection.In,
        NamedPipeServerStream.MaxAllowedServerInstances,
        PipeTransmissionMode.Message, PipeOptions.Asynchronous, 5 * 1024, 5 * 1024, pipeSecurity);
    dmPipe.BeginWaitForConnection(ReceiveCommand, null);
}

void Stop()
{
    colors.Dispose();
    Global.DeviceManager.ShutdownDevices().Wait(5000);
    Global.DeviceManager.Dispose();

    endTaskSource.TrySetResult();
}
