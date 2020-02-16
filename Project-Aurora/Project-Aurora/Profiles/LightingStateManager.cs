using Aurora.EffectsEngine;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Profiles
{
    public interface ILightingEngine
    {
        void ActiveProfileChanged(ILightEvent profile);
        void RefreshOverLayerProfiles(List<ILightEvent> profiles);
    }

    public class LightingStateManager : ILightingEngine, IInit
    {
        private ILightEvent ActiveProfile;
        private List<ILightEvent> OverlayProfiles = new List<ILightEvent>();

        private Timer updateTimer;

        private int timerInterval = 33;

        private long currentTick;


        public event EventHandler PreUpdate;
        public event EventHandler PostUpdate;

        private List<TimedListObject> TimedLayers = new List<TimedListObject>();

        public LightingStateManager(ILightEvent profile)
        {
            ActiveProfile = profile;
        }
        public bool Initialized { get; private set; }

        public bool Initialize()
        {
            if (Initialized)
                return true;

            this.InitUpdate();

            Initialized = true;
            return Initialized;
        }

        private void InitUpdate()
        {
            updateTimer = new System.Threading.Timer(g => {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                if (Global.isDebug)
                    Update();
                else
                {
                    try
                    {
                        Update();
                    }
                    catch (Exception exc)
                    {
                        Global.logger.Error("ProfilesManager.Update() Exception, " + exc);
                    }
                }

                watch.Stop();
                currentTick += timerInterval + watch.ElapsedMilliseconds;
                updateTimer?.Change(Math.Max(timerInterval, 0), Timeout.Infinite);
            }, null, 0, System.Threading.Timeout.Infinite);
            GC.KeepAlive(updateTimer);
        }

        private void RefreshLightningFrame()
        {
            EffectsEngine.EffectFrame newFrame = new EffectsEngine.EffectFrame();

            if (ActiveProfile.IsEnabled)
            {
                ActiveProfile.UpdateLights(newFrame);
            }
            foreach (ILightEvent prof in OverlayProfiles)
            {
                if (prof.IsOverlayEnabled)
                    prof.UpdateOverlayLights(newFrame);
            }
            if (ActiveProfile.IsEnabled)
            {
                ActiveProfile.UpdateOverlayLights(newFrame);
            }

            var timedLayers = TimedLayers.ToList();
            var layers = new Queue<EffectLayer>(timedLayers.Select(l => ((Layer)l.item).Render(null)));
            newFrame.AddOverlayLayers(layers.ToArray());

            Global.effengine.PushFrame(newFrame);
        }

        private void Update()
        {
            PreUpdate?.Invoke(this, null);

            //Blackout. TODO: Cleanup this a bit. Maybe push blank effect frame to keyboard incase it has existing stuff displayed
            if ((Global.Configuration.time_based_dimming_enabled &&
               Utils.Time.IsCurrentTimeBetween(Global.Configuration.time_based_dimming_start_hour, Global.Configuration.time_based_dimming_start_minute, Global.Configuration.time_based_dimming_end_hour, Global.Configuration.time_based_dimming_end_minute)))
            {
                return;
            }

            Global.dev_manager.InitializeOnce();

            timerInterval = 1000 / Global.Configuration.FrameRate;


            RefreshLightningFrame();

            PostUpdate?.Invoke(this, null);
        }

        public void Dispose()
        {
            updateTimer.Dispose();
            updateTimer = null;
        }

        public void ActiveProfileChanged(ILightEvent profile)
        {
            if (Global.Configuration.ProfileChangeAnimation)
            {
                AddOverlayForDuration(new Layer("Profile Close Helper Layer", new GradientFillLayerHandler()
                {
                    Properties = new GradientFillLayerHandlerProperties()
                    {
                        _FillEntireKeyboard = true,
                        _GradientConfig = new LayerEffectConfig(Color.FromArgb(0, 0, 0, 0), Color.FromArgb(255, 0, 0, 0)) { AnimationType = AnimationType.Translate_XY, speed = 10 }

                    }
                }), 600);
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(450);

                    ActiveProfile = profile;
                    AddOverlayForDuration(new Layer("Profile Open Helper Layer", new GradientFillLayerHandler()
                    {
                        Properties = new GradientFillLayerHandlerProperties()
                        {
                            _FillEntireKeyboard = true,
                            _GradientConfig = new LayerEffectConfig(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(0, 0, 0, 0)) { AnimationType = AnimationType.Translate_XY, speed = 10 }
                        }
                    }), 900);

                });
            }
            else
            {
                ActiveProfile = profile;
            }
        }

        public void RefreshOverLayerProfiles(List<ILightEvent> profiles)
        {
            OverlayProfiles = profiles;
        }

        /// <summary>KeyDown handler that checks the current application's profiles for keybinds.
        /// In the case of multiple profiles matching the keybind, it will pick the next one as specified in the Application.Profile order.</summary>
        public void CheckProfileKeybinds(object sender, SharpDX.RawInput.KeyboardInputEventArgs e)
        {
            ILightEvent profile = ActiveProfile;

            // Check profile is valid and do not switch profiles if the user is trying to enter a keybind
            if (profile is Application && Controls.Control_Keybind._ActiveKeybind == null)
            {

                // Find all profiles that have their keybinds pressed
                List<ApplicationProfile> possibleProfiles = new List<ApplicationProfile>();
                foreach (var prof in (profile as Application).Profiles)
                    if (prof.TriggerKeybind.IsPressed())
                        possibleProfiles.Add(prof);

                // If atleast one profile has it's key pressed
                if (possibleProfiles.Count > 0)
                {
                    // The target profile is the NEXT valid profile after the currently selected one (or the first valid one if the currently selected one doesn't share this keybind)
                    int trg = (possibleProfiles.IndexOf((profile as Application).Profile) + 1) % possibleProfiles.Count;
                    (profile as Application).SwitchToProfile(possibleProfiles[trg]);
                }
            }
        }
        public void AddOverlayForDuration(Layer overlay_event, int duration)
        {
            TimedLayers.Add(new TimedListObject(overlay_event, duration, TimedLayers));
        }

    }
}
