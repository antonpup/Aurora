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
using Aurora.Utils;

namespace Aurora.Profiles.EliteDangerous.Layers
{
    public enum EliteAnimation
    {
        None,
        FsdCountdowm,
        Hyperspace,
        HyperspaceExit,
    }
    
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
        private AnimationMix fsd_countdown_mix;
        private AnimationMix hyperspace_mix;
        private AnimationMix hypespace_exit_mix;
        private StarClass hyperspace_exit_star = StarClass.None;
        
        public static Dictionary<StarClass, Color> starColors = new Dictionary<StarClass, Color>()
        {
            {StarClass.O, Color.FromArgb(132, 187, 255)},
            {StarClass.B, Color.FromArgb(191, 255, 251)},
            {StarClass.A, Color.FromArgb(255, 255, 255)},
            {StarClass.F, Color.FromArgb(254, 250, 98)},
            {StarClass.G, Color.FromArgb(255, 140, 0)},
            {StarClass.K, Color.FromArgb(255, 140, 0)},
            {StarClass.M, Color.FromArgb(255, 67, 26)},
            {StarClass.L, Color.FromArgb(255, 0, 0)},
            {StarClass.T, Color.FromArgb(148, 0, 44)}, 
            {StarClass.Y, Color.FromArgb(148, 0, 44)},
            // TTS, AeBe, - same as K?
            {StarClass.W, Color.FromArgb(180, 180, 255)},
            {StarClass.WN, Color.FromArgb(180, 180, 255)},
            // WNC, - same as K?
            {StarClass.WC, Color.FromArgb(180, 180, 255)},
            {StarClass.WO, Color.FromArgb(180, 180, 255)},
            // CS, - unknown
            {StarClass.C, Color.FromArgb(230, 0, 0)},
            // CN, CJ, CH, CHd, - same as K
            // MS, S, - same as K
            {StarClass.D, Color.FromArgb(255, 255, 255)},
            {StarClass.DA, Color.FromArgb(255, 255, 255)},
            {StarClass.DAB, Color.FromArgb(255, 255, 255)},
            {StarClass.DAO, Color.FromArgb(255, 255, 255)},
            {StarClass.DAZ, Color.FromArgb(255, 255, 255)},
            {StarClass.DAV, Color.FromArgb(255, 255, 255)},
            {StarClass.DB, Color.FromArgb(255, 255, 255)},
            {StarClass.DBZ, Color.FromArgb(255, 255, 255)},
            {StarClass.DBV, Color.FromArgb(255, 255, 255)},
            {StarClass.DO, Color.FromArgb(255, 255, 255)},
            {StarClass.DOV, Color.FromArgb(255, 255, 255)},
            {StarClass.DQ, Color.FromArgb(255, 255, 255)},
            {StarClass.DC, Color.FromArgb(255, 255, 255)},
            {StarClass.DCV, Color.FromArgb(255, 255, 255)},
            {StarClass.DX, Color.FromArgb(255, 255, 255)},
                          
            {StarClass.N, Color.FromArgb(156, 202, 255)},
            {StarClass.H, Color.FromArgb(0, 0, 40)},
            // X, - exotic (whatever that means)
        };
        
        private long previousTime = Time.GetMillisecondsSinceEpoch();
        private long currentTime = Time.GetMillisecondsSinceEpoch();

        private float getDeltaTime()
        {
            return (currentTime - previousTime) / 1000.0f;
        }

        private float layerFadeState = 0;
        private static float totalAnimationTime, animationKeyframe = 0.0f;
        private static EliteAnimation currentAnimation = EliteAnimation.None;
        private static float animationTime = 0.0f;
        private EliteAnimation animateOnce = EliteAnimation.None;
        
        public EliteDangerousAnimationLayerHandler() : base()
        {
            UpdateAnimations();
            RegenerateHyperspaceExitAnimation();
        }

        protected override UserControl CreateControl()
        {
            return new Control_EliteDangerousAnimationLayer(this);
        }
        
        static float findMod(float a, float b) 
        { 
          
            // Handling negative values 
            if (a < 0) 
                a = -a; 
            if (b < 0) 
                b = -b; 
      
            // Finding mod by repeated subtraction 
            float mod = a; 
            while (mod >= b) 
                mod = mod - b; 
      
            // Sign of result typically depends 
            // on sign of a. 
            if (a < 0) 
                return -mod; 
      
            return mod; 
        }

        private void BgFadeIn(EffectLayer animation_layer)
        {
            layerFadeState = Math.Min(1, layerFadeState + 0.07f);
            animation_layer.Fill(ColorUtils.BlendColors(Color.Empty, Color.Black, layerFadeState));
        }

        private void BgFadeOut(EffectLayer animation_layer)
        {
            if (!(layerFadeState > 0)) return;
            layerFadeState = Math.Max(0, layerFadeState - 0.03f);
            animation_layer.Fill(ColorUtils.BlendColors(Color.Empty, Color.Black, layerFadeState));
        }

        public override EffectLayer Render(IGameState state)
        {
            GameState_EliteDangerous gameState = state as GameState_EliteDangerous;

            previousTime = currentTime;
            currentTime = Time.GetMillisecondsSinceEpoch();

            EffectLayer animation_layer = new EffectLayer("Elite: Dangerous - Animations");
            
            if (gameState.Journal.ExitStarClass != StarClass.None)
            {
                RegenerateHyperspaceExitAnimation(gameState.Journal.ExitStarClass);
                gameState.Journal.ExitStarClass = StarClass.None;
                animateOnce = EliteAnimation.HyperspaceExit;
                totalAnimationTime = 0;
                animationKeyframe = 0;
            }

            if (animateOnce != EliteAnimation.None)
            {
                currentAnimation = animateOnce;
            } else if (gameState.Journal.fsdState == FSDState.Idle)
            {
                currentAnimation = EliteAnimation.None;
            } else if (gameState.Journal.fsdState == FSDState.CountdownSupercruise || gameState.Journal.fsdState == FSDState.CountdownHyperspace)
            {
                currentAnimation = EliteAnimation.FsdCountdowm;
            } else if (gameState.Journal.fsdState == FSDState.InHyperspace)
            {
                currentAnimation = EliteAnimation.Hyperspace;
            }

            if (currentAnimation == EliteAnimation.None)
            {
                animationKeyframe = 0;
                totalAnimationTime = 0;
            }

            if ((currentAnimation != EliteAnimation.None && currentAnimation != EliteAnimation.HyperspaceExit) || gameState.Journal.fsdWaitingSupercruise)
            {
                BgFadeIn(animation_layer);
            }
            else if (layerFadeState > 0)
            {
                BgFadeOut(animation_layer);
            }

            float deltaTime = 0f, currentAnimationDuration = 0f;
            if(currentAnimation == EliteAnimation.FsdCountdowm) {
                currentAnimationDuration = fsd_countdown_mix.GetDuration();
                fsd_countdown_mix.Draw(animation_layer.GetGraphics(), animationKeyframe);
                deltaTime = getDeltaTime();
                animationKeyframe += deltaTime;
            } else if (currentAnimation == EliteAnimation.Hyperspace)
            {
                currentAnimationDuration = hyperspace_mix.GetDuration();
                hyperspace_mix.Draw(animation_layer.GetGraphics(), animationKeyframe);
                hyperspace_mix.Draw(animation_layer.GetGraphics(), findMod(animationKeyframe + 1.2f, currentAnimationDuration));
                hyperspace_mix.Draw(animation_layer.GetGraphics(), findMod(animationKeyframe + 2.8f, currentAnimationDuration));
                deltaTime = getDeltaTime();
                //Loop the animation
                animationKeyframe = findMod(animationKeyframe + (deltaTime), currentAnimationDuration);
               
            } else if (currentAnimation == EliteAnimation.HyperspaceExit)
            {
                currentAnimationDuration = hypespace_exit_mix.GetDuration();
                hypespace_exit_mix.Draw(animation_layer.GetGraphics(), animationKeyframe);
                deltaTime = getDeltaTime();

                animationKeyframe += deltaTime;
            }
            
            totalAnimationTime += deltaTime;
            if (totalAnimationTime > currentAnimationDuration)
            {
                animateOnce = EliteAnimation.None;
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
            fsd_countdown_mix.AddTrack(GenerateFsdPulse());
            fsd_countdown_mix.AddTrack(GenerateFsdPulse(1));
            fsd_countdown_mix.AddTrack(GenerateFsdPulse(2));
            fsd_countdown_mix.AddTrack(GenerateFsdPulse(3));
            fsd_countdown_mix.AddTrack(GenerateFsdPulse(4));
            
            hyperspace_mix = new AnimationMix();
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 0, 1.5f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 5, 0.1f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 12, 2.1f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 15, 2.7f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 20, 0.7f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 25, 2.4f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 30, 1.4f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 35, 0.3f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 40, 1.8f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 45, 1.0f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 50, 2.5f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 55, 1.5f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 60, 0.9f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 64, 2.3f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 68, 1.9f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 77, 0.0f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 82, 1.1f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 85, 1.3f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 93, 2.1f, hyperspace_mix.GetTracks().Count));
            hyperspace_mix.AddTrack(GenerateHyperspaceStreak(Effects.canvas_width / 100 * 100, 0.4f, hyperspace_mix.GetTracks().Count));
        }

        private void RegenerateHyperspaceExitAnimation(StarClass starClass = StarClass.K)
        {
            if (starClass == hyperspace_exit_star) return;
            
            hypespace_exit_mix = new AnimationMix();
            float startingX = Effects.canvas_width_center - 10;
            Color hyperspaceExitColor = starColors.ContainsKey(starClass) ? starColors[starClass] : starColors[StarClass.K];
            
            AnimationTrack star_entry = new AnimationTrack("Hyperspace exit", 2.0f);
            star_entry.SetFrame(0.0f,
                new AnimationFilledCircle(startingX, Effects.canvas_height_center, 0, hyperspaceExitColor, 1)
            );
            star_entry.SetFrame(1.0f,
                new AnimationFilledCircle(startingX, Effects.canvas_height_center, Effects.canvas_biggest, hyperspaceExitColor, 1)
            );
            star_entry.SetFrame(1.2f,
                new AnimationFilledCircle(startingX, Effects.canvas_height_center, Effects.canvas_biggest, hyperspaceExitColor, 1)
            );
            star_entry.SetFrame(2f,
                new AnimationFilledCircle(startingX, Effects.canvas_height_center, Effects.canvas_biggest, Color.Empty, 1)
            );
            
            AnimationTrack star_entry_bg = new AnimationTrack("Hyperspace exit bg", 2f);
            star_entry_bg.SetFrame(0.0f,
                new AnimationFill(Color.Black)
            );
            star_entry_bg.SetFrame(1.2f,
                new AnimationFill(Color.Black)
            );

            hypespace_exit_mix.AddTrack(star_entry_bg);
            hypespace_exit_mix.AddTrack(star_entry);
        }

        private AnimationTrack GenerateFsdPulse(int index = 0)
        {
            Color pulseStartColor = Color.FromArgb(0, 126, 255);
            Color pulseEndColor = Color.FromArgb(200, 0, 126, 255);

            float startingX = Effects.canvas_width_center - 10;
            int pulseStartWidth = 10;
            int pulseEndWidth = 2;
            
            float pulseFrameDuration = 1;
            float pulseDuration = 0.7f;
            
            AnimationTrack countdown_pulse = new AnimationTrack("Fsd countdown pulse " + index, pulseFrameDuration, index);
            countdown_pulse.SetFrame(0.0f,
                new AnimationCircle(startingX, Effects.canvas_height_center, 0, pulseStartColor, pulseStartWidth)
            );
            countdown_pulse.SetFrame(pulseDuration,
                new AnimationCircle(startingX, Effects.canvas_height_center, Effects.canvas_biggest, pulseEndColor, pulseEndWidth)
            );

            return countdown_pulse;
        }
        
        private AnimationTrack GenerateHyperspaceStreak(float xOffset, float timeShift, int index = 0)
        {
            Color streakEndColor = Color.FromArgb(178, 217, 255);
            Color streakStartColor = Color.FromArgb(0, 64, 135);

            float animationDuration = 0.8f; 
            int streakSize = 7;
            int streakWidth = 3;

            int startPosition = -40;
            int endPosition = Effects.canvas_height + streakSize * 2;
            
            AnimationTrack streak = new AnimationTrack("Hyperspace streak " + index, animationDuration, timeShift);
            streak.SetFrame(0.0f,
                new AnimationLine(new PointF(xOffset, startPosition), new PointF(xOffset, startPosition + streakSize), streakStartColor, streakEndColor, streakWidth)
            );
            streak.SetFrame(animationDuration,
                new AnimationLine(new PointF(xOffset, endPosition), new PointF(xOffset, endPosition + streakSize), streakStartColor, streakEndColor, streakWidth)
            );

            return streak;
        }
    }
}
