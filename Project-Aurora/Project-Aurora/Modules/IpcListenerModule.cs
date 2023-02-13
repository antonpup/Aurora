using System;
using System.Threading.Tasks;
using System.Windows;
using Aurora.Modules.GameStateListen;
using JetBrains.Annotations;
using Lombok.NET;

namespace Aurora.Modules;

public sealed partial class IpcListenerModule : IAuroraModule
{
    private readonly TaskCompletionSource<IpcListener?> _taskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private IpcListener? _listener;

    public Task<IpcListener?> IpcListener => _taskSource.Task;

    [Async]
    public void Initialize()
    {
        if (!Global.Configuration.EnableIpcListener)
        {
            Global.logger.Info("IpcListener is disabled");
            _taskSource.SetResult(null);
            return;
        }
        Global.logger.Info("Starting IpcListener");
        try
        {
            _listener = new IpcListener();
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "IpcListener Exception");
            MessageBox.Show("IpcListener Exception.\r\n" +
                            "Ipc pipe could not be created. Wrapper integrations won't work." +
                            "\r\n" + exc);
            _taskSource.SetResult(null);
            return;
        }

        if (!_listener.Start())
        {
            Global.logger.Error("IpcListener could not start");
            MessageBox.Show("IpcListener could not start. Try running this program as Administrator.\r\n" +
                            "Wrapper integrations won't work.");
            _taskSource.SetResult(null);
            return;
        }

        Global.logger.Info("Listening for wrapper integration calls...");
        _taskSource.SetResult(_listener);
    }

    public void Dispose()
    {
        _listener?.Stop().Wait();
    }

    public Task DisposeAsync()
    {
        return _listener?.Stop();
    }
}