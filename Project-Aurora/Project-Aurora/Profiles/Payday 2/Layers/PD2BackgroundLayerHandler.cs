using Aurora.EffectsEngine;
using Aurora.Profiles.Payday_2.GSI;
using Aurora.Profiles.Payday_2.GSI.Nodes;
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

namespace Aurora.Profiles.Payday_2.Layers
{
    public class PD2BackgroundLayerHandlerProperties : LayerHandlerProperties2Color<PD2BackgroundLayerHandlerProperties>
    {
        public Color? _AmbientColor { get; set; }

        [JsonIgnore]
        public Color AmbientColor { get { return Logic._AmbientColor ?? _AmbientColor ?? Color.Empty; } }

        public Color? _AssaultColor { get; set; }

        [JsonIgnore]
        public Color AssaultColor { get { return Logic._AssaultColor ?? _AssaultColor ?? Color.Empty; } }

        public Color? _WintersColor { get; set; }

        [JsonIgnore]
        public Color WintersColor { get { return Logic._WintersColor ?? _WintersColor ?? Color.Empty; } }

        public float? _AssaultSpeedMultiplier { get; set; }

        [JsonIgnore]
        public float AssaultSpeedMultiplier { get { return Logic._AssaultSpeedMultiplier ?? _AssaultSpeedMultiplier ?? 0.0F; } }

        public Color? _AssaultFadeColor { get; set; }

        [JsonIgnore]
        public Color AssaultFadeColor { get { return Logic._AssaultFadeColor ?? _AssaultFadeColor ?? Color.Empty; } }

        public bool? _AssaultAnimationEnabled { get; set; }

        [JsonIgnore]
        public bool AssaultAnimationEnabled { get { return Logic._AssaultAnimationEnabled ?? _AssaultAnimationEnabled ?? false; } }

        public Color? _LowSuspicionColor { get; set; }

        [JsonIgnore]
        public Color LowSuspicionColor { get { return Logic._LowSuspicionColor ?? _LowSuspicionColor ?? Color.Empty; } }

        public Color? _MediumSuspicionColor { get; set; }

        [JsonIgnore]
        public Color MediumSuspicionColor { get { return Logic._MediumSuspicionColor ?? _MediumSuspicionColor ?? Color.Empty; } }

        public Color? _HighSuspicionColor { get; set; }

        [JsonIgnore]
        public Color HighSuspicionColor { get { return Logic._HighSuspicionColor ?? _HighSuspicionColor ?? Color.Empty; } }

        public bool? _ShowSuspicion { get; set; }

        [JsonIgnore]
        public bool ShowSuspicion { get { return Logic._ShowSuspicion ?? _ShowSuspicion ?? false; } }

        public PercentEffectType? _SuspicionEffectType { get; set; }

        public PercentEffectType SuspicionEffectType { get { return Logic._SuspicionEffectType ?? _SuspicionEffectType ?? PercentEffectType.AllAtOnce; } }

        public bool? _PeripheralUse { get; set; }

        [JsonIgnore]
        public bool PeripheralUse { get { return Logic._PeripheralUse ?? _PeripheralUse ?? false; } }


        public PD2BackgroundLayerHandlerProperties() : base() { }

        public PD2BackgroundLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            _AmbientColor = Color.FromArgb(158, 205, 255);
            _AssaultColor = Color.FromArgb(158, 205, 255);
            _WintersColor = Color.FromArgb(221, 99, 33);
            _AssaultSpeedMultiplier = 1.0f;
            _AssaultFadeColor = Color.FromArgb(255, 255, 255);
            _AssaultAnimationEnabled = true;
            _LowSuspicionColor = Color.FromArgb(0, 0, 0, 255);
            _MediumSuspicionColor = Color.FromArgb(255, 0, 0, 255);
            _HighSuspicionColor = Color.FromArgb(255, 255, 0, 0);
            _ShowSuspicion = true;
            _SuspicionEffectType = PercentEffectType.Progressive;
            _PeripheralUse = true;
        }

    }

    public class PD2BackgroundLayerHandler : LayerHandler<PD2BackgroundLayerHandlerProperties>
    {
        private float no_return_flashamount = 1.0f;
        private float no_return_timeleft;

        protected override UserControl CreateControl()
        {
            return new Control_PD2BackgroundLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer bg_layer = new EffectLayer("Payday 2 - Background");

            if (state is GameState_PD2)
            {

                GameState_PD2 pd2 = (GameState_PD2)state;

                Color bg_color = Properties.AmbientColor;

                long currenttime = Utils.Time.GetMillisecondsSinceEpoch();

                if ((pd2.Level.Phase == LevelPhase.Assault || pd2.Level.Phase == LevelPhase.Winters) && pd2.Game.State == GameStates.Ingame)
                {
                    if (pd2.Level.Phase == LevelPhase.Assault)
                        bg_color = Properties.AssaultColor;
                    else if (pd2.Level.Phase == LevelPhase.Winters)
                        bg_color = Properties.WintersColor;

                    double blend_percent = Math.Pow(Math.Sin(((currenttime % 1300L) / 1300.0D) * Properties.AssaultSpeedMultiplier * 2.0D * Math.PI), 2.0D);

                    bg_color = Utils.ColorUtils.BlendColors(Properties.AssaultFadeColor, bg_color, blend_percent);

                    if (Properties.AssaultAnimationEnabled)
                    {

                        Color effect_contours = Color.FromArgb(200, Color.Black);
                        float animation_stage_yoffset = 20.0f;
                        float animation_repeat_keyframes = 250.0f; //Effects.canvas_width * 2.0f;

                        /* Effect visual:

                        / /  ----  / /

                        */

                        /*
                         * !!!NOTE: TO BE REWORKED INTO ANIMATIONS!!!

                        EffectColorFunction line1_col_func = new EffectColorFunction(
                            new EffectLine(-1f, Effects.canvas_width + assault_yoffset + animation_stage_yoffset),
                            new ColorSpectrum(effect_contours),
                            2);

                        EffectColorFunction line2_col_func = new EffectColorFunction(
                            new EffectLine(-1f, Effects.canvas_width + assault_yoffset + 9.0f + animation_stage_yoffset),
                            new ColorSpectrum(effect_contours),
                            2);

                        EffectColorFunction line3_col_func = new EffectColorFunction(
                            new EffectLine(new EffectPoint(Effects.canvas_width + assault_yoffset + 17.0f + animation_stage_yoffset, Effects.canvas_height / 2.0f), new EffectPoint(Effects.canvas_width + assault_yoffset + 34.0f + animation_stage_yoffset, Effects.canvas_height / 2.0f), true),
                            new ColorSpectrum(effect_contours),
                            6);

                        EffectColorFunction line4_col_func = new EffectColorFunction(
                            new EffectLine(-1f, Effects.canvas_width + assault_yoffset + 52.0f + animation_stage_yoffset),
                            new ColorSpectrum(effect_contours),
                            2);

                        EffectColorFunction line5_col_func = new EffectColorFunction(
                            new EffectLine(-1f, Effects.canvas_width + assault_yoffset + 61.0f + animation_stage_yoffset),
                            new ColorSpectrum(effect_contours),
                            2);

                        assault_yoffset -= 0.50f;
                        assault_yoffset = assault_yoffset % animation_repeat_keyframes;

                        bg_layer.AddPostFunction(line1_col_func);
                        bg_layer.AddPostFunction(line2_col_func);
                        //bg_layer.AddPostFunction(line3_col_func);
                        bg_layer.AddPostFunction(line4_col_func);
                        bg_layer.AddPostFunction(line5_col_func);

                        */
                    }

                    bg_layer.Fill(bg_color);

                    if (Properties.PeripheralUse)
                        bg_layer.Set(Devices.DeviceKeys.Peripheral, bg_color);
                }
                else if (pd2.Level.Phase == LevelPhase.Stealth && pd2.Game.State == GameStates.Ingame)
                {
                    if (Properties.ShowSuspicion)
                    {
                        double percentSuspicious = ((double)pd2.Players.LocalPlayer.SuspicionAmount / (double)1.0);

                        ColorSpectrum suspicion_spec = new ColorSpectrum(Properties.LowSuspicionColor, Properties.HighSuspicionColor);
                        suspicion_spec.SetColorAt(0.5f, Properties.MediumSuspicionColor);

                        Settings.KeySequence suspicionSequence = new Settings.KeySequence(new Settings.FreeFormObject(0, 0, 1.0f / (Effects.editor_to_canvas_width / Effects.canvas_width), 1.0f / (Effects.editor_to_canvas_height / Effects.canvas_height)));

                        bg_layer.PercentEffect(suspicion_spec, suspicionSequence, percentSuspicious, 1.0D, Properties.SuspicionEffectType);

                        if (Properties.PeripheralUse)
                            bg_layer.Set(Devices.DeviceKeys.Peripheral, suspicion_spec.GetColorAt((float)percentSuspicious));
                    }
                }
                else if (pd2.Level.Phase == LevelPhase.Point_of_no_return && pd2.Game.State == GameStates.Ingame)
                {
                    ColorSpectrum no_return_spec = new ColorSpectrum(Color.Red, Color.Yellow);
                    if (pd2.Level.NoReturnTime != no_return_timeleft)
                    {
                        no_return_timeleft = pd2.Level.NoReturnTime;
                        no_return_flashamount = 1.0f;
                    }

                    Color no_return_color = no_return_spec.GetColorAt(no_return_flashamount);
                    no_return_flashamount -= 0.05f;

                    if (no_return_flashamount < 0.0f)
                        no_return_flashamount = 0.0f;

                    bg_layer.Fill(no_return_color);

                    if (Properties.PeripheralUse)
                        bg_layer.Set(Devices.DeviceKeys.Peripheral, no_return_color);
                }
                else
                {
                    bg_layer.Fill(bg_color);

                    if (Properties.PeripheralUse)
                        bg_layer.Set(Devices.DeviceKeys.Peripheral, bg_color);
                }

            }

            return bg_layer;
        }

        public override void SetApplication(Application profile)
        {
            (_Control as Control_PD2BackgroundLayer)?.SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}