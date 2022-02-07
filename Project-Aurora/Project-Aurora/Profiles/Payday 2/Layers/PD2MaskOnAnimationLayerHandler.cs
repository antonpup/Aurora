using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Profiles.CSGO.GSI.Nodes;
using Aurora.Profiles.Payday_2.GSI;
using Aurora.Profiles.Payday_2.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.Payday_2.Layers
{
    public class PD2MaskOnAnimationLayerHandler : LayerHandler
    {
        private readonly AnimationTrack[] tracks =
        {
            new AnimationTrack("Track 0", 1.0f, 0.0f)
        };

        private long previoustime = 0;
        private long currenttime = 0;

        private static float effectKeyframe = 0.0f;
        private const float effectAnimationTime = 1.0f;

        private bool showAnimation = false;
        private bool animationRun = false;

        protected override UserControl CreateControl()
        {
            return new Control_PD2MaskOnAnimationLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            previoustime = currenttime;
            currenttime = Utils.Time.GetMillisecondsSinceEpoch();

            EffectLayer effectLayer = new EffectLayer("PD2 - Mask On Animation");
            AnimationMix animationMix = new AnimationMix();

            if (state is GameState_PD2 gameState_PD2)
            {
                if (animationRun && (gameState_PD2.LocalPlayer.State == PlayerState.Civilian || gameState_PD2.LocalPlayer.State == PlayerState.Undefined || gameState_PD2.LocalPlayer.State == PlayerState.Mask_Off))
                {
                    animationRun = false;
                }

                if ((gameState_PD2.LocalPlayer.State == PlayerState.Standard) && !animationRun && !showAnimation)
                {
                    this.SetTracks(PD2Colours.PlayerState_Standard);
                    animationMix.Clear();
                    showAnimation = true;
                }

                if (showAnimation)
                {
                    animationMix = new AnimationMix(tracks);

                    effectLayer.Fill(PD2Colours.PlayerState_Civilian);
                    animationMix.Draw(effectLayer.GetGraphics(), effectKeyframe);
                    effectKeyframe += (currenttime - previoustime) / 1000.0f;

                    if (effectKeyframe >= effectAnimationTime)
                    {
                        showAnimation = false;
                        animationRun = true;
                        effectKeyframe = 0;
                    }
                }
            }

            return effectLayer;
        }

        public override void SetApplication(Application profile)
        {
            base.SetApplication(profile);
        }

        private void SetTracks(Color color)
        {
            for (int i = 0; i < tracks.Length; i++)
            {
                tracks[i].SetFrame(
                    0.0f,
                    new AnimationFilledCircle(
                        (int)(Effects.canvas_width_center * 0.9),
                        Effects.canvas_height_center,
                        0,
                        PD2Colours.GameTheme,
                        4)
                );

                tracks[i].SetFrame(
                    1.0f,
                    new AnimationFilledCircle(
                        (int)(Effects.canvas_width_center * 0.9),
                        Effects.canvas_height_center,
                        Effects.canvas_biggest / 2.0f,
                        PD2Colours.GameTheme,
                        4)
                );
            }
        }
    }
}