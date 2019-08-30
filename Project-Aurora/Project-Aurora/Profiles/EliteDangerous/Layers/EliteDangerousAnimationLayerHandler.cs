using Aurora.EffectsEngine;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.EliteDangerous.GSI;
using Aurora.Profiles.EliteDangerous.GSI.Nodes;

namespace Aurora.Profiles.EliteDangerous.Layers
{
    public class EliteDangerousAnimationHandlerProperties : LayerHandlerProperties2Color<EliteDangerousAnimationHandlerProperties>
    {
        public EliteDangerousAnimationHandlerProperties() : base() { }

        public EliteDangerousAnimationHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
        }
    }
    public class EliteDangerousAnimationLayerHandler : LayerHandler<EliteDangerousAnimationHandlerProperties>
    {
        enum EliteAnimation
        {
            None,
            FsdCountdowm,
            Hyperspace
        }
        private AnimationMix fsd_countdown_mix;
        private AnimationMix hyperspace_mix;
        
        private long previousTime = 0;
        private long currentTime = 0;

        private float getDeltaTime()
        {
            return (currentTime - previousTime) / 1000.0f;
        }
        
        private static float animationKeyframe = 0.0f;
        private static EliteAnimation currentAnimation = EliteAnimation.None;
        private static float animationTime = 0.0f;
        
        public EliteDangerousAnimationLayerHandler() : base()
        {
            _ID = "EliteDangerousAnimations";
        }

        protected override UserControl CreateControl()
        {
            return new Control_EliteDangerousAnimationLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            GameState_EliteDangerous gameState = state as GameState_EliteDangerous;
            
            previousTime = currentTime;
            currentTime = Utils.Time.GetMillisecondsSinceEpoch();

            if (currentTime - previousTime > 300000 || (currentTime == 0 && previousTime == 0))
                UpdateAnimations();
            
            EffectLayer animation_layer = new EffectLayer("Elite: Dangerous - Animations");
            
            if (gameState.Journal.fsdState == FSDState.Idle)
            {
                animationKeyframe = 0;
                currentAnimation = EliteAnimation.None;
            } else if (gameState.Journal.fsdState != FSDState.Idle)
            {
                currentAnimation = EliteAnimation.FsdCountdowm;
            }

            if (currentAnimation != EliteAnimation.None)
            {
                animation_layer.Fill(Color.Black);
            }
            
            if(currentAnimation == EliteAnimation.FsdCountdowm) {
                fsd_countdown_mix.Draw(animation_layer.GetGraphics(), animationKeyframe);
                animationKeyframe += getDeltaTime();
            } else if (currentAnimation == EliteAnimation.Hyperspace)
            {
                hyperspace_mix.Draw(animation_layer.GetGraphics(), animationKeyframe);
                animationKeyframe += getDeltaTime();
            }

            return animation_layer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_EliteDangerousAnimationLayer).SetProfile(profile);
            base.SetApplication(profile);
        }

        public void UpdateAnimations()
        {
            fsd_countdown_mix = new AnimationMix();
            Color pulseStartColor = Color.FromArgb(0, 126, 255);
            Color pulseEndColor = Color.FromArgb(200, 0, 126, 255);

            float startingX = 42F;
            int pulseStartWidth = 10;
            int pulseEndWidth = 2;
            float animationDuration = 0.7f;
            
            AnimationTrack countdown_pulse_1 = new AnimationTrack("Fsd countdown pulse 1", animationDuration);
            countdown_pulse_1.SetFrame(0.0f,
                new AnimationCircle(startingX, Effects.canvas_height_center, 0, pulseStartColor, pulseStartWidth)
            );
            countdown_pulse_1.SetFrame(animationDuration,
                new AnimationCircle(startingX, Effects.canvas_height_center, Effects.canvas_biggest, pulseEndColor, pulseEndWidth)
            );
            
            AnimationTrack countdown_pulse_2 = new AnimationTrack("Fsd countdown pulse 2", animationDuration, 1);
            countdown_pulse_2.SetFrame(0.0f,
                new AnimationCircle(startingX, Effects.canvas_height_center, 0, pulseStartColor, pulseStartWidth)
            );
            countdown_pulse_2.SetFrame(animationDuration,
                new AnimationCircle(startingX, Effects.canvas_height_center, Effects.canvas_biggest, pulseEndColor, pulseEndWidth)
            );
            
            AnimationTrack countdown_pulse_3 = new AnimationTrack("Fsd countdown pulse 3", animationDuration, 2);
            countdown_pulse_3.SetFrame(0.0f,
                new AnimationCircle(startingX, Effects.canvas_height_center, 0, pulseStartColor, pulseStartWidth)
            );
            countdown_pulse_3.SetFrame(animationDuration,
                new AnimationCircle(startingX, Effects.canvas_height_center, Effects.canvas_biggest, pulseEndColor, pulseEndWidth)
            );
            
            AnimationTrack countdown_pulse_4 = new AnimationTrack("Fsd countdown pulse 4", animationDuration, 3);
            countdown_pulse_4.SetFrame(0.0f,
                new AnimationCircle(startingX, Effects.canvas_height_center, 0, pulseStartColor, pulseStartWidth)
            );
            countdown_pulse_4.SetFrame(animationDuration,
                new AnimationCircle(startingX, Effects.canvas_height_center, Effects.canvas_biggest, pulseEndColor, pulseEndWidth)
            );
            
            AnimationTrack countdown_pulse_5 = new AnimationTrack("Fsd countdown pulse 5", animationDuration, 4);
            countdown_pulse_5.SetFrame(0.0f,
                new AnimationCircle(startingX, Effects.canvas_height_center, 0, pulseStartColor, pulseStartWidth)
            );
            countdown_pulse_5.SetFrame(animationDuration,
                new AnimationCircle(startingX, Effects.canvas_height_center, Effects.canvas_biggest, pulseEndColor, pulseEndWidth)
            );

            fsd_countdown_mix.AddTrack(countdown_pulse_1);
            fsd_countdown_mix.AddTrack(countdown_pulse_2);
            fsd_countdown_mix.AddTrack(countdown_pulse_3);
            fsd_countdown_mix.AddTrack(countdown_pulse_4);
            fsd_countdown_mix.AddTrack(countdown_pulse_5);
        }
    }
}
