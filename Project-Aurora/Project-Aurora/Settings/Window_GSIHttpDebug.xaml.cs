using Newtonsoft.Json.Linq;
using System;
using System.Windows;
using System.Windows.Documents;

namespace Aurora.Settings {
    /// <summary>
    /// A window that logs the latest new game state recieved via HTTP, regardless of the provider.
    /// </summary>
    public partial class Window_GSIHttpDebug : Window {

        public Window_GSIHttpDebug() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            // When the window has opened and loaded, start listening to the NetworkListener for when it
            // recieves new GameStates.
            Global.net_listener.NewGameState += Net_listener_NewGameState;

            // If a gamestate is already stored by the network listener, display it to the user immediately.
            if (Global.net_listener.CurrentGameState != null)
                SetJsonText(Global.net_listener.CurrentGameState.json);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            // When the window closes, we need to clean up our listener, otherwise it'll keep listening and
            // this window will never get garbage collected.
            Global.net_listener.NewGameState -= Net_listener_NewGameState;
        }

        private void Net_listener_NewGameState(Profiles.IGameState gamestate) {
            // This needs to be invoked due to the UI thread being different from the networking thread.
            // Without this, an exception is thrown trying to update the text box.
            Dispatcher.Invoke(() => SetJsonText(gamestate.json));
        }

        /// <summary>
        /// Sets the text of the body preview text box to the given (json) string.
        /// </summary>
        private void SetJsonText(string json) {
            // Pretty-print the JSON (add new lines and indentations)
            string prettyJson = JToken.Parse(json).ToString(Newtonsoft.Json.Formatting.Indented);
            BodyPreviewTxt.Document.Blocks.Clear();
            BodyPreviewTxt.Document.Blocks.Add(new Paragraph(new Run(prettyJson)));
        }
    }
}
