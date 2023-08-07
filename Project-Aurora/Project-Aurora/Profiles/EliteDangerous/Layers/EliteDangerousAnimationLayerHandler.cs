using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.EliteDangerous.GSI;
using Aurora.Profiles.EliteDangerous.GSI.Nodes;
using Aurora.Settings.Layers;
using Aurora.Utils;

namespace Aurora.Profiles.EliteDangerous.Layers;

public enum EliteAnimation
{
    None,
    FsdCountdowm,
    Hyperspace,
    HyperspaceExit,
}
    
public class EliteDangerousAnimationHandlerProperties : LayerHandlerProperties2Color<EliteDangerousAnimationHandlerProperties>
{
    public EliteDangerousAnimationHandlerProperties()
    { }

    public EliteDangerousAnimationHandlerProperties(bool assign_default = false) : base(assign_default) { }

    public override void Default()
    {
        base.Default();
    }
}
public class EliteDangerousAnimationLayerHandler : LayerHandler<EliteDangerousAnimationHandlerProperties>
{
    private AnimationMix _fsdCountdownMix;
    private AnimationMix _hyperspaceMix;
    private AnimationMix _hypespaceExitMix;
    private StarClass _hyperspaceExitStar = StarClass.None;

    private static readonly Dictionary<StarClass, Color> _starColors = new()
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
        
    private long _previousTime = Time.GetMillisecondsSinceEpoch();
    private long _currentTime = Time.GetMillisecondsSinceEpoch();

    private float GetDeltaTime()
    {
        return (_currentTime - _previousTime) / 1000.0f;
    }

    private float _layerFadeState;
    private static float _totalAnimationTime, _animationKeyframe;
    private static EliteAnimation _currentAnimation = EliteAnimation.None;
    private EliteAnimation _animateOnce = EliteAnimation.None;
        
    public EliteDangerousAnimationLayerHandler() : base("Elite: Dangerous - Animations")
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

    private void BgFadeIn(EffectLayer animationLayer)
    {
        _layerFadeState = Math.Min(1, _layerFadeState + 0.07f);
        animationLayer.FillOver(ColorUtils.BlendColors(Color.Empty, Color.Black, _layerFadeState));
    }

    private void BgFadeOut(EffectLayer animationLayer)
    {
        if (!(_layerFadeState > 0)) return;
        _layerFadeState = Math.Max(0, _layerFadeState - 0.03f);
        animationLayer.FillOver(ColorUtils.BlendColors(Color.Empty, Color.Black, _layerFadeState));
    }

    public override EffectLayer Render(IGameState state)
    {
        var gameState = (GameState_EliteDangerous)state;

        _previousTime = _currentTime;
        _currentTime = Time.GetMillisecondsSinceEpoch();

        EffectLayer.Clear();
            
        if (gameState.Journal.ExitStarClass != StarClass.None)
        {
            RegenerateHyperspaceExitAnimation(gameState.Journal.ExitStarClass);
            gameState.Journal.ExitStarClass = StarClass.None;
            _animateOnce = EliteAnimation.HyperspaceExit;
            _totalAnimationTime = 0;
            _animationKeyframe = 0;
        }

        if (_animateOnce != EliteAnimation.None)
        {
            _currentAnimation = _animateOnce;
        } else if (gameState.Journal.fsdState == FSDState.Idle)
        {
            _currentAnimation = EliteAnimation.None;
        } else if (gameState.Journal.fsdState == FSDState.CountdownSupercruise || gameState.Journal.fsdState == FSDState.CountdownHyperspace)
        {
            _currentAnimation = EliteAnimation.FsdCountdowm;
        } else if (gameState.Journal.fsdState == FSDState.InHyperspace)
        {
            _currentAnimation = EliteAnimation.Hyperspace;
        }

        if (_currentAnimation == EliteAnimation.None)
        {
            _animationKeyframe = 0;
            _totalAnimationTime = 0;
        }

        if ((_currentAnimation != EliteAnimation.None && _currentAnimation != EliteAnimation.HyperspaceExit) || gameState.Journal.fsdWaitingSupercruise)
        {
            BgFadeIn(EffectLayer);
        }
        else if (_layerFadeState > 0)
        {
            BgFadeOut(EffectLayer);
        }

        float deltaTime = 0f, currentAnimationDuration = 0f;
        if(_currentAnimation == EliteAnimation.FsdCountdowm) {
            currentAnimationDuration = _fsdCountdownMix.GetDuration();
            _fsdCountdownMix.Draw(EffectLayer.GetGraphics(), _animationKeyframe);
            deltaTime = GetDeltaTime();
            _animationKeyframe += deltaTime;
        } else if (_currentAnimation == EliteAnimation.Hyperspace)
        {
            currentAnimationDuration = _hyperspaceMix.GetDuration();
            _hyperspaceMix.Draw(EffectLayer.GetGraphics(), _animationKeyframe);
            _hyperspaceMix.Draw(EffectLayer.GetGraphics(), findMod(_animationKeyframe + 1.2f, currentAnimationDuration));
            _hyperspaceMix.Draw(EffectLayer.GetGraphics(), findMod(_animationKeyframe + 2.8f, currentAnimationDuration));
            deltaTime = GetDeltaTime();
            //Loop the animation
            _animationKeyframe = findMod(_animationKeyframe + (deltaTime), currentAnimationDuration);
               
        } else if (_currentAnimation == EliteAnimation.HyperspaceExit)
        {
            currentAnimationDuration = _hypespaceExitMix.GetDuration();
            _hypespaceExitMix.Draw(EffectLayer.GetGraphics(), _animationKeyframe);
            deltaTime = GetDeltaTime();

            _animationKeyframe += deltaTime;
        }
            
        _totalAnimationTime += deltaTime;
        if (_totalAnimationTime > currentAnimationDuration)
        {
            _animateOnce = EliteAnimation.None;
        }

        return EffectLayer;
    }

