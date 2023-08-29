using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Aurora.Profiles;

namespace Aurora.Modules.GameStateListen;

public class AuroraHttpListener
{
    private bool _isRunning;
    private IGameState _currentGameState = new EmptyGameState("{}");
    private readonly HttpListener _netListener;
    private readonly int _port;

    public IGameState CurrentGameState
    {
        get => _currentGameState;
        private set
        {
            _currentGameState = value;
            NewGameState?.Invoke(this, CurrentGameState);
        }
    }

    /// <summary>
    ///  Event for handing a newly received game state
    /// </summary>
    public event EventHandler<IGameState>? NewGameState;

    /// <summary>
    /// A GameStateListener that listens for connections on http://127.0.0.1:port/
    /// </summary>
    /// <param name="port"></param>
    public AuroraHttpListener(int port)
    {
        _port = port;
        _netListener = new HttpListener();
        _netListener.Prefixes.Add($"http://127.0.0.1:{port}/");
    }

    /// <summary>
    /// Starts listening for GameState requests
    /// </summary>
    public bool Start()
    {
        if (_isRunning) return false;

        try
        {
            _netListener.Start();
        }
        catch (HttpListenerException exc)
        {
            Global.logger.Error(exc, "Could not start HttpListener");
            
            if (exc.ErrorCode == 5)//Access Denied
                MessageBox.Show("Access error during start of network listener.\r\n\r\n" +
                                "To fix this issue, please run the following commands as admin in Command Prompt:r\n" +
                                $"   netsh http add urlacl url=http://127.0.0.1:{_port}/ user=Everyone listen=yes", 
                    "Aurora - Error");

            return false;
        }
        _isRunning = true;

        _netListener.BeginGetContext(ReceiveGameState, null);
        return true;
    }

    /// <summary>
    /// Stops listening for GameState requests
    /// </summary>
    public Task Stop()
    {
        _isRunning = false;

        _netListener.Close();
        return Task.CompletedTask;
    }

    private void ReceiveGameState(IAsyncResult result)
    {
        if (!_isRunning)
        {
            return;
        }

        try
        {
            var context = _netListener.EndGetContext(result);
            var json = TryProcessRequest(context);

            if (!string.IsNullOrWhiteSpace(json))
            {
                CurrentGameState = new EmptyGameState(json);
            }
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "[NetworkListener] ReceiveGameState error:");
        }

        //run in another thread to reset stack
        Task.Run(() => _netListener.BeginGetContext(ReceiveGameState, null));
    }

    private static string TryProcessRequest(HttpListenerContext context)
    {
        var request = context.Request;
        string json;

        using (var sr = new StreamReader(request.InputStream))
        {
            json = sr.ReadToEnd();
        }

        using (var response = context.Response)
        {
            response.StatusCode = (int)HttpStatusCode.OK;
            response.StatusDescription = "OK";
            response.ContentType = "application/json";
            response.ContentLength64 = 0;
        }

        return json;
    }
}