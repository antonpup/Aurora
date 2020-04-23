using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Aurora.Profiles;
using Aurora.Utils;

namespace Aurora.Settings.Overrides.Logic.Number {

    /// <summary>
    /// An evaluatable that returns the time since the user has pressed a keyboard key or clicked the mouse.
    /// </summary>
    [Evaluatable("Away Time", category: EvaluatableCategory.Input)]
    public class NumberAwayTime : IEvaluatable<double> {

        /// <summary>Gets or sets the time unit that the time is being measured in.</summary>
        public TimeUnit TimeUnit { get; set; }

        #region ctors
        /// <summary>Create a new away time evaluatable returning the time in seconds.</summary>
        public NumberAwayTime() : this(TimeUnit.Seconds) { }
        /// <summary>Creates a new away time that measures the time in the specified time unit.</summary>
        public NumberAwayTime(TimeUnit unit) {
            TimeUnit = unit;
        }
        #endregion

        // Control
        public Visual GetControl() => new ComboBox {
                DisplayMemberPath = "Key",
                SelectedValuePath = "Value",
                ItemsSource = EnumUtils.GetEnumItemsSource<TimeUnit>()
            }.WithBinding(ComboBox.SelectedValueProperty, this, "TimeUnit", BindingMode.TwoWay);

        /// <summary>Checks to see if the duration since the last input is greater than the given inactive time.</summary>
        public double Evaluate(IGameState gameState) {
            var idleTime = ActiveProcessMonitor.GetTimeSinceLastInput();
            switch (TimeUnit) {
                case TimeUnit.Milliseconds: return idleTime.TotalMilliseconds;
                case TimeUnit.Seconds: return idleTime.TotalSeconds;
                case TimeUnit.Minutes: return idleTime.TotalMinutes;
                case TimeUnit.Hours: return idleTime.TotalHours;
                default: return 0;
            };
        }
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public IEvaluatable<double> Clone() => new NumberAwayTime { TimeUnit = TimeUnit };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
}
