using System;
using System.Threading.Tasks;
using System.Windows;
using Aurora.Modules.GameStateListen;
using Lombok.NET;

namespace Aurora.Modules;

public sealed partial class HttpListenerModule : IAuroraModule
{
    private readonly TaskCompletionSource<AuroraHttpListener?> _taskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private AuroraHttpListener? _listener;

    public Task<AuroraHttpListener?> HttpListener => _taskSource.Task;

    public override void Initialize()
    {
        if (!Global.Configuration.EnableHttpListener)
        {
            Global.logger.Info("HttpListener is disabled");
            _taskSource.SetResult(null);
            return;
        }
        try
        {
            _listener = new AuroraHttpListener(9088);
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "HttpListener Exception");
            MessageBox.Show("HttpListener Exception.\r\n" +
                            "Http socket could not be created. Games using this integration won't work" +
                            "\r\n" + exc);
            _taskSource.SetResult(null);
        }

        if (!_listener.Start())
        {
            Global.logger.Error("GameStateListener could not start");
            MessageBox.Show("HttpListener could not start. Try running this program as Administrator.\r\n" +
                            "Http socket could not be created. Games using this integration won't work");
            _taskSource.SetResult(null);
            return;
        }
        _taskSource.SetResult(_listener);
    }

    public override void Dispose()
    {
        _listener?.Stop().Wait();
    }

    public override async Task DisposeAsync()
    {
        await _listener?.Stop();
    }
}