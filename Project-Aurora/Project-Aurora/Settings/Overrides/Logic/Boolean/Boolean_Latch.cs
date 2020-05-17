using Aurora.Profiles;
using Aurora.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic.Boolean {

    /// <summary>
    /// A simple memory gate that can be used for storing a boolean state.
    /// When 'Set' is true, the gate will start outputting true until 'Reset' becomes true.
    /// </summary>
    [Evaluatable("Flip-flop", category: EvaluatableCategory.Logic)]
    public class Boolean_Latch : Evaluatable<bool> {

        private bool state = false;

        public Evaluatable<bool> Set { get; set; }
        public Evaluatable<bool> Reset { get; set; }

        public Boolean_Latch() : this(EvaluatableDefaults.Get<bool>(),EvaluatableDefaults.Get<bool>()) { }
        public Boolean_Latch(Evaluatable<bool> set, Evaluatable<bool> reset) { Set = set; Reset = reset; }

        protected override bool Execute(IGameState gameState) {
            if (Reset.Evaluate(gameState))
                state = false;
            if (Set.Evaluate(gameState))
                state = true;
            return state;
        }
        
        public override Visual GetControl() => new StackPanel()
            .WithChild(new TextBlock { Text = "Flip-Flop", FontWeight = FontWeights.Bold })
            .WithChild(new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4) }
                .WithChild(new Label { Content = "Set" })
                .WithChild(new Control_EvaluatablePresenter { EvalType = typeof(bool) }
                    .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, new Binding(nameof(Set)) { Source = this, Mode = BindingMode.TwoWay })))
            .WithChild(new StackPanel { Orientation = Orientation.Horizontal }
                .WithChild(new Label { Content = "Reset" })
                .WithChild(new Control_EvaluatablePresenter { EvalType = typeof(bool) }
                    .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, new Binding(nameof(Reset)) { Source = this, Mode = BindingMode.TwoWay })));

        public override Evaluatable<bool> Clone() => new Boolean_Latch(Set.Clone(), Reset.Clone());        
    }
}
