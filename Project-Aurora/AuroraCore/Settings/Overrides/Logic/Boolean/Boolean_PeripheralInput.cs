using Aurora.Profiles;
using System;
using System.Diagnostics;
using System.Linq;
using Keys = System.Windows.Forms.Keys;

namespace Aurora.Settings.Overrides.Logic
{

    /// <summary>
    /// Condition that is true when a specific keyboard button is held down.
    /// </summary>
    [OverrideLogic("Key Held", category: OverrideLogicCategory.Input)]
    public class BooleanKeyDown : IEvaluatable<bool>
    {

        /// <summary>Creates a new key held condition with the default key (Space) as the trigger key.</summary>
        public BooleanKeyDown() { }
        /// <summary>Creates a new key held condition with the given key as the trigger key.</summary>
        public BooleanKeyDown(Keys target) { TargetKey = target; }

        /// <summary>The key to be checked to see if it is held down.</summary>
        public Keys TargetKey { get; set; } = Keys.Space;

        /// <summary>True if the global event bus's pressed key list contains the target key.</summary>
        public bool Evaluate(IGameState gameState) => AuroraCore.Instance.InputEvents.PressedKeys.Contains(TargetKey);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Do nothing - this is an application-independent condition.</summary>
        public void SetApplication(Application application) { }

        public IEvaluatable<bool> Clone() => new BooleanKeyDown { TargetKey = TargetKey };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    /// <summary>
    /// Condition that is true when a specific keyboard button is held down.
    /// </summary>
    [OverrideLogic("Key Press (Retain for duration)", category: OverrideLogicCategory.Input)]
    public class BooleanKeyDownWithTimer : IEvaluatable<bool>
    {
        private Stopwatch watch = new Stopwatch();

        /// <summary>Creates a new key held condition with the default key (Space) as the trigger key.</summary>
        public BooleanKeyDownWithTimer() { }
        /// <summary>Creates a new key held condition with the given key as the trigger key.</summary>
        public BooleanKeyDownWithTimer(Keys target, float seconds) : this() { TargetKey = target; Seconds = seconds; }

        /// <summary>The key to be checked to see if it is held down.</summary>
        public Keys TargetKey { get; set; } = Keys.Space;
        public float Seconds { get; set; } = 1;

        /// <summary>True if the global event bus's pressed key list contains the target key.</summary>
        public bool Evaluate(IGameState gameState)
        {
            if (AuroraCore.Instance.InputEvents.PressedKeys.Contains(TargetKey))
            {
                watch.Restart();
                return true;
            }
            else if (watch.IsRunning && watch.Elapsed.TotalSeconds <= Seconds)
            {
                return true;
            }
            else
                watch.Stop();

            return false;
        }
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Do nothing - this is an application-independent condition.</summary>
        public void SetApplication(Application application) { }

        public IEvaluatable<bool> Clone() => new BooleanKeyDown { TargetKey = TargetKey };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    /// <summary>
    /// Condition that is true when a specific mouse button is held down.
    /// </summary>
    [OverrideLogic("Mouse Button Held", category: OverrideLogicCategory.Input)]
    public class BooleanMouseDown : IEvaluatable<bool>
    {

        /// <summary>Creates a new key held condition with the default mouse button (Left) as the trigger button.</summary>
        public BooleanMouseDown() { }
        /// <summary>Creates a new key held condition with the given mouse button as the trigger button.</summary>
        public BooleanMouseDown(System.Windows.Forms.MouseButtons target) { TargetButton = target; }

        /// <summary>The mouse button to be checked to see if it is held down.</summary>
        public System.Windows.Forms.MouseButtons TargetButton { get; set; } = System.Windows.Forms.MouseButtons.Left;

        /// <summary>True if the global event bus's pressed mouse button list contains the target button.</summary>
        public bool Evaluate(IGameState gameState) => AuroraCore.Instance.InputEvents.PressedButtons.Contains(TargetButton);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Do nothing - this is an application-independent condition.</summary>
        public void SetApplication(Application application) { }

        public IEvaluatable<bool> Clone() => new BooleanMouseDown { TargetButton = TargetButton };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    /// <summary>
    /// Condition that is true when the specified lock key (e.g. caps lock) is active.
    /// </summary>
    [OverrideLogic("Lock Key Active", category: OverrideLogicCategory.Input)]
    public class BooleanLockKeyActive : IEvaluatable<bool>
    {

        /// <summary>Creates a new key held condition with the default lock type (CapsLock) as the lock type.</summary>
        public BooleanLockKeyActive() { }
        /// <summary>Creates a new key held condition with the given button as the lock type button.</summary>
        public BooleanLockKeyActive(Keys target) { TargetKey = target; }

        public Keys TargetKey { get; set; } = Keys.CapsLock;

        /// <summary>Return true if the target lock key is active.</summary>
        public bool Evaluate(IGameState gameState) => System.Windows.Forms.Control.IsKeyLocked(TargetKey);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Do nothing - this is an application-independent condition.</summary>
        public void SetApplication(Application application) { }

        public IEvaluatable<bool> Clone() => new BooleanLockKeyActive { TargetKey = TargetKey };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
}
