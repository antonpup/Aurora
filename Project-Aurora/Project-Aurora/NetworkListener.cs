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
                NewGameState?.Invoke(CurrentGameState);
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

        private Thread ListenerThread;
        private Thread ServerThread;
        private Thread CommandThread;

        private NamedPipeServerStream IPCpipeStream;
        private NamedPipeServerStream IPCCommandpipeStream;

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
                ListenerThread = new Thread(new ThreadStart(Run));
                ListenerThread.IsBackground = true;
                try
                {
                    net_Listener.Start();
                }
                catch (HttpListenerException exc)
                {
                    if (exc.ErrorCode == 5)//Access Denied
                        System.Windows.MessageBox.Show($"Access error during start of network listener.\r\n\r\nTo fix this issue, please run the following commands as admin in Command Prompt:\r\n   netsh http add urlacl url=http://localhost:{Port}/ user=Everyone listen=yes\r\nand\r\n   netsh http add urlacl url=http://127.0.0.1:{Port}/ user=Everyone listen=yes", "Aurora - Error");

                    Global.logger.Error(exc.ToString());

                    return false;
                }
                isRunning = true;
                ListenerThread.Start();

                ServerThread = new Thread(IPCServerThread);
                ServerThread.IsBackground = true;
                ServerThread.Start();
                CommandThread = new Thread(AuroraCommandsServerIPC);
                CommandThread.IsBackground = true;
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

            if (ListenerThread != null)
            {
                ListenerThread.Abort();
                ListenerThread = null;
            }

            if (ServerThread != null)
            {
                ServerThread.Abort();
                ServerThread = null;
            }
                

            if (CommandThread != null)
            {
                CommandThread.Abort();
                CommandThread = null;
            }

            if(IPCpipeStream != null)
            {
                if(IPCpipeStream.IsConnected)
                    IPCpipeStream.Disconnect();
                IPCpipeStream.Dispose();
                IPCpipeStream = null;
            }

            if (IPCCommandpipeStream != null)
            {
                if (IPCCommandpipeStream.IsConnected)
                    IPCCommandpipeStream.Disconnect();
                IPCCommandpipeStream.Dispose();
                IPCCommandpipeStream = null;
            }
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
            CurrentGameState = new EmptyGameState(JSON);
        }

        private void HandleNewIPCGameState(string gs_data)
        {
            //Global.logger.LogLine("Received gs!");
            //Global.logger.LogLine(gs_data);

            var task = new System.Threading.Tasks.Task(() =>
                {
                    GameState_Wrapper new_state = new GameState_Wrapper(gs_data); //GameState_Wrapper 

                    wrapper_connected = true;
                    wrapped_process = new_state.Provider.Name.ToLowerInvariant();

                    //if (new_state.Provider.Name.ToLowerInvariant().Equals("gta5.exe"))
                    //CurrentGameState = new Profiles.GTA5.GSI.GameState_GTA5(gs_data);
                    //else
                    CurrentGameState = new_state;
                }
            );
            task.Start();
        }

        private void IPCServerThread()
        {
            PipeSecurity pipeSa = new PipeSecurity();
            pipeSa.SetAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                            PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));
            while (isRunning)
            {
                try
                {
                    using (IPCpipeStream = new NamedPipeServerStream(
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
                        Global.logger.Info("[IPCServer] Pipe created {0}", IPCpipeStream?.GetHashCode());

                        IPCpipeStream?.WaitForConnection();
                        Global.logger.Info("[IPCServer] Pipe connection established");

                        using (StreamReader sr = new StreamReader(IPCpipeStream))
                        {
                            string temp;
                            while ((temp = sr.ReadLine()) != null)
                            {
                                //Global.logger.LogLine(String.Format("{0}: {1}", DateTime.Now, temp));
                                try
                                {
                                    //Begin handling the game state outside this loop
                                    HandleNewIPCGameState(temp);
                                }
                                catch(Exception exc)
                                {
                                    Global.logger.Error("[IPCServer] HandleNewIPCGameState Exception, " + exc);
                                    //if (Global.isDebug)
                                        Global.logger.Info("Recieved data that caused error:\n\r"+temp);
                                }

                                //var task = new System.Threading.Tasks.Task(() => HandleNewIPCGameState(temp));
                                //task.Start();
                            }
                        }
                    }

                    WrapperConnectionClosed?.Invoke(wrapped_process);

                    wrapper_connected = false;
                    wrapped_process = "";
                    Global.logger.Info("[IPCServer] Pipe connection lost");
                }
                catch (Exception exc)
                {
                    IPCpipeStream?.Close();
                    IPCpipeStream?.Dispose();

                    WrapperConnectionClosed?.Invoke(wrapped_process);

                    wrapper_connected = false;
                    wrapped_process = "";
                    Global.logger.Info("[IPCServer] Named Pipe Exception, " + exc);
                }
            }
        }

        private void AuroraCommandsServerIPC()
        {
            PipeSecurity pipeSa = new PipeSecurity();
            pipeSa.SetAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                            PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));
            while (isRunning)
            {
                try
                {
                    using (IPCCommandpipeStream = new NamedPipeServerStream(
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
                        Global.logger.Info("[AuroraCommandsServerIPC] Pipe created {0}", IPCCommandpipeStream?.GetHashCode());

                        IPCCommandpipeStream?.WaitForConnection();
                        Global.logger.Info("[AuroraCommandsServerIPC] Pipe connection established");

                        using (StreamReader sr = new StreamReader(IPCCommandpipeStream))
                        {
                            string temp;
                            while ((temp = sr.ReadLine()) != null)
                            {
                                Global.logger.Info("[AuroraCommandsServerIPC] Recieved command: " + temp);
                                string[] split = temp.Contains(':') ? temp.Split(':') : new[] { temp };
                                CommandRecieved.Invoke(split[0], split.Length > 1 ? split[1] : "");
                            }
                        }
                    }

                    Global.logger.Info("[AuroraCommandsServerIPC] Pipe connection lost");
                }
                catch (Exception exc)
                {
                    Global.logger.Info("[AuroraCommandsServerIPC] Named Pipe Exception, " + exc, Logging_Level.Error);
                }
            }
        }

        private void NetworkListener_CommandRecieved(string command, string args)
        {
            switch (command)
            {
                case "restore":
                    Global.logger.Info("Initiating command restore");
                    System.Windows.Application.Current.Dispatcher.Invoke(() => ((ConfigUI)System.Windows.Application.Current.MainWindow).ShowWindow());
                    break;
            }
        }
    }
}
