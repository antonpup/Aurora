using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Aurora.Profiles;
using Aurora.Utils;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Extends the length of a boolean 'true' state. For example, if the sub-evaluatable evaluated to true for 1 second and this had it's
    /// <see cref="ExtensionTime"/> set to 3 seconds, the result of this would be a 3 second long "true" signal. Receiving another true
    /// signal while already extending an existing signal will restart the timer.
    /// </summary>
    [OverrideLogic("True Extender", category: OverrideLogicCategory.Logic)]
    public class BooleanExtender : IEvaluatable<bool> {

        private Stopwatch sw = new Stopwatch();

        public BooleanExtender() { }
        public BooleanExtender(IEvaluatable<bool> evaluatable) { Evaluatable = evaluatable; }
        public BooleanExtender(IEvaluatable<bool> evaluatable, double time, TimeUnit timeUnit = TimeUnit.Seconds) : this(evaluatable) { ExtensionTime = time; TimeUnit = timeUnit; }

        public IEvaluatable<bool> Evaluatable { get; set; } = new BooleanConstant();
        public double ExtensionTime { get; set; } = 5;
        public TimeUnit TimeUnit { get; set; } = TimeUnit.Seconds;

        private StackPanel sp;
        private Control_EvaluatablePresenter ep;
        public Visual GetControl(Application a) => sp ?? (sp = new StackPanel()
            .WithChild(ep = new Control_EvaluatablePresenter { EvalType = EvaluatableType.Boolean, Margin = new System.Windows.Thickness(24, 0, 0, 0) }
                .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, new Binding("Evaluatable") { Source = this, Mode = BindingMode.TwoWay }))
            .WithChild(new Control_TimeAndUnit()
                .WithBinding(Control_TimeAndUnit.TimeProperty, new Binding("ExtensionTime") { Source = this, Mode = BindingMode.TwoWay })
                .WithBinding(Control_TimeAndUnit.UnitProperty, new Binding("TimeUnit") { Source = this, Mode = BindingMode.TwoWay })
            ));

        public bool Evaluate(IGameState gameState) {
            var res = Evaluatable.Evaluate(gameState);
            if (res) sw.Restart();
            switch (TimeUnit) {
                case TimeUnit.Milliseconds: return sw.IsRunning && sw.Elapsed.TotalMilliseconds < ExtensionTime;
                case TimeUnit.Seconds: return sw.IsRunning && sw.Elapsed.TotalSeconds < ExtensionTime;
                case TimeUnit.Minutes: return sw.IsRunning && sw.Elapsed.TotalMinutes < ExtensionTime;
                case TimeUnit.Hours: return sw.IsRunning && sw.Elapsed.TotalHours < ExtensionTime;
                default: return false;
            }
        }
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public void SetApplication(Application application) {
            ep.Application = application;
            Evaluatable.SetApplication(application);
        }

        public IEvaluatable<bool> Clone() => new BooleanExtender { Evaluatable = Evaluatable.Clone(), ExtensionTime = ExtensionTime, TimeUnit = TimeUnit };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
}
