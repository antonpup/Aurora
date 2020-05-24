using Aurora.Profiles;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Keys = System.Windows.Forms.Keys;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Condition that is true when a specific keyboard button is held down.
    /// </summary>
    [Evaluatable("Key Held", category: EvaluatableCategory.Input)]
    public class BooleanKeyDown : Evaluatable<bool> {

        /// <summary>Creates a new key held condition with the default key (Space) as the trigger key.</summary>
        public BooleanKeyDown() { }
        /// <summary>Creates a new key held condition with the given key as the trigger key.</summary>
        public BooleanKeyDown(Keys target) { TargetKey = target; }

        /// <summary>The key to be checked to see if it is held down.</summary>
        public Keys TargetKey { get; set; } = Keys.Space;

        /// <summary>Create a control where the user can select the key they wish to detect.</summary>
        public override Visual GetControl() => new StackPanel { Orientation = Orientation.Horizontal }
            .WithChild(new Label { Content = "Key held" })
            .WithChild(new Controls.Control_FieldPresenter { Type = typeof(Keys) }
                .WithBinding(Controls.Control_FieldPresenter.ValueProperty, new Binding("TargetKey") { Source = this, Mode = BindingMode.TwoWay }));

        /// <summary>True if the global event bus's pressed key list contains the target key.</summary>
        protected override bool Execute(IGameState gameState) => Global.InputEvents.PressedKeys.Contains(TargetKey);

        public override Evaluatable<bool> Clone() => new BooleanKeyDown { TargetKey = TargetKey };
    }


    /// <summary>
    /// Condition that is true when any keyboard button is held down
    /// </summary>
    [Evaluatable("Any Key Held", category: EvaluatableCategory.Input)]
    public class BooleanAnyKeyDown : Evaluatable<bool> {
        /// <summary>Creates a new any key held condition.</summary>
        public BooleanAnyKeyDown() { }

        /// <summary>Create a control with no options indicating what the evaluatable does.</summary>
        public override Visual GetControl() => new Label() { Content = "Any Key Held" };

        /// <summary>True if the global event bus's pressed key list contains any key.</summary>
        protected override bool Execute(IGameState gameState) => Global.InputEvents.PressedKeys.Any();
        public override Evaluatable<bool> Clone() => new BooleanAnyKeyDown();
    }


    /// <summary>
    /// Condition that is true when a specific keyboard button is held down.
    /// </summary>
    [Evaluatable("Key Press (Retain for duration)", category: EvaluatableCategory.Input)]
    public class BooleanKeyDownWithTimer : Evaluatable<bool> {
        private readonly Stopwatch watch = new Stopwatch();

        /// <summary>Creates a new key held condition with the default key (Space) as the trigger key.</summary>
        public BooleanKeyDownWithTimer() { }
        /// <summary>Creates a new key held condition with the given key as the trigger key.</summary>
        public BooleanKeyDownWithTimer(Keys target, float seconds) : this() { TargetKey = target; Seconds = seconds; }

        /// <summary>The key to be checked to see if it is held down.</summary>
        public Keys TargetKey { get; set; } = Keys.Space;
        public float Seconds { get; set; } = 1;

        /// <summary>Create a control where the user can select the key they wish to detect.</summary>
        public override Visual GetControl() => new StackPanel()
            .WithChild(new StackPanel { Orientation = Orientation.Horizontal }
                .WithChild(new TextBlock { Text = "Key held" })
                .WithChild(new Controls.Control_FieldPresenter { Type = typeof(Keys), Margin = new System.Windows.Thickness(0, 0, 0, 6) }
                    .WithBinding(Controls.Control_FieldPresenter.ValueProperty, new Binding("TargetKey") { Source = this, Mode = BindingMode.TwoWay })))
            .WithChild(new StackPanel { Orientation = Orientation.Horizontal }
                .WithChild(new TextBlock { Text = "For" })
                .WithChild(new Controls.Control_FieldPresenter { Type = typeof(float), Margin = new System.Windows.Thickness(5, 0, 5, 6) }
                    .WithBinding(Controls.Control_FieldPresenter.ValueProperty, new Binding("Seconds") { Source = this, Mode = BindingMode.TwoWay }))
                .WithChild(new TextBlock { Text = "Seconds" }));

        /// <summary>True if the global event bus's pressed key list contains the target key.</summary>
        protected override bool Execute(IGameState gameState) {
            if (Global.InputEvents.PressedKeys.Contains(TargetKey)) {
                watch.Restart();
                return true;
            } else if (watch.IsRunning && watch.Elapsed.TotalSeconds <= Seconds) {
                return true;
            } else
                watch.Stop();

            return false;
        }

        public override Evaluatable<bool> Clone() => new BooleanKeyDownWithTimer { Seconds = Seconds, TargetKey = TargetKey };
    }


    /// <summary>
    /// Condition that is true when a specific mouse button is held down.
    /// </summary>
    [Evaluatable("Mouse Button Held", category: EvaluatableCategory.Input)]
    public class BooleanMouseDown : Evaluatable<bool> {

        /// <summary>Creates a new key held condition with the default mouse button (Left) as the trigger button.</summary>
        public BooleanMouseDown() { }
        /// <summary>Creates a new key held condition with the given mouse button as the trigger button.</summary>
        public BooleanMouseDown(System.Windows.Forms.MouseButtons target) { TargetButton = target; }

        /// <summary>The mouse button to be checked to see if it is held down.</summary>
        public System.Windows.Forms.MouseButtons TargetButton { get; set; } = System.Windows.Forms.MouseButtons.Left;

        /// <summary>Create a control where the user can select the mouse button they wish to detect.</summary>
        public override Visual GetControl() => new StackPanel { Orientation = Orientation.Horizontal }
            .WithChild(new Label { Content = "When" })
            .WithChild(new Controls.Control_FieldPresenter { Type = typeof(System.Windows.Forms.MouseButtons), Margin = new System.Windows.Thickness(0, 0, 0, 6) }
                .WithBinding(Controls.Control_FieldPresenter.ValueProperty, new Binding("TargetButton") { Source = this, Mode = BindingMode.TwoWay }))
            .WithChild(new Label { Content = "mouse button down" });

        /// <summary>True if the global event bus's pressed mouse button list contains the target button.</summary>
        protected override bool Execute(IGameState gameState) => Global.InputEvents.PressedButtons.Contains(TargetButton);

        public override Evaluatable<bool> Clone() => new BooleanMouseDown { TargetButton = TargetButton };
    }


    /// <summary>
    /// Condition that is true when the specified lock key (e.g. caps lock) is active.
    /// </summary>
    [Evaluatable("Lock Key Active", category: EvaluatableCategory.Input)]
    public class BooleanLockKeyActive : Evaluatable<bool> {

        /// <summary>Creates a new key held condition with the default lock type (CapsLock) as the lock type.</summary>
        public BooleanLockKeyActive() { }
        /// <summary>Creates a new key held condition with the given button as the lock type button.</summary>
        public BooleanLockKeyActive(Keys target) { TargetKey = target; }

        public Keys TargetKey { get; set; } = Keys.CapsLock;

        /// <summary>Create a control allowing the user to specify which lock key to check.</summary>
        public override Visual GetControl() => new StackPanel { Orientation = Orientation.Horizontal }
            .WithChild(new ComboBox { ItemsSource = new[] { Keys.CapsLock, Keys.NumLock, Keys.Scroll } }
                .WithBinding(ComboBox.SelectedValueProperty, new Binding("TargetKey") { Source = this, Mode = BindingMode.TwoWay }))
            .WithChild(new Label { Content = "is locked" });

        /// <summary>Return true if the target lock key is active.</summary>
        protected override bool Execute(IGameState gameState) => System.Windows.Forms.Control.IsKeyLocked(TargetKey);

        public override Evaluatable<bool> Clone() => new BooleanLockKeyActive { TargetKey = TargetKey };
    }


    /// <summary>
    /// An evaluatable that returns true when the specified time has elapsed without the user pressing a keyboard button or clicking the mouse.
    /// </summary>
    [Evaluatable("Away Timer", category: EvaluatableCategory.Input)]
    public class BooleanAwayTimer : Evaluatable<bool> {

        /// <summary>Gets or sets the time before this timer starts returning true after there has been no user input.</summary>
        public double InactiveTime { get; set; }
        /// <summary>Gets or sets the time unit that is being used to measure the AFK time.</summary>
        public TimeUnit TimeUnit { get; set; }

        #region ctors
        /// <summary>Create a new away timer condition with the default time (60 seconds).</summary>
        public BooleanAwayTimer() : this(60) { }

        /// <summary>Creates a new away timer with the specified number of seconds before activating.</summary>
        public BooleanAwayTimer(double time) : this(time, TimeUnit.Seconds) { }

        /// <summary>Creates a new away timer with the specified time before activating.</summary>
        public BooleanAwayTimer(double time, TimeUnit unit) {
            InactiveTime = time;
            TimeUnit = unit;
        }
        #endregion

        public override Visual GetControl() => new StackPanel { Orientation = Orientation.Horizontal }
            .WithChild(new Label { Content = "Away for" })
            .WithChild(new Control_TimeAndUnit()
                .WithBinding(Control_TimeAndUnit.TimeProperty, new Binding("InactiveTime") { Source = this, Mode = BindingMode.TwoWay })
                .WithBinding(Control_TimeAndUnit.UnitProperty, new Binding("TimeUnit") { Source = this, Mode = BindingMode.TwoWay }));

        /// <summary>Checks to see if the duration since the last input is greater than the given inactive time.</summary>
        protected override bool Execute(IGameState gameState) {
            var idleTime = ActiveProcessMonitor.GetTimeSinceLastInput();
            switch (TimeUnit) {
                case TimeUnit.Milliseconds: return idleTime.TotalMilliseconds > InactiveTime;
                case TimeUnit.Seconds: return idleTime.TotalSeconds > InactiveTime;
                case TimeUnit.Minutes: return idleTime.TotalMinutes > InactiveTime;
                case TimeUnit.Hours: return idleTime.TotalHours > InactiveTime;
                default: return false;
            };
        }

        public override Evaluatable<bool> Clone() => new BooleanAwayTimer { InactiveTime = InactiveTime, TimeUnit = TimeUnit };
    }

    public enum TimeUnit { Milliseconds, Seconds, Minutes, Hours }
}
