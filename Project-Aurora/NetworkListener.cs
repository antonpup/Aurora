using Aurora.Profiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora
{
    public delegate void NewGameStateHandler(IGameState gamestate);
    public delegate void WrapperConnectionClosedHandler(string process);
    public delegate void CommandRecievedHandler(string command, string args);

    public class NetworkListener
    {
        private bool isRunning = false;
        private bool wrapper_connected = false;
        private string wrapped_process = "";
        private int connection_port;
        private HttpListener net_Listener;
        private AutoResetEvent waitForConnection = new AutoResetEvent(false);
        private IGameState currentGameState;

        public IGameState CurrentGameState
        {
            get
            {
                return currentGameState;
            }
            private set
            {
                currentGameState = value;
                RaiseOnNewGameState();
            }
        }

        /// <summary>
        /// Gets the port that is being listened
        /// </summary>
        public int Port { get { return connection_port; } }

        /// <summary>
        /// Returns whether or not the listener is running
        /// </summary>
        public bool Running { get { return isRunning; } }

        /// <summary>
        ///  Event for handing a newly received game state
        /// </summary>
        public event NewGameStateHandler NewGameState = delegate { };

        public event WrapperConnectionClosedHandler WrapperConnectionClosed = delegate { };

        public event CommandRecievedHandler CommandRecieved = delegate { };

        /// <summary>
        /// Returns whether or not the wrapper is connected through IPC
        /// </summary>
        public bool IsWrapperConnected { get { return wrapper_connected; } }

        /// <summary>
        /// Returns the process of the wrapped connection
        /// </summary>
        public string WrappedProcess { get { return wrapped_process; } }

        public NetworkListener()
        {
            CommandRecieved += NetworkListener_CommandRecieved;
        }

        /// <summary>
        /// A GameStateListener that listens for connections on http://localhost:port/
        /// </summary>
        /// <param name="Port"></param>
        public NetworkListener(int Port) : this()
        {
            connection_port = Port;
            net_Listener = new HttpListener();
            net_Listener.Prefixes.Add("http://127.0.0.1:" + Port + "/");
            net_Listener.Prefixes.Add("http://localhost:" + Port + "/");
        }

        /// <summary>
        /// A GameStateListener that listens for connections to the specified URI
        /// </summary>
        /// <param name="URI">The URI to listen to</param>
        public NetworkListener(string URI) : this()
        {
            if (!URI.EndsWith("/"))
                URI += "/";

            Regex URIPattern = new Regex("^https?:\\/\\/.+:([0-9]*)\\/$", RegexOptions.IgnoreCase);
            Match PortMatch = URIPattern.Match(URI);

            if (!PortMatch.Success)
                throw new ArgumentException("Not a valid URI: " + URI);

            connection_port = Convert.ToInt32(PortMatch.Groups[1].Value);

            net_Listener = new HttpListener();
            net_Listener.Prefixes.Add(URI);

        }

        /// <summary>
        /// Starts listening for GameState requests
        /// </summary>
        public bool Start()
        {
            if (!isRunning)
            {
                Thread ListenerThread = new Thread(new ThreadStart(Run));
                try
                {
                    net_Listener.Start();
                }
                catch (HttpListenerException exc)
                {
                    if(exc.ErrorCode == 5)//Access Denied
                        System.Windows.MessageBox.Show($"Access error during start of network listener.\r\n\r\nTo fix this issue, please run the following commands as admin in Command Prompt:\r\n   netsh http add urlacl url=http://localhost:{Port}/ user=Everyone listen=yes\r\nand\r\n   netsh http add urlacl url=http://127.0.0.1:{Port}/ user=Everyone listen=yes", "Aurora - Error");

                    Global.logger.LogLine(exc.ToString(), Logging_Level.Error);

                    return false;
                }
                isRunning = true;
                ListenerThread.Start();

                Thread ServerThread = new Thread(IPCServerThread);
                ServerThread.Start();
                Thread CommandThread = new Thread(AuroraCommandsServerIPC);
                CommandThread.Start();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stops listening for GameState requests
        /// </summary>
        public void Stop()
        {
            isRunning = false;
        }

        private void Run()
        {
            while (isRunning)
            {
                net_Listener.BeginGetContext(ReceiveGameState, net_Listener);
                waitForConnection.WaitOne();
                waitForConnection.Reset();
            }
            net_Listener.Stop();
        }

        private void ReceiveGameState(IAsyncResult result)
        {
            HttpListenerContext context = net_Listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;
            string JSON;

            waitForConnection.Set();

            using (Stream inputStream = request.InputStream)
            {
                using (StreamReader sr = new StreamReader(inputStream))
                    JSON = sr.ReadToEnd();
            }

            using (HttpListenerResponse response = context.Response)
            {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.StatusDescription = "OK";
                response.ContentType = "text/html";
                response.ContentLength64 = 0;
                response.Close();
            }
            CurrentGameState = new GameState(JSON);
        }

        private void RaiseOnNewGameState()
        {
            var hander = NewGameState;

            if (hander != null)
                hander.Invoke(CurrentGameState);
        }

        private void HandleNewIPCGameState(string gs_data)
        {
            GameState_Wrapper new_state = new GameState_Wrapper(gs_data); //GameState_Wrapper 

            wrapper_connected = true;
            wrapped_process = new_state.Provider.Name.ToLowerInvariant();

            if (new_state.Provider.Name.ToLowerInvariant().Equals("gta5.exe"))
                CurrentGameState = new Profiles.GTA5.GSI.GameState_GTA5(gs_data);
            else
                CurrentGameState = new_state;
        }

        private void IPCServerThread()
        {
            PipeSecurity pipeSa = new PipeSecurity();
            pipeSa.SetAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                            PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));
            while (true)
            {
                try
                {
                    using (NamedPipeServerStream pipeStream = new NamedPipeServerStream(
                    "Aurora\\server",
                    PipeDirection.In,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Message,
                    PipeOptions.None,
                    5 * 1024,
                    5 * 1024,
                    pipeSa,
                    HandleInheritability.None
                    ))
                    {
                        wrapper_connected = false;
                        wrapped_process = "";
                        Global.logger.LogLine(String.Format("[IPCServer] Pipe created {0}", pipeStream.GetHashCode()));

                        pipeStream.WaitForConnection();
                        Global.logger.LogLine("[IPCServer] Pipe connection established");

                        using (StreamReader sr = new StreamReader(pipeStream))
                        {
                            string temp;
                            while ((temp = sr.ReadLine()) != null)
                            {
                                //Global.logger.LogLine(String.Format("{0}: {1}", DateTime.Now, temp));

                                //Begin handling the game state outside this loop
                                var task = new System.Threading.Tasks.Task(() => HandleNewIPCGameState(temp));
                                task.Start();
                            }
                        }
                    }

                    WrapperConnectionClosed?.Invoke(wrapped_process);

                    wrapper_connected = false;
                    wrapped_process = "";
                    Global.logger.LogLine("[IPCServer] Pipe connection lost");
                }
                catch (Exception exc)
                {
                    WrapperConnectionClosed?.Invoke(wrapped_process);

                    wrapper_connected = false;
                    wrapped_process = "";
                    Global.logger.LogLine("[IPCServer] Named Pipe Exception, " + exc, Logging_Level.Error);
                }
            }
        }

        private void AuroraCommandsServerIPC()
        {
            PipeSecurity pipeSa = new PipeSecurity();
            pipeSa.SetAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                            PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));
            while (true)
            {
                try
                {
                    using (NamedPipeServerStream pipeStream = new NamedPipeServerStream(
                    "Aurora\\interface",
                    PipeDirection.In,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Message,
                    PipeOptions.None,
                    5 * 1024,
                    5 * 1024,
                    pipeSa,
                    HandleInheritability.None
                    ))
                    {
                        Global.logger.LogLine(String.Format("[AuroraCommandsServerIPC] Pipe created {0}", pipeStream.GetHashCode()));

                        pipeStream.WaitForConnection();
                        Global.logger.LogLine("[AuroraCommandsServerIPC] Pipe connection established");

                        using (StreamReader sr = new StreamReader(pipeStream))
                        {
                            string temp;
                            while ((temp = sr.ReadLine()) != null)
                            {
                                string[] split = temp.Contains(':') ? temp.Split(':') : new[] { temp };
                                CommandRecieved.Invoke(split[0], split.Length > 1 ? split[1] : "");
                            }
                        }
                    }

                    Global.logger.LogLine("[AuroraCommandsServerIPC] Pipe connection lost");
                }
                catch (Exception exc)
                {
                    Global.logger.LogLine("[AuroraCommandsServerIPC] Named Pipe Exception, " + exc, Logging_Level.Error);
                }
            }
        }

        private void NetworkListener_CommandRecieved(string command, string args)
        {
            switch(command)
            {
                case "restore":
                    Program.MainWindow.Dispatcher.Invoke(() => ((ConfigUI)Program.MainWindow).ShowWindow());
                    break;
            }
        }
    }
}
