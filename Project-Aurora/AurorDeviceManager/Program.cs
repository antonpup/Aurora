using System.Diagnostics;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using AurorDeviceManager;
using Common.Devices;
using AurorDeviceManager.Devices;
using AurorDeviceManager.Utils;
using Common;
using RGB.NET.Core;
using Color = System.Drawing.Color;

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

//TODO gotta make multi-connection?
void ReceiveCommand(IAsyncResult ar)
{
    Global.Logger.Information("Pipe connection established");

    using var sr = new StreamReader(dmPipe);
    while (sr.ReadLine() is { } command)
    {
        using var splits = command.Split(Constants.StringSplit).AsEnumerable().GetEnumerator();

        var word = splits.Next();
        Global.Logger.Information("Received word: {Word}", word);
        switch (word)
        {
            case "stop":
                Global.Logger.Information("Received close command");
                Stop();
                return;
            case "blink":
            {
                var deviceId = splits.Next();
                var deviceLed = (LedId)int.Parse(splits.Next());
                Global.DeviceManager.BlinkDevice(deviceId, deviceLed);
                break;
            }
            case "remap":
            {
                var deviceId = splits.Next();
                var deviceLed = (LedId)int.Parse(splits.Next());
                var remappedKey = (DeviceKeys)int.Parse(splits.Next());
                Global.DeviceManager.RemapKey(deviceId, deviceLed, remappedKey);
                break;
            }
            case "demap":
            {
                var deviceId = splits.Next();
                var deviceLed = (LedId)int.Parse(splits.Next());
                Global.DeviceManager.RemapKey(deviceId, deviceLed, null);
                break;
            }
            case "share":
            {
                Global.DeviceManager.ShareRemappableDevices().Wait();
                //TODO send IPC command?
                break;
            }
            default:
            {
                Global.Logger.Warning("Uknown command: {Command}", command);
                break;
            }
        }
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
