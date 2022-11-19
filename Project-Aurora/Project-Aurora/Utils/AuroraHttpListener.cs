using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Aurora.Profiles;
using Application = System.Windows.Application;

namespace Aurora.Utils;

public delegate void NewGameStateHandler(IGameState gamestate);
public class AuroraHttpListener
{
    private bool _isRunning;
    private IGameState _currentGameState;
    private HttpListener _netListener;
    private readonly int _port;

    public IGameState CurrentGameState
    {
        get => _currentGameState;
        private set
        {
            _currentGameState = value;
            NewGameState?.Invoke(CurrentGameState);
        }
    }

    /// <summary>
    ///  Event for handing a newly received game state
    /// </summary>
    public event NewGameStateHandler NewGameState = delegate { };

    /// <summary>
    /// A GameStateListener that listens for connections on http://localhost:port/
    /// </summary>
    /// <param name="port"></param>
    public AuroraHttpListener(int port)
    {
        _port = port;
        _netListener = new HttpListener();
        _netListener.Prefixes.Add("http://127.0.0.1:" + port + "/");
        _netListener.Prefixes.Add("http://localhost:" + port + "/");
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
            if (exc.ErrorCode == 5)//Access Denied
                MessageBox.Show("Access error during start of network listener.\r\n\r\n" +
                                "To fix this issue, please run the following commands as admin in Command Prompt:\r\n" +
                                $"   netsh http add urlacl url=http://localhost:{_port}/ user=Everyone listen=yes\r\nand\r\n" +
                                $"   netsh http add urlacl url=http://127.0.0.1:{_port}/ user=Everyone listen=yes", 
                    "Aurora - Error");

            Global.logger.Error(exc.ToString());

            return false;
        }
        _isRunning = true;

        BeginRun();
        return true;
    }

    /// <summary>
    /// Stops listening for GameState requests
    /// </summary>
    public Task Stop()
    {
        _isRunning = false;

        _netListener?.Close();
        _netListener = null;
        return Task.CompletedTask;
    }

    private void BeginRun()
    {
        _netListener.BeginGetContext(ReceiveGameState, _netListener);
    }

    private void ReceiveGameState(IAsyncResult result)
    {
        if (_netListener == null || !_isRunning)
        {
            return;
        }

        try
        {
            var context = _netListener.EndGetContext(result);
            var request = context.Request;
            string json;

            using (var inputStream = request.InputStream)
            {
                using (var sr = new StreamReader(inputStream))
                    json = sr.ReadToEnd();
            }

            using (var response = context.Response)
            {
                response.StatusCode = (int) HttpStatusCode.OK;
                response.StatusDescription = "OK";
                response.ContentType = "text/html";
                response.ContentLength64 = 0;
                response.Close();
            }

            CurrentGameState = new EmptyGameState(json);
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "[NetworkListener] ReceiveGameState error:");
        }
        _netListener.BeginGetContext(ReceiveGameState, null);
    }
}