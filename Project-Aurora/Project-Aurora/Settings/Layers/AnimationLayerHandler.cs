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
        public Keybind[] TriggerKeybinds => Logic._TriggerKeys ?? _TriggerKeys ?? new Keybind[] { };
        public Keybind[] _TriggerKeys { get; set; }

        [JsonIgnore]
        public bool KeyTriggerTranslate => Logic._KeyTriggerTranslate ?? _KeyTriggerTranslate ?? false;
        public bool? _KeyTriggerTranslate { get; set; }

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
            this._TriggerKeys = new Keybind[] { };
            this._TriggerAnyKey = false;
            this._KeyTriggerTranslate = false;
        }
    }

    public class AnimationLayerHandler : LayerHandler<AnimationLayerHandlerProperties> {

        private List<RunningAnimation> runningAnimations = new List<RunningAnimation>();
        private Stopwatch _animTimeStopwatch = new Stopwatch();
        private bool _alwaysOnHasPlayed = false; // A dedicated variable has to be used to make 'Always On' work with the repeat count since the logic has changed
        private double _previousTriggerValue; // Used for tracking when a gamestate value changes
        private bool _awaitingTrigger; // Used to track when an animation should be triggered and hasn't been yet
        private PointF _awaitingOffset; // Used to track the desired offset for the next trigger (used for key related triggers)
        private HashSet<System.Windows.Forms.Keys> _pressedKeys = new HashSet<System.Windows.Forms.Keys>(); // A list of pressed keys. Used to ensure that the key down event only fires for each key when it first goes down, not as it's held
        private HashSet<Keybind> _pressedKeybinds = new HashSet<Keybind>(); // A list of pressed keybinds... ^^

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

            // Update all running animations
            runningAnimations.ForEach(anim => {
                anim.currentTime += dt / 1000f;
                if (Properties.AnimationRepeat > 0)
                    anim.playTimes += (int)(anim.currentTime / Properties.AnimationDuration);
                anim.currentTime %= Properties.AnimationDuration;
            });

            // Remove any animations that have completed their play times
            if (Properties.AnimationRepeat > 0)
                runningAnimations.RemoveAll(ra => ra.playTimes >= Properties.AnimationRepeat);

            // Check to see if the gamestate will cause any animations to trigger
            if (IsTriggered(gamestate))
                TriggerAnimation();

            // Render each playing animation
            runningAnimations.ForEach(anim => {
                EffectLayer temp = new EffectLayer();

                // Default values for the destination rect (the area that the canvas is drawn to) and animation offset
                Rectangle destRect = new Rectangle(0, 0, Effects.canvas_width, Effects.canvas_height);
                PointF offset = anim.offset;

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
        private bool IsTriggered(IGameState gamestate) {
            if (Properties.TriggerMode == AnimationTriggerMode.AlwaysOn) {
                // Should return true if it has not already been played OR it is allowed to repeat indefinately
                // Should also not try to get a value from the state
                if (Properties.AnimationRepeat == 0)
                    return true; // Always true if infinite repeats
                else if (!_alwaysOnHasPlayed)
                    return (_alwaysOnHasPlayed = true); // True if it has not been played
                return false; // Otherwise false if it has been played

            // Handling for key-based triggers
            } else if (new[] { AnimationTriggerMode.OnKeyPress, AnimationTriggerMode.OnKeyRelease }.Contains(Properties.TriggerMode)) {
                // If there are keys on the trigger list that have been pressed/released, set the trigger to true
                if (_awaitingTrigger) {
                    _awaitingTrigger = false;
                    return true;
                }
                return false;

                // Handling for gamestate-change-based triggers
            } else {
                // Check to see if a gamestate value change should trigger the animation
                double resolvedTriggerValue = Utils.GameStateUtils.TryGetDoubleFromState(gamestate, Properties.TriggerPath);
                bool shouldTrigger = false;
                switch (Properties.TriggerMode) {
                    case AnimationTriggerMode.OnChange: shouldTrigger = resolvedTriggerValue != _previousTriggerValue; break;
                    case AnimationTriggerMode.OnHigh: shouldTrigger = resolvedTriggerValue > _previousTriggerValue; break;
                    case AnimationTriggerMode.OnLow: shouldTrigger = resolvedTriggerValue < _previousTriggerValue; break;
                }
                _previousTriggerValue = resolvedTriggerValue;
                return shouldTrigger;
            }
        }

        /// <summary>
        /// Triggers a new animation to play depending on the StackMode setting.
        /// </summary>
        private void TriggerAnimation() {
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
            if (Properties.KeyTriggerTranslate && anim != null)
                anim.offset = _awaitingOffset;
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
            if (Properties.TriggerMode != AnimationTriggerMode.OnKeyRelease && Properties.TriggerMode != AnimationTriggerMode.OnKeyPress) return;

            if (Properties.TriggerAnyKey) { // ANY-KEY-TRIGGERS MODE
                // If the pressed key has not already been handled (i.e. it's not being held)
                if (!_pressedKeys.Contains(e.Key)) {
                    // Do a trigger if waiting for a 'press' event
                    if (Properties.TriggerMode == AnimationTriggerMode.OnKeyPress) {
                        _awaitingTrigger = true;
                        _awaitingOffset = Effects.GetBitmappingFromDeviceKey(e.GetDeviceKey()).Center;
                    }
                    // Mark it as handled
                    _pressedKeys.Add(e.Key);
                }


            } else { //KEYBIND MODE
                // Find if any keybind has been pressed (and was not previously pressed, i.e. not in _pressedKeybinds)
                Keybind activedKeybind = Properties.TriggerKeybinds
                    .FirstOrDefault(kb => kb.IsPressed() && !_pressedKeybinds.Contains(kb));

                // If there is a new keybind that is pressed, show a flag that we are awaiting a trigger (if waiting on 'press')
                // and add the keybind to the pressed list so it doesn't re-trigger until it has been released.
                if (activedKeybind != null) {
                    if (Properties.TriggerMode == AnimationTriggerMode.OnKeyPress) {
                        _awaitingTrigger = true;
                        _awaitingOffset = Effects.GetBitmappingFromDeviceKey(e.GetDeviceKey()).Center;
                    }
                    _pressedKeybinds.Add(activedKeybind);
                }
            }
        }

        /// <summary>
        /// Event handler for when keys are released.
        /// </summary>
        private void InputEvents_KeyUp(object sender, SharpDX.RawInput.KeyboardInputEventArgs e) {
            // Skip handler if not waiting for a key-related trigger to save memory/CPU time
            if (Properties.TriggerMode != AnimationTriggerMode.OnKeyRelease && Properties.TriggerMode != AnimationTriggerMode.OnKeyPress) return;

            if (Properties.TriggerAnyKey) { // ANY-KEY-TRIGGERS MODE
                // Do a trigger if waiting for a 'release' event
                if (Properties.TriggerMode == AnimationTriggerMode.OnKeyRelease) {
                    _awaitingTrigger = true;
                    _awaitingOffset = Effects.GetBitmappingFromDeviceKey(e.GetDeviceKey()).Center;
                }
                // Remove it from the pressed keys so it can be re-detected by the KeyDown event handler
                _pressedKeys.Remove(e.Key);

            } else { //KEYBIND MODE
                // Find if any currently pressed keybinds are now no longer pressed
                Keybind deactivatedKeybind = _pressedKeybinds
                .FirstOrDefault(kb => !kb.IsPressed());

                // If there is a keybind that is no longer pressed, remove it from the pressed list. Also, if we are waiting
                // on a 'release', set the flag for doing a key trigger.
                if (deactivatedKeybind != null) {
                    if (Properties.TriggerMode == AnimationTriggerMode.OnKeyRelease) {
                        _awaitingTrigger = true;
                        _awaitingOffset = Effects.GetBitmappingFromDeviceKey(e.GetDeviceKey()).Center;
                    }
                    _pressedKeybinds.Remove(deactivatedKeybind);
                }
            }
        }

        /// <summary>
        /// A tiny data class just to store information about
        /// currently running animations.
        /// </summary>
        class RunningAnimation {
            public float currentTime = 0;
            public int playTimes = 0;
            public PointF offset = PointF.Empty;

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

        [Description("On key pressed")]
        OnKeyPress,
        [Description("On key released")]
        OnKeyRelease
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
