using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using AuroraDeviceManager.Devices;
using AuroraDeviceManager.Utils;
using Common;
using Common.Devices;
using RGB.NET.Core;

namespace AuroraDeviceManager;

public class AuroraPipe
{
    public event EventHandler<EventArgs>? Shutdown;

    private readonly DeviceManager _deviceManager;
    private NamedPipeServerStream _dmPipe;

    public AuroraPipe(DeviceManager deviceManager)
    {
        _deviceManager = deviceManager;

        _dmPipe = CreatePipe();
    }

    private NamedPipeServerStream CreatePipe()
    {
        var securityIdentifier = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

        var pipeSecurity = new PipeSecurity();
        pipeSecurity.AddAccessRule(new PipeAccessRule(securityIdentifier,
            PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance | PipeAccessRights.FullControl,
            AccessControlType.Allow));

        var pipe = NamedPipeServerStreamAcl.Create(
            Constants.DeviceManagerPipe, PipeDirection.In,
            NamedPipeServerStream.MaxAllowedServerInstances,
            PipeTransmissionMode.Message, PipeOptions.Asynchronous, 5 * 1024, 5 * 1024, pipeSecurity);
        
        Task.Run(() => { pipe.BeginWaitForConnection(ReceiveCommand, null); });
        
        return pipe;
    }

    async void ReceiveCommand(IAsyncResult ar)
    {
        Global.Logger.Information("Pipe connection established");

        var pipe = _dmPipe;

        _dmPipe = CreatePipe();

        using var sr = new StreamReader(pipe);
        while (await sr.ReadLineAsync() is { } command)
        {
            using var splits = command.Split(Constants.StringSplit).AsEnumerable().GetEnumerator();

            var word = splits.Next();
            Global.Logger.Information("Received word: {Word}", word);
            switch (word)
            {
                case DeviceCommands.Quit:
                    Global.Logger.Information("Received close command");
                    Shutdown?.Invoke(this, EventArgs.Empty);
                    return;
                case DeviceCommands.Enable:
                {
                    var deviceId = splits.Next();
                    await _deviceManager.Enable(deviceId);
                    break;
                }
                case DeviceCommands.Disable:
                {
                    var deviceId = splits.Next();
                    await _deviceManager.Disable(deviceId);
                    break;
                }
                case DeviceCommands.Blink:
                {
                    var deviceId = splits.Next();
                    var deviceLed = (LedId)int.Parse(splits.Next());
                    _deviceManager.BlinkDevice(deviceId, deviceLed);
                    break;
                }
                case DeviceCommands.Remap:
                {
                    var deviceId = splits.Next();
                    var deviceLed = (LedId)int.Parse(splits.Next());
                    var remappedKey = (DeviceKeys)int.Parse(splits.Next());
                    _deviceManager.RemapKey(deviceId, deviceLed, remappedKey);
                    break;
                }
                case DeviceCommands.Unmap:
                {
                    var deviceId = splits.Next();
                    var deviceLed = (LedId)int.Parse(splits.Next());
                    _deviceManager.RemapKey(deviceId, deviceLed, null);
                    break;
                }
                case DeviceCommands.Share:
                {
                    await _deviceManager.ShareRemappableDevices();
                    break;
                }
                default:
                {
                    Global.Logger.Warning("Uknown command: {Command}", command);
                    break;
                }
            }
        }
    }
}