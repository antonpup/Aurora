using Aurora.Controls;
using Aurora.Profiles;
using Aurora.Utils;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Evaluatable that detects when the value of the given numeric evaluatable changes by a particular amount.
    /// <para>Can be used in conjunction with the <see cref="BooleanExtender"/> to make the 'true' last longer than a single eval tick.</para>
    /// </summary>
    [Evaluatable("Number Change Detector", category: EvaluatableCategory.Maths)]
    public class NumericChangeDetector : Evaluatable<bool> {

        private double? lastValue;

        public NumericChangeDetector() { }
        public NumericChangeDetector(Evaluatable<double> eval, bool detectRising = true, bool detectFalling = true, double threshold = 0) {
            Evaluatable = eval;
            DetectRising = detectRising;
            DetectFalling = detectFalling;
            DetectionThreshold = threshold;
        }

        public Evaluatable<double> Evaluatable { get; set; } = EvaluatableDefaults.Get<double>();
        public bool DetectRising { get; set; } = true;
        public bool DetectFalling { get; set; } = true;
        public double DetectionThreshold { get; set; } = 0;

        public override Visual GetControl() => new StackPanel()
            .WithChild(new Control_EvaluatablePresenter { EvalType = typeof(double) }
                .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, this, nameof(Evaluatable), BindingMode.TwoWay))
            .WithChild(new CheckBox { Content = "Trigger on increase" }
                .WithBinding(CheckBox.IsCheckedProperty, this, nameof(DetectRising)))
            .WithChild(new CheckBox { Content = "Trigger on decrease" }
                .WithBinding(CheckBox.IsCheckedProperty, this, nameof(DetectFalling)))
            .WithChild(new DockPanel { LastChildFill = true }
                .WithChild(new Label { Content = "Change required", VerticalAlignment = System.Windows.VerticalAlignment.Center }, Dock.Left)
                .WithChild(new DoubleUpDown { Minimum = 0 }
                    .WithBinding(DoubleUpDown.ValueProperty, this, nameof(DetectionThreshold))));

        protected override bool Execute(IGameState gameState) {
            var val = Evaluatable.Evaluate(gameState);
            var @out = false;
            if (lastValue.HasValue) {
                // If threshold is 0, we want it to be true on any change, so we can't use >= (as this will always be true). We also can't use > as this means a
                // threshold of 1 would not trigger on change 5 -> 6. By making it the smallest possible double when it's 0, it means that any change is detected
                // but 0 won't be - exactly what we want.
                var threshold = DetectionThreshold == 0 ? double.Epsilon : DetectionThreshold;
                var delta = lastValue.Value - val;
                @out = (DetectRising && delta <= -threshold) || (DetectFalling && delta >= threshold);
            }
            lastValue = val;
            return @out;
        }
        
        public override Evaluatable<bool> Clone() => new NumericChangeDetector { Evaluatable = Evaluatable.Clone(), DetectRising = DetectRising, DetectFalling = DetectFalling, DetectionThreshold = DetectionThreshold };
    }



    /// <summary>
    /// Evaluatable that detects when a boolean value changes.
    /// </summary>
    [Evaluatable("Boolean Change Detector", category: EvaluatableCategory.Logic)]
    public class BooleanChangeDetector : Evaluatable<bool> {

        private bool? lastValue;

        public BooleanChangeDetector() { }
        public BooleanChangeDetector(Evaluatable<bool> eval) : this(eval, true, true) { }
        public BooleanChangeDetector(Evaluatable<bool> eval, bool detectTrue = true, bool detectFalse = true) {
            Evaluatable = eval;
            DetectTrue = detectTrue;
            DetectFalse = detectFalse;
        }

        public Evaluatable<bool> Evaluatable { get; set; } = EvaluatableDefaults.Get<bool>();
        public bool DetectTrue { get; set; } = true;
        public bool DetectFalse { get; set; } = true;

        public override Visual GetControl() => new StackPanel()
            .WithChild(new Control_EvaluatablePresenter { EvalType = typeof(bool) }
                .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, this, nameof(Evaluatable), BindingMode.TwoWay))
            .WithChild(new CheckBox { Content = "Trigger on become true" }
                .WithBinding(CheckBox.IsCheckedProperty, this, nameof(DetectTrue)))
            .WithChild(new CheckBox { Content = "Trigger on become false" }
                .WithBinding(CheckBox.IsCheckedProperty, this, nameof(DetectFalse)));

        protected override bool Execute(IGameState gameState) {
            var val = Evaluatable.Evaluate(gameState);
            var result = (val  && lastValue == false  && DetectTrue) // Result is true if: the next value is true, the old value was false and we are detecting true
                      || (!val && lastValue == true && DetectFalse); // Or the next value is false, the old value was true and we are detecting false
            lastValue = val;
            return result;
        }

        public override Evaluatable<bool> Clone() => new BooleanChangeDetector { Evaluatable = Evaluatable.Clone(), DetectTrue = DetectTrue, DetectFalse = DetectFalse };
    }
}
