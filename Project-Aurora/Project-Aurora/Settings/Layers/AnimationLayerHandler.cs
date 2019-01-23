using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles;
using Aurora.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    public class AnimationLayerHandlerProperties : LayerHandlerProperties2Color<AnimationLayerHandlerProperties>
    {
        public AnimationMix _AnimationMix { get; set; }

        [JsonIgnore]
        public AnimationMix AnimationMix { get { return Logic._AnimationMix ?? _AnimationMix; } }

        public bool? _forceKeySequence { get; set; }

        [JsonIgnore]
        public bool ForceKeySequence { get { return Logic._forceKeySequence ?? _forceKeySequence ?? false; } }

        public bool? _scaleToKeySequenceBounds { get; set; }

        [JsonIgnore]
        public bool ScaleToKeySequenceBounds { get { return Logic._scaleToKeySequenceBounds ?? _scaleToKeySequenceBounds ?? false; } }

        public float? _AnimationDuration { get; set; }

        [JsonIgnore]
        public float AnimationDuration { get { return Logic._AnimationDuration ?? _AnimationDuration ?? 0.0f; } }

        public int? _AnimationRepeat { get; set; }

        [JsonIgnore]
        public int AnimationRepeat { get { return Logic._AnimationRepeat ?? _AnimationRepeat ?? 0; } }

        [JsonIgnore]
        public AnimationTriggerMode TriggerMode => Logic._TriggerMode ?? _TriggerMode ?? AnimationTriggerMode.AlwaysOn;
        public AnimationTriggerMode? _TriggerMode { get; set; }

        [JsonIgnore]
        public AnimationStackMode StackMode => Logic._StackMode ?? _StackMode ?? AnimationStackMode.Ignore;
        public AnimationStackMode? _StackMode { get; set; }

        [JsonIgnore]
        public string TriggerPath => Logic._TriggerPath ?? _TriggerPath ?? string.Empty;
        public string _TriggerPath { get; set; }

        [JsonIgnore]
        public bool TriggerAnyKey => Logic._TriggerAnyKey ?? _TriggerAnyKey ?? false;
        public bool? _TriggerAnyKey { get; set; }

        [JsonIgnore]
        public KeySequence TriggerKeySequence => Logic._TriggerKeySequence ?? _TriggerKeySequence ?? new KeySequence { };
        public KeySequence _TriggerKeySequence { get; set; }

        [JsonIgnore]
        public bool KeyTriggerTranslate => Logic._KeyTriggerTranslate ?? _KeyTriggerTranslate ?? false;
        public bool? _KeyTriggerTranslate { get; set; }

        [JsonIgnore]
        public bool WhileKeyHeldTerminateRunning => Logic._WhileKeyHeldTerminateRunning ?? _WhileKeyHeldTerminateRunning ?? false;
        public bool? _WhileKeyHeldTerminateRunning { get; set; }

        public AnimationLayerHandlerProperties() : base() { }
        public AnimationLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default() {
            base.Default();
            this._AnimationMix = new AnimationMix();
            this._forceKeySequence = false;
            this._scaleToKeySequenceBounds = false;
            this._AnimationDuration = 1;
            this._AnimationRepeat = 0;
            this._TriggerMode = AnimationTriggerMode.AlwaysOn;
            this._StackMode = AnimationStackMode.Ignore;
            this._TriggerPath = "";
            this._TriggerKeySequence = new KeySequence();
            this._TriggerAnyKey = false;
            this._KeyTriggerTranslate = false;
            this._WhileKeyHeldTerminateRunning = false;
        }
    }

    public class AnimationLayerHandler : LayerHandler<AnimationLayerHandlerProperties> {

        private List<RunningAnimation> runningAnimations = new List<RunningAnimation>();
        private Stopwatch _animTimeStopwatch = new Stopwatch();
        private bool _alwaysOnHasPlayed = false; // A dedicated variable has to be used to make 'Always On' work with the repeat count since the logic has changed
        private double _previousTriggerDoubleValue; // Used for tracking when a numeric gamestate value changes
        private bool _previousTriggerBoolValue; // Used for tracking when a boolean gamestate value changes
        private HashSet<DeviceKeys> _pressedKeys = new HashSet<DeviceKeys>(); // A list of pressed keys. Used to ensure that the key down event only fires for each key when it first goes down, not as it's held

        public AnimationLayerHandler() {
            _ID = "Animation";

            // Listen for key events for the key-based triggers
            Global.InputEvents.KeyDown += InputEvents_KeyDown;
            Global.InputEvents.KeyUp += InputEvents_KeyUp;
        }

        protected override UserControl CreateControl() {
            return new Control_AnimationLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate) {
            EffectLayer animationLayer = new EffectLayer();

            // Calculate elapsed time since last Render call
            long dt = _animTimeStopwatch.ElapsedMilliseconds;
            _animTimeStopwatch.Restart();

            // Update all running animations. We have to call "ToList()" to prevent "Collection was modified; enumeration operation may not execute"
            runningAnimations.ToList().ForEach(anim => {
                anim.currentTime += dt / 1000f;
                if (Properties.AnimationRepeat > 0)
                    anim.playTimes += (int)(anim.currentTime / Properties.AnimationDuration);
                anim.currentTime %= Properties.AnimationDuration;
            });

            // Remove any animations that have completed their play times
            if (Properties.AnimationRepeat > 0)
                runningAnimations.RemoveAll(ra => ra.playTimes >= Properties.AnimationRepeat);

            // Check to see if the gamestate will cause any animations to trigger
            CheckTriggers(gamestate);

            // Render each playing animation. We have to call "ToList()" to prevent "Collection was modified; enumeration operation may not execute"
            runningAnimations.ToList().ForEach(anim => {
                EffectLayer temp = new EffectLayer();

                // Default values for the destination rect (the area that the canvas is drawn to) and animation offset
                Rectangle destRect = new Rectangle(0, 0, Effects.canvas_width, Effects.canvas_height);
                PointF offset = Properties.KeyTriggerTranslate ? anim.offset : PointF.Empty;

                // When ScaleToKeySequenceBounds is true, additional calculations are needed on the destRect and offset:
                if (Properties.ScaleToKeySequenceBounds) {
                    // The dest rect should simply be the bounding region of the affected keys
                    RectangleF affectedRegion = Properties.Sequence.GetAffectedRegion();
                    destRect = Rectangle.Truncate(affectedRegion);

                    // If we are scaling to key sequence bounds, we need to adapt the offset of the pressed key so that it
                    // remains where it is after the bound - scaling operation.
                    // Let's consider only 1 dimension (X) for now since it makes it easier to think about. The scaling process
                    // is: the whole canvas width is scaled down to the width of the affected region, and then it offset by the
                    // X of the affected region. To have a point that remains the same, we need to reposition it when it's being
                    // used on the canvas, therefore this process needs to be inverted: 1.take the original offset of X and
                    // subtract the affected region's X, thereby giving us the distance from the edge of the affected region to
                    // the offset; 2. scale this up to counter-act the down-scaling done, so we calculate the change in scale off
                    // the canvas by dividing canvas width by the affected region's width; 3.multiply these two numbers together
                    // and that's our new X offset.
                    // This probably makes no sense and I'll forget how it works immediately, but hopefully it helps a little in
                    // future if this code ever needs to be revised. It's embarassing how long it took to work this equation out.
                    offset.X = (offset.X - affectedRegion.X) * (Effects.canvas_width / affectedRegion.Width);
                    offset.Y = (offset.Y - affectedRegion.Y) * (Effects.canvas_height / affectedRegion.Height);
                }

                // Draw the animation to a temporary canvas
                using (Graphics g = temp.GetGraphics())
                    Properties.AnimationMix.Draw(g, anim.currentTime, 1f, offset);
                
                // Draw from this temp canvas to the actual layer, performing the scale down if it's needed.
                using (Graphics g = animationLayer.GetGraphics())
                    g.DrawImage(temp.GetBitmap(), destRect, new Rectangle(0, 0, Effects.canvas_width, Effects.canvas_height), GraphicsUnit.Pixel);

                temp.Dispose();
            });

            if (Properties.ForceKeySequence)
                animationLayer.OnlyInclude(Properties.Sequence);

            return animationLayer;
        }

        /// <summary>
        /// Checks the current gamestate and checks if the animation layer should be triggered.
        /// Note will also have the side-effect of updating _previousTriggerValue so this should not be called
        /// more than once per frame.
        /// </summary>
        private void CheckTriggers(IGameState gamestate) {
            if (Properties.TriggerMode == AnimationTriggerMode.AlwaysOn) {
                // Should return true if it has not already been played OR it is allowed to repeat indefinately
                // Should also not try to get a value from the state
                if (Properties.AnimationRepeat == 0)
                    StartAnimation(); // Always true if infinite repeats
                else if (!_alwaysOnHasPlayed) {
                    _alwaysOnHasPlayed = true; // True if it has not been played
                    StartAnimation();
                }

            // Handling for key-based triggers
            } else if (Properties.TriggerMode == AnimationTriggerMode.WhileKeyHeld) {
                // If we are in "while held down" mode, check to see if any of the keys pressed do not currently
                // have an animation with them as the assigned key. If not, create trigger it
                foreach (var key in _pressedKeys.Where(k => !runningAnimations.Any(a => a.assignedKey == k)))
                    StartAnimation(key);

            // Handling for numeric value change based triggers
            } else if (IsTriggerNumericValueBased(Properties.TriggerMode)) {
                // Check to see if a gamestate value change should trigger the animation
                double resolvedTriggerValue = GameStateUtils.TryGetDoubleFromState(gamestate, Properties.TriggerPath);
                switch (Properties.TriggerMode) {
                    case AnimationTriggerMode.OnChange:
                        if (resolvedTriggerValue != _previousTriggerDoubleValue)
                            StartAnimation();
                        break;
                    case AnimationTriggerMode.OnHigh:
                        if (resolvedTriggerValue > _previousTriggerDoubleValue)
                            StartAnimation();
                        break;
                    case AnimationTriggerMode.OnLow:
                        if (resolvedTriggerValue < _previousTriggerDoubleValue)
                            StartAnimation();
                        break;
                }
                _previousTriggerDoubleValue = resolvedTriggerValue;
            
            // Handling for boolean value based triggers
            } else {
                bool resolvedTriggerValue = GameStateUtils.TryGetBoolFromState(gamestate, Properties.TriggerPath);
                switch (Properties.TriggerMode) {
                    case AnimationTriggerMode.OnTrue:
                        if (resolvedTriggerValue && !_previousTriggerBoolValue)
                            StartAnimation();
                        break;
                    case AnimationTriggerMode.OnFalse:
                        if (!resolvedTriggerValue && _previousTriggerBoolValue)
                            StartAnimation();
                        break;
                    case AnimationTriggerMode.WhileTrue:
                        if (resolvedTriggerValue && runningAnimations.Count == 0)
                            StartAnimation();
                        break;
                }
                _previousTriggerBoolValue = resolvedTriggerValue;
            }
        }

        /// <summary>
        /// Triggers a new animation to play depending on the StackMode setting.
        /// </summary>
        /// <param name="targetKey">The key to center the animation around.</param>
        private void StartAnimation(DeviceKeys targetKey = default(DeviceKeys)) {
            RunningAnimation anim = null; // Store a reference to the new animation (or the restarted one)
            if (runningAnimations.Count == 0)
                // If there are no running animations, we will always start a new one
                runningAnimations.Add(anim = new RunningAnimation());

            else if (Properties.TriggerMode != AnimationTriggerMode.AlwaysOn) // Ignore stack/reset when animation is always on
                // If there are already running animations, exactly what happens depends on StackMode
                switch (Properties.StackMode) {
                    case AnimationStackMode.Reset: anim = runningAnimations[0].Reset(); break;
                    case AnimationStackMode.Stack: runningAnimations.Add(anim = new RunningAnimation()); break;
                }

            // If a new animation has been started or an existing one restarted, and we are translating based on key press
            // assign the target ket to the animation to allow it to calculate the offset.
            if (anim != null)
                anim.assignedKey = targetKey;
        }

        public override void SetApplication(Application profile) {
            // Check to ensure the property specified actually exists
            if (profile != null && !string.IsNullOrWhiteSpace(Properties._TriggerPath) && !profile.ParameterLookup.ContainsKey(Properties._TriggerPath))
                Properties._TriggerPath = string.Empty;

            // Tell the control to update (will update the combobox with the possible variable paths)
            (Control as Control_AnimationLayer).SetProfile(profile);

            base.SetApplication(profile);
        }

        /// <summary>
        /// Event handler for when keys are pressed.
        /// </summary>
        private void InputEvents_KeyDown(object sender, SharpDX.RawInput.KeyboardInputEventArgs e) {
            // Skip handler if not waiting for a key-related trigger to save memory/CPU time
            if (!IsTriggerKeyBased(Properties.TriggerMode)) return;

            // If triggering on any key or the pressed key is in the trigger list AND the pressed key has not already been handled (i.e. it's not being held)
            if ((Properties.TriggerAnyKey || Properties.TriggerKeySequence.keys.Contains(e.GetDeviceKey())) && !_pressedKeys.Contains(e.GetDeviceKey())) {
                // Start an animation if trigger is for 'press' event
                if (Properties.TriggerMode == AnimationTriggerMode.OnKeyPress)
                    StartAnimation(e.GetDeviceKey());
                // Mark it as handled
                _pressedKeys.Add(e.GetDeviceKey());
            }
        }

        /// <summary>
        /// Event handler for when keys are released.
        /// </summary>
        private void InputEvents_KeyUp(object sender, SharpDX.RawInput.KeyboardInputEventArgs e) {
            // Skip handler if not waiting for a key-related trigger to save memory/CPU time
            if (!IsTriggerKeyBased(Properties.TriggerMode)) return;

            // If the pressed list contains the now released key (ensures we don't trigger on a key not in the sequence)
            if (_pressedKeys.Contains(e.GetDeviceKey())) {
                // Start animation if trigger is for 'release' event
                if (Properties.TriggerMode == AnimationTriggerMode.OnKeyRelease)
                    StartAnimation(e.GetDeviceKey());
                // Remove it from the pressed keys so it can be re-detected by the KeyDown event handler
                _pressedKeys.Remove(e.GetDeviceKey());
            }

            // If we are in "while key held" mode and the user wishes to immediately terminate animations for a key when that key
            // is released (instead of letting the animation finish first), remove any animations assigned to the given key.
            if ((Properties.TriggerMode == AnimationTriggerMode.OnKeyPress || Properties.TriggerMode == AnimationTriggerMode.WhileKeyHeld) && Properties.WhileKeyHeldTerminateRunning)
                runningAnimations.RemoveAll(anim => anim.assignedKey == e.GetDeviceKey());
        }

        /// <summary>
        /// Returns true if the given AnimationTrigger mode is a numeric gamestate value-related trigger (OnHigh, OnLow or OnChange)
        /// </summary>
        public static bool IsTriggerNumericValueBased(AnimationTriggerMode m) {
            return new[] { AnimationTriggerMode.OnHigh, AnimationTriggerMode.OnLow, AnimationTriggerMode.OnChange }.Contains(m);
        }

        /// <summary>
        /// Returns true if the given AnimationTrigger mode is a boolean gamestate value-related trigger (OnTrue, OnFalse or WhileTrue)
        /// </summary>
        public static bool IsTriggerBooleanValueBased(AnimationTriggerMode m) {
            return new[] { AnimationTriggerMode.OnTrue, AnimationTriggerMode.OnFalse, AnimationTriggerMode.WhileTrue }.Contains(m);
        }

        /// <summary>
        /// Returns true if the given AnimationTrigger mode is a key-related trigger (OnKeyPress or OnKeyRelease)
        /// </summary>
        public static bool IsTriggerKeyBased(AnimationTriggerMode m) {
            return new[] { AnimationTriggerMode.OnKeyPress, AnimationTriggerMode.OnKeyRelease, AnimationTriggerMode.WhileKeyHeld }.Contains(m);
        }

        /// <summary>
        /// A tiny data class just to store information about
        /// currently running animations.
        /// </summary>
        class RunningAnimation {
            public float currentTime = 0;
            public int playTimes = 0;
            public DeviceKeys assignedKey = DeviceKeys.NONE;
            public PointF offset => assignedKey == DeviceKeys.NONE ? PointF.Empty : Effects.GetBitmappingFromDeviceKey(assignedKey).Center;

            public RunningAnimation Reset() {
                currentTime = 0;
                playTimes = 0;
                return this;
            }
        }
    }

    /// <summary>
    /// An enum of the possible ways for an animation to trigger.
    /// </summary>
    public enum AnimationTriggerMode {
        [Description("Always on (disable trigger)")]
        AlwaysOn,

        [Description("On value increase")]
        OnHigh,
        [Description("On value decrease")]
        OnLow,
        [Description("On value change")]
        OnChange,

        [Description("On boolean become true")]
        OnTrue,
        [Description("On boolean become false")]
        OnFalse,
        [Description("While boolean true")]
        WhileTrue,

        [Description("On key pressed")]
        OnKeyPress,
        [Description("On key released")]
        OnKeyRelease,
        [Description("While key held")]
        WhileKeyHeld
    }

    /// <summary>
    /// An enum dictating what should happen if a trigger happens while
    /// an animation is already in progress.
    /// </summary>
    public enum AnimationStackMode {
        [Description("Ignore")]
        Ignore,
        [Description("Restart")]
        Reset,
        [Description("Play multiple")]
        Stack
    }
}
