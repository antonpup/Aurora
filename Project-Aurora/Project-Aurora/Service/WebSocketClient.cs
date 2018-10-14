using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Service
{
    class WebSocketClient
    {
        private static object consoleLock = new object();
        private const int sendChunkSize = 256;
        private const int receiveChunkSize = 20480;
        private const bool verbose = true;
        private static readonly TimeSpan delay = TimeSpan.FromMilliseconds(30000);
        private static string uriWS = "ws://138.68.102.142:81?id=";
        //private static string uriWS = "wss://aa034e47.ngrok.io:81?id="; 

        public static string uriREST = "http://138.68.102.142:70/ping?ids=";
        //  public static string uriREST = "http://e13f69f4.ngrok.io:70/ping?ids=";
        static UTF8Encoding encoder = new UTF8Encoding();

        private static ClientWebSocket webSocket = null;
        internal static async Task DisconnectAsync()
        {
            webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            Global.Configuration.SocketClosed = true;
            ConfigManager.Save(Global.Configuration);
        }
        private static async Task connect(string clientID, bool listen)
        {
            try
            {
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri(uriWS + clientID), CancellationToken.None);
                Global.Configuration.SocketClosed = false;
                ConfigManager.Save(Global.Configuration);
                if (listen)
                {
                    await Task.WhenAll(Receive(webSocket));
                }
            }
            catch (Exception ex)
            {
                Global.logger.Error("WS Exception: {0}", ex);
            }
            finally
            {
                if (listen)
                {
                    if (webSocket != null)
                        webSocket.Dispose();
                    Global.logger.Info("Web socket closed");
                }
            }
        }
        internal static async void Connect(string clientID)
        {
            if (Global.Configuration.SocketClosed == false)
            {
                await connect(clientID,false );
                await DisconnectAsync();
            }
            ThreadPool.QueueUserWorkItem(async (o) =>
            {
                connect(clientID, true);
            });
        }

        private static async Task Receive(ClientWebSocket ws)
        {
            byte[] buffer = new byte[receiveChunkSize];
            while (ws.State == WebSocketState.Open)
            {
                
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    var p = Newtonsoft.Json.JsonConvert.DeserializeObject<ProfileSwitcherCommand>(encoder.GetString(buffer,0,result.Count));
                    ProfileSwitcher.Switch(p);
                }
            }
        }
    }
}
