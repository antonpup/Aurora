using System;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Aurora.Profiles;
using Application = System.Windows.Application;

namespace Aurora
{
    public delegate void NewGameStateHandler(IGameState gamestate);
    public delegate void WrapperConnectionClosedHandler(string process);
    public delegate void CommandRecievedHandler(string command, string args);

    public class NetworkListener
    {
        private bool _isRunning;
        private HttpListener _netListener;
        private IGameState _currentGameState;

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
        /// Gets the port that is being listened
        /// </summary>
        public int Port { get; }

        /// <summary>
        ///  Event for handing a newly received game state
        /// </summary>
        public event NewGameStateHandler NewGameState = delegate { };

        public event WrapperConnectionClosedHandler WrapperConnectionClosed = delegate { };

        public event CommandRecievedHandler CommandReceived = delegate { };

        /// <summary>
        /// Returns whether or not the wrapper is connected through IPC
        /// </summary>
        public bool IsWrapperConnected { get; private set; }

        /// <summary>
        /// Returns the process of the wrapped connection
        /// </summary>
        public string WrappedProcess { get; private set; } = "";
        

        private NamedPipeServerStream _ipcPipeStream;
        private NamedPipeServerStream _ipcCommandPipeStream;

        private NetworkListener()
        {
            CommandReceived += NetworkListener_CommandReceived;
        }

        /// <summary>
        /// A GameStateListener that listens for connections on http://localhost:port/
        /// </summary>
        /// <param name="port"></param>
        public NetworkListener(int port) : this()
        {
            Port = port;
            _netListener = new HttpListener();
            _netListener.Prefixes.Add("http://127.0.0.1:" + port + "/");
            _netListener.Prefixes.Add("http://localhost:" + port + "/");
        }

        /// <summary>
        /// A GameStateListener that listens for connections to the specified URI
        /// </summary>
        /// <param name="URI">The URI to listen to</param>
        public NetworkListener(string URI) : this()
        {
            if (!URI.EndsWith("/"))
                URI += "/";

            var uriPattern = new Regex("^https?:\\/\\/.+:([0-9]*)\\/$", RegexOptions.IgnoreCase);
            var portMatch = uriPattern.Match(URI);

            if (!portMatch.Success)
                throw new ArgumentException("Not a valid URI: " + URI);

            Port = Convert.ToInt32(portMatch.Groups[1].Value);

            _netListener = new HttpListener();
            _netListener.Prefixes.Add(URI);

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
                                    $"   netsh http add urlacl url=http://localhost:{Port}/ user=Everyone listen=yes\r\nand\r\n" +
                                    $"   netsh http add urlacl url=http://127.0.0.1:{Port}/ user=Everyone listen=yes", 
                        "Aurora - Error");

                Global.logger.Error(exc.ToString());

                return false;
            }
            _isRunning = true;

            BeginRun();
            BeginIpcServer();
            BeginAuroraCommandsServerIpcTask();
            return true;

        }

        /// <summary>
        /// Stops listening for GameState requests
        /// </summary>
        public async Task Stop()
        {
            _isRunning = false;

            _netListener?.Close();
            _netListener = null;

            _ipcPipeStream.Close();
            await _ipcPipeStream.DisposeAsync();

            _ipcCommandPipeStream.Close();
            if (_ipcCommandPipeStream != null)
            {
                if (_ipcCommandPipeStream.IsConnected)
                    _ipcCommandPipeStream.Disconnect();
                await _ipcCommandPipeStream.DisposeAsync();
            }
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

                using (HttpListenerResponse response = context.Response)
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
            _netListener.BeginGetContext(ReceiveGameState, _netListener);
        }

        private void HandleNewIpcGameState(string gsData)
        {
            Task.Run(() =>
                {
                    var newState = new GameState_Wrapper(gsData); //GameState_Wrapper 

                    IsWrapperConnected = true;
                    WrappedProcess = newState.Provider.Name.ToLowerInvariant();
                    CurrentGameState = newState;
                }
            );
        }

        private void BeginIpcServer()
        {
            _ipcPipeStream = new NamedPipeServerStream(
                "Aurora\\server",
                PipeDirection.In,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Message,
                PipeOptions.None,
                5 * 1024,
                5 * 1024
            );
            
            IsWrapperConnected = false;
            WrappedProcess = "";
            Global.logger.Info("[IPCServer] Pipe created {}", _ipcPipeStream?.GetHashCode() ?? -1);

            _ipcPipeStream?.BeginWaitForConnection(_ =>
            {
                Global.logger.Info("[IPCServer] Pipe connection established");

                using var sr = new StreamReader(_ipcPipeStream);
                while (sr.ReadLine() is { } temp)
                {
                    try
                    {
                        HandleNewIpcGameState(temp);
                    }
                    catch (Exception exc)
                    {
                        Global.logger.Error("[IPCServer] HandleNewIPCGameState Exception, " + exc);
                        Global.logger.Info("Recieved data that caused error:\n\r" + temp);
                    }

                }
            }, null);
        }

        private void BeginAuroraCommandsServerIpcTask()
        {
            _ipcCommandPipeStream = new NamedPipeServerStream(
                "Aurora\\interface",
                PipeDirection.In,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Message,
                PipeOptions.None,
                5 * 1024,
                5 * 1024
            );
            Global.logger.Info("[AuroraCommandsServerIPC] Pipe created {}", _ipcCommandPipeStream?.GetHashCode() ?? -1);

            void AsyncCallback(IAsyncResult ar)
            {
                Global.logger.Info("[AuroraCommandsServerIPC] Pipe connection established");

                using var sr = new StreamReader(_ipcCommandPipeStream);
                string temp;
                while ((temp = sr.ReadLine()) != null)
                {
                    Global.logger.Info("[AuroraCommandsServerIPC] Recieved command: " + temp);
                    var split = temp.Contains(':') ? temp.Split(':') : new[] { temp };
                    CommandReceived.Invoke(split[0], split.Length > 1 ? split[1] : "");
                }
            }
            _ipcCommandPipeStream?.BeginWaitForConnection(AsyncCallback, null);
        }

        private void NetworkListener_CommandReceived(string command, string args)
        {
            switch (command)
            {
                case "restore":
                    Global.logger.Info("Initiating command restore");
                    Application.Current.Dispatcher.Invoke(() => ((ConfigUI)Application.Current.MainWindow).ShowWindow());
                    break;
            }
        }
    }
}
