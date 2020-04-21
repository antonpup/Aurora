using Aurora.Profiles;
using Aurora.Utils;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Evaluatable that returns true/false depending on whether the given process name is running.
    /// </summary>
    [OverrideLogic("Process Running", category: OverrideLogicCategory.Misc)]
    public class BooleanProcessRunning : IEvaluatable<bool> {

        public string ProcessName { get; set; } = "";

        public BooleanProcessRunning() { }
        public BooleanProcessRunning(string processName) { ProcessName = processName; }

        public Visual GetControl() => new TextBox { MinWidth = 80 }
            .WithBinding(TextBox.TextProperty, new Binding("ProcessName") { Source = this, Mode = BindingMode.TwoWay });

        public bool Evaluate(IGameState gameState)
            => Global.LightingStateManager.RunningProcessMonitor.IsProcessRunning(ProcessName);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public IEvaluatable<bool> Clone() => new BooleanProcessRunning { ProcessName = ProcessName };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
}