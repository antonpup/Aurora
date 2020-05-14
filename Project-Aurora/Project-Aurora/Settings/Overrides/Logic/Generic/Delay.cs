using Aurora.Profiles;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic.Generic {

    /// <summary>
    /// Generic evaluatable that delays the source input by a set amount of time.
    /// </summary>
    public abstract class DelayGeneric<T> : Evaluatable<T> {

        /// <summary>Keeps track of all changes that have happened and which will need to be repeated.</summary>
        private readonly Queue<(DateTime changeTime, T value)> history = new Queue<(DateTime changeTime, T value)>();

        /// <summary>The last value returned by the evaluation of the 'Source' evaluatable.</summary>
        private T lastValue;

        /// <summary>The currently (delayed) value returned by this evaluatable.</summary>
        private T currentValue;

        /// <summary>An evaluatable that will be delayed bu the desired amount.</summary>
        public Evaluatable<T> Source { get; set; }

        /// <summary>The amount of time to delay the evaluatable by (in seconds).</summary>
        public double Delay { get; set; } = 3;

        // Ctors
        public DelayGeneric() { Source = EvaluatableDefaults.Get<T>(); }
        public DelayGeneric(Evaluatable<T> source, double delay) { Source = source; Delay = delay; }

        // Control
        public override Visual GetControl() => new Control_Delay<T>(this);

        // Eval
        protected override T Execute(IGameState gameState) {
            // First, evaluate the source evaluatable and check if the returned value is different from the last one we read.
            var val = Source.Evaluate(gameState);
            if (!EqualityComparer<T>.Default.Equals(val, lastValue)) {
                // If different, record the time it changed and (add the delay so that we don't have to keep adding when checking later)
                history.Enqueue((DateTime.Now.AddSeconds(Delay), val));
                lastValue = val;
            }

            // Next, check if the time next item in the queue changed has passed, update the current value
            // Note that we don't need to check other items since they are kept in order by the queue
            if (history.Count > 0 && history.Peek().changeTime < DateTime.Now)
                currentValue = history.Dequeue().value;

            return currentValue;
        }
    }


    // Concrete classes
    [Evaluatable("Delay", category: EvaluatableCategory.Misc)]
    public class DelayBoolean : DelayGeneric<bool> {
        public DelayBoolean() : base() { }
        public DelayBoolean(Evaluatable<bool> source, double delay) : base(source, delay) { }
        public override Evaluatable<bool> Clone() => new DelayBoolean(Source, Delay);
    }

    [Evaluatable("Delay", category: EvaluatableCategory.Misc)]
    public class DelayNumeric : DelayGeneric<double> {
        public DelayNumeric() : base() { }
        public DelayNumeric(Evaluatable<double> source, double delay) : base(source, delay) { }
        public override Evaluatable<double> Clone() => new DelayNumeric(Source, Delay);
    }

    [Evaluatable("Delay", category: EvaluatableCategory.Misc)]
    public class DelayString : DelayGeneric<string> {
        public DelayString() : base() { }
        public DelayString(Evaluatable<string> source, double delay) : base(source, delay) { }
        public override Evaluatable<string> Clone() => new DelayString(Source, Delay);
    }
}
