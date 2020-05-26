using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Documents;

namespace Aurora.Settings {
    /// <summary>
    /// A window that logs the latest new game state recieved via HTTP, regardless of the provider.
    /// </summary>
    public partial class Window_GSIHttpDebug : Window
    {
        static Window_GSIHttpDebug HttpDebugWindow = null;
        /// <summary>
        /// Opens the HttpDebugWindow if not already opened. If opened bring it to the foreground. 
        /// </summary>
        public static void Open()
        {
            if (HttpDebugWindow == null)
            {
                HttpDebugWindow = new Window_GSIHttpDebug();
                HttpDebugWindow.Show();
            }
            else
            {
                HttpDebugWindow.Activate();
            }
        }

        private Timer timeDisplayTimer;
        private DateTime? lastRequestTime = null;

        private Window_GSIHttpDebug()
        {
            this.SourceInitialized += Window_SetPlacement;
            this.Closed += Window_Closed;
            InitializeComponent();
            DataContext = Global.Configuration;
        }

        private void Window_SetPlacement(object sender, EventArgs e)
        {
            Utils.WindowPlacement.SetPlacement(this, Global.Configuration.HttpDebugPlacement);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            // When the window has opened and loaded, start listening to the NetworkListener for when it
            // recieves new GameStates.
            Global.net_listener.NewGameState += Net_listener_NewGameState;

            // If a gamestate is already stored by the network listener, display it to the user immediately.
            if (Global.net_listener.CurrentGameState != null)
                SetJsonText(Global.net_listener.CurrentGameState.Json);

            // Start a timer to update the time displays for the request
            timeDisplayTimer = new Timer(_ => Dispatcher.Invoke(() => {
                if (lastRequestTime.HasValue)
                    CurRequestTime.Text = lastRequestTime.ToString() + " (" + (DateTime.Now - lastRequestTime).Value.TotalSeconds.ToString("0.00") + "s ago)";
            }), null, 0, 50);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Save current window Placement
            Global.Configuration.HttpDebugPlacement = Utils.WindowPlacement.GetPlacement(this);

            // When the window closes, we need to clean up our listener, otherwise it'll keep listening and
            // this window will never get garbage collected.
            Global.net_listener.NewGameState -= Net_listener_NewGameState;

            // Destory the timer to ensure it doesn't keep running and eating memory.
            timeDisplayTimer.Dispose();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //Set the HttpDebugWindow instance to null if it got closed.
            if (HttpDebugWindow != null && HttpDebugWindow.Equals(this))
                HttpDebugWindow = null;
        }

        private void Net_listener_NewGameState(Profiles.IGameState gamestate)
        {
            // This needs to be invoked due to the UI thread being different from the networking thread.
            // Without this, an exception is thrown trying to update the text box.
            Dispatcher.Invoke(() => SetJsonText(gamestate.Json));
            // Also record the time this request came in
            lastRequestTime = DateTime.Now;
        }

        /// <summary>
        /// Sets the text of the body preview text box to the given (json) string.
        /// </summary>
        private void SetJsonText(string json) {
            // Pretty-print the JSON (add new lines and indentations)
            BodyPreviewTxt.Text = JToken.Parse(json).ToString(Newtonsoft.Json.Formatting.Indented);
        }
    }
}