    public override void SetApplication(Application profile)
    {
        (Control as Control_EliteDangerousAnimationLayer).SetProfile(profile);
        base.SetApplication(profile);
    }

    public void UpdateAnimations()
    {
        _fsdCountdownMix = new AnimationMix();
        _fsdCountdownMix.AddTrack(GenerateFsdPulse());
        _fsdCountdownMix.AddTrack(GenerateFsdPulse(1));
        _fsdCountdownMix.AddTrack(GenerateFsdPulse(2));
        _fsdCountdownMix.AddTrack(GenerateFsdPulse(3));
        _fsdCountdownMix.AddTrack(GenerateFsdPulse(4));
            
        _hyperspaceMix = new AnimationMix();
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak(0, 1.5f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 5, 0.1f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 12, 2.1f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 15, 2.7f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 20, 0.7f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 25, 2.4f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 30, 1.4f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 35, 0.3f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 40, 1.8f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 45, 1.0f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 50, 2.5f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 55, 1.5f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 60, 0.9f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 64, 2.3f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 68, 1.9f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 77, 0.0f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 82, 1.1f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 85, 1.3f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak((float)Effects.CanvasWidth / 100 * 93, 2.1f, _hyperspaceMix.GetTracks().Count));
        _hyperspaceMix.AddTrack(GenerateHyperspaceStreak(Effects.CanvasWidth, 0.4f, _hyperspaceMix.GetTracks().Count));
    }

    private void RegenerateHyperspaceExitAnimation(StarClass starClass = StarClass.K)
    {
        if (starClass == _hyperspaceExitStar) return;
            
        _hypespaceExitMix = new AnimationMix();
        float startingX = Effects.CanvasWidthCenter - 10;
        Color hyperspaceExitColor = _starColors.ContainsKey(starClass) ? _starColors[starClass] : _starColors[StarClass.K];
            
        AnimationTrack star_entry = new AnimationTrack("Hyperspace exit", 2.0f);
        star_entry.SetFrame(0.0f,
            new AnimationFilledCircle(startingX, Effects.CanvasHeightCenter, 0, hyperspaceExitColor)
        );
        star_entry.SetFrame(1.0f,
            new AnimationFilledCircle(startingX, Effects.CanvasHeightCenter, Effects.CanvasBiggest, hyperspaceExitColor)
        );
        star_entry.SetFrame(1.2f,
            new AnimationFilledCircle(startingX, Effects.CanvasHeightCenter, Effects.CanvasBiggest, hyperspaceExitColor)
        );
        star_entry.SetFrame(2f,
            new AnimationFilledCircle(startingX, Effects.CanvasHeightCenter, Effects.CanvasBiggest, Color.Empty)
        );
            
        AnimationTrack star_entry_bg = new AnimationTrack("Hyperspace exit bg", 2f);
        star_entry_bg.SetFrame(0.0f,
            new AnimationFill(Color.Black)
        );
        star_entry_bg.SetFrame(1.2f,
            new AnimationFill(Color.Black)
        );

        _hypespaceExitMix.AddTrack(star_entry_bg);
        _hypespaceExitMix.AddTrack(star_entry);
    }

    private AnimationTrack GenerateFsdPulse(int index = 0)
    {
        Color pulseStartColor = Color.FromArgb(0, 126, 255);
        Color pulseEndColor = Color.FromArgb(200, 0, 126, 255);

        float startingX = Effects.CanvasWidthCenter - 10;
        int pulseStartWidth = 10;
        int pulseEndWidth = 2;
            
        float pulseFrameDuration = 1;
        float pulseDuration = 0.7f;
            
        AnimationTrack countdown_pulse = new AnimationTrack("Fsd countdown pulse " + index, pulseFrameDuration, index);
        countdown_pulse.SetFrame(0.0f,
            new AnimationCircle(startingX, Effects.CanvasHeightCenter, 0, pulseStartColor, pulseStartWidth)
        );
        countdown_pulse.SetFrame(pulseDuration,
            new AnimationCircle(startingX, Effects.CanvasHeightCenter, Effects.CanvasBiggest, pulseEndColor, pulseEndWidth)
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
        int endPosition = Effects.CanvasHeight + streakSize * 2;
            
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