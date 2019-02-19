using Aurora.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Keys = System.Windows.Forms.Keys;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Condition that is true when a specific keyboard button is held down.
    /// </summary>
    [OverrideLogic("Key Held", category: OverrideLogicCategory.Input)]
    public class BooleanKeyDown : IEvaluatableBoolean {

        /// <summary>Creates a new key held condition with the default key (Space) as the trigger key.</summary>
        public BooleanKeyDown() { }
        /// <summary>Creates a new key held condition with the given key as the trigger key.</summary>
        public BooleanKeyDown(Keys target) { TargetKey = target; }

        /// <summary>The key to be checked to see if it is held down.</summary>
        public Keys TargetKey { get; set; } = Keys.Space;

        /// <summary>Create a control where the user can select the key they wish to detect.</summary>
        public Visual GetControl(Application application) {
            var c = new Controls.Control_FieldPresenter { Type = typeof(Keys), Margin = new System.Windows.Thickness(0, 0, 0, 6) };
            c.SetBinding(Controls.Control_FieldPresenter.ValueProperty, new Binding("TargetKey") { Source = this, Mode = BindingMode.TwoWay });
            return c;
        }

        /// <summary>True if the global event bus's pressed key list contains the target key.</summary>
        public bool Evaluate(IGameState gameState) => Global.InputEvents.PressedKeys.Contains(TargetKey);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Do nothing - this is an application-independent condition.</summary>
        public void SetApplication(Application application) { }

        public IEvaluatableBoolean Clone() => new BooleanKeyDown { TargetKey = TargetKey };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
    

    /// <summary>
    /// Condition that is true when a specific mouse button is held down.
    /// </summary>
    [OverrideLogic("Mouse Button Held", category: OverrideLogicCategory.Input)]
    public class BooleanMouseDown : IEvaluatableBoolean {

        /// <summary>Creates a new key held condition with the default mouse button (Left) as the trigger button.</summary>
        public BooleanMouseDown() { }
        /// <summary>Creates a new key held condition with the given mouse button as the trigger button.</summary>
        public BooleanMouseDown(System.Windows.Forms.MouseButtons target) { TargetButton = target; }

        /// <summary>The mouse button to be checked to see if it is held down.</summary>
        public System.Windows.Forms.MouseButtons TargetButton { get; set; } = System.Windows.Forms.MouseButtons.Left;
        
        /// <summary>Create a control where the user can select the mouse button they wish to detect.</summary>
        public Visual GetControl(Application application) {
            var c = new Controls.Control_FieldPresenter { Type = typeof(System.Windows.Forms.MouseButtons), Margin = new System.Windows.Thickness(0, 0, 0, 6) };
            c.SetBinding(Controls.Control_FieldPresenter.ValueProperty, new Binding("TargetButton") { Source = this, Mode = BindingMode.TwoWay });
            return c;
        }

        /// <summary>True if the global event bus's pressed mouse button list contains the target button.</summary>
        public bool Evaluate(IGameState gameState) => Global.InputEvents.PressedButtons.Contains(TargetButton);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Do nothing - this is an application-independent condition.</summary>
        public void SetApplication(Application application) { }

        public IEvaluatableBoolean Clone() => new BooleanMouseDown { TargetButton = TargetButton };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    /// <summary>
    /// Condition that is true when the specified lock key (e.g. caps lock) is active.
    /// </summary>
    [OverrideLogic("Lock Key Active", category: OverrideLogicCategory.Input)]
    public class BooleanLockKeyActive : IEvaluatableBoolean {

        /// <summary>Creates a new key held condition with the default lock type (CapsLock) as the lock type.</summary>
        public BooleanLockKeyActive() { }
        /// <summary>Creates a new key held condition with the given button as the lock type button.</summary>
        public BooleanLockKeyActive(Keys target) { TargetKey = target; }

        public Keys TargetKey { get; set; } = Keys.CapsLock;

        /// <summary>Create a control allowing the user to specify which lock key to check.</summary>
        public Visual GetControl(Application application) {
            var cb = new ComboBox { ItemsSource = new[] { Keys.CapsLock, Keys.NumLock, Keys.Scroll } };
            cb.SetBinding(ComboBox.SelectedValueProperty, new Binding("TargetKey") { Source = this, Mode=BindingMode.TwoWay });
            return cb;
        }

        /// <summary>Return true if the target lock key is active.</summary>
        public bool Evaluate(IGameState gameState) => System.Windows.Forms.Control.IsKeyLocked(TargetKey);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Do nothing - this is an application-independent condition.</summary>
        public void SetApplication(Application application) { }

        public IEvaluatableBoolean Clone() => new BooleanLockKeyActive { TargetKey = TargetKey };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
}
