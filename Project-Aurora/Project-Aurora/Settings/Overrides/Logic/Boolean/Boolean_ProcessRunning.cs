using Aurora.Profiles;
using Aurora.Utils;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Evaluatable that returns true/false depending on whether the given process name is running.
    /// </summary>
    [Evaluatable("Process Running", category: EvaluatableCategory.Misc)]
    public class BooleanProcessRunning : Evaluatable<bool>, INotifyPropertyChanged {

        public string ProcessName { get; set; } = "";

        public BooleanProcessRunning() { }
        public BooleanProcessRunning(string processName) { ProcessName = processName; }

        public event PropertyChangedEventHandler PropertyChanged;

        public override Visual GetControl() {
            var selectButton = new Button { Content = "Select", Padding = new System.Windows.Thickness(8, 0, 8, 0), Margin = new System.Windows.Thickness(8, 0, 0, 0) };
            selectButton.Click += (sender, e) => {
                var wnd = new Window_ProcessSelection { ButtonLabel = "Select" };
                if (wnd.ShowDialog() == true && !string.IsNullOrWhiteSpace(wnd.ChosenExecutableName))
                    ProcessName = wnd.ChosenExecutableName;
            };

            return new StackPanel { Orientation = Orientation.Horizontal }
                .WithChild(new Label { Content = "Is process" })
                .WithChild(new TextBox { MinWidth = 80 }
                    .WithBinding(TextBox.TextProperty, this, "ProcessName", BindingMode.TwoWay))
                .WithChild(selectButton)
                .WithChild(new Label { Content = "running" });
        }

        protected override bool Execute(IGameState gameState)
            => Global.LightingStateManager.RunningProcessMonitor.IsProcessRunning(ProcessName);
        
        public override Evaluatable<bool> Clone() => new BooleanProcessRunning { ProcessName = ProcessName };
    }
}