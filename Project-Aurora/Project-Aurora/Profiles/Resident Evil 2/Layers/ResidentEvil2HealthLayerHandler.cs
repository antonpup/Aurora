using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.ResidentEvil2.GSI;
using Aurora.Profiles.ResidentEvil2.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.ResidentEvil2.Layers
{
    public class ResidentEvil2HealthLayerHandlerProperties : LayerHandlerProperties2Color<ResidentEvil2HealthLayerHandlerProperties>
    {
        public enum HealthDisplayType
        {
            Static = 0,
            Scanning = 1
        }

        public HealthDisplayType? _DisplayType { get; set; }

        [JsonIgnore]
        public HealthDisplayType DisplayType { get { return Logic._DisplayType ?? _DisplayType ?? HealthDisplayType.Static; } }

        public ResidentEvil2HealthLayerHandlerProperties() : base() { }

        public ResidentEvil2HealthLayerHandlerProperties(bool assign_default = false) : base( assign_default ) { }

        public override void Default()
        {
            base.Default();
            this._DisplayType = HealthDisplayType.Static;
        }

    }

    public class ResidentEvil2HealthLayerHandler : LayerHandler<ResidentEvil2HealthLayerHandlerProperties>
    {
        private long previoustime = 0;
        private long currenttime = 0;

        private static float heartbeat_keyframe = 0.0f;
        private static float heartbeat_animationTime = 1.0f;

        private static float[] animationTimes = { 4.0f, 3.0f, 2.0f };
        private static float[] breakTimes = { 2.0f, 1.0f, 1.0f };
        private static float[] fullAnimTimes = { animationTimes[0] + breakTimes[0], animationTimes[1] + breakTimes[1], animationTimes[2] + breakTimes[2] };

        static AnimationTrack hbFine = new AnimationTrack("HBScan", fullAnimTimes[0])
            .SetFrame(0, new AnimationFilledRectangle(-5, Effects.canvas_height_center, 20, Effects.canvas_height, Color.Green))
            .SetFrame(animationTimes[0], new AnimationFilledRectangle(125, Effects.canvas_height_center, 20, Effects.canvas_height, Color.Green))
            .SetFrame(breakTimes[0], new AnimationFill(Color.Black));
        static AnimationTrack hbLiteFine = new AnimationTrack("HBScan", fullAnimTimes[0])
            .SetFrame(0, new AnimationFilledRectangle(-5, Effects.canvas_height_center, 20, Effects.canvas_height, Color.YellowGreen))
            .SetFrame(animationTimes[0], new AnimationFilledRectangle(125, Effects.canvas_height_center, 20, Effects.canvas_height, Color.YellowGreen))
            .SetFrame(breakTimes[0], new AnimationFill(Color.Black));
        static AnimationTrack hbCaution = new AnimationTrack("HBScan", fullAnimTimes[1])
            .SetFrame(0, new AnimationFilledRectangle(-5, Effects.canvas_height_center, 20, Effects.canvas_height, Color.Gold))
            .SetFrame(animationTimes[1], new AnimationFilledRectangle(125, Effects.canvas_height_center, 20, Effects.canvas_height, Color.Gold))
            .SetFrame(breakTimes[1], new AnimationFill(Color.Black));
        static AnimationTrack hbDanger = new AnimationTrack("HBScan", fullAnimTimes[2])
            .SetFrame(0, new AnimationFilledRectangle(-5, Effects.canvas_height_center, 20, Effects.canvas_height, Color.Red))
            .SetFrame(animationTimes[2], new AnimationFilledRectangle(125, Effects.canvas_height_center, 20, Effects.canvas_height, Color.Red))
            .SetFrame(breakTimes[2], new AnimationFill(Color.Black));

        AnimationMix mixFine = new AnimationMix(new[] { hbFine });
        AnimationMix mixLiteFine = new AnimationMix(new[] { hbLiteFine });
        AnimationMix mixCaution = new AnimationMix(new[] { hbCaution });
        AnimationMix mixDanger = new AnimationMix(new[] { hbDanger });

        protected override UserControl CreateControl()
        {
            return new Control_ResidentEvil2HealthLayer( this );
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer bg_layer = new EffectLayer("Resident Evil 2 - Health");

            if (state is GameState_ResidentEvil2)
            {
                if (Properties.DisplayType == ResidentEvil2HealthLayerHandlerProperties.HealthDisplayType.Static)
                {
                    GameState_ResidentEvil2 re2state = state as GameState_ResidentEvil2;

                    switch (re2state.Player.Status)
                    {
                        case Player_ResidentEvil2.PlayerStatus.Fine:
                            bg_layer.Fill(Color.Green);
                            break;
                        case Player_ResidentEvil2.PlayerStatus.LiteFine:
                            bg_layer.Fill(Color.YellowGreen);
                            break;
                        case Player_ResidentEvil2.PlayerStatus.Caution:
                            bg_layer.Fill(Color.Gold);
                            break;
                        case Player_ResidentEvil2.PlayerStatus.Danger:
                            bg_layer.Fill(Color.Red);
                            break;
                        case Player_ResidentEvil2.PlayerStatus.Dead:
                            bg_layer.Fill(Color.DarkGray);
                            break;
                        default:
                            bg_layer.Fill(Color.DarkSlateBlue);
                            break;
                    }

                    return bg_layer;
                }
                else if (Properties.DisplayType == ResidentEvil2HealthLayerHandlerProperties.HealthDisplayType.Scanning)
                {
                    previoustime = currenttime;
                    currenttime = Utils.Time.GetMillisecondsSinceEpoch();

                    GameState_ResidentEvil2 re2state = state as GameState_ResidentEvil2;

                    switch (re2state.Player.Status)
                    {
                        case Player_ResidentEvil2.PlayerStatus.Fine:
                            bg_layer.Fill(Color.FromArgb(8, Color.Green.R, Color.Green.G, Color.Green.B));
                            heartbeat_animationTime = fullAnimTimes[0];
                            mixFine.Draw(bg_layer.GetGraphics(), heartbeat_keyframe);
                            break;
                        case Player_ResidentEvil2.PlayerStatus.LiteFine:
                            bg_layer.Fill(Color.FromArgb(8, Color.YellowGreen.R, Color.YellowGreen.G, Color.YellowGreen.B));
                            heartbeat_animationTime = fullAnimTimes[0];
                            mixLiteFine.Draw(bg_layer.GetGraphics(), heartbeat_keyframe);
                            break;
                        case Player_ResidentEvil2.PlayerStatus.Caution:
                            bg_layer.Fill(Color.FromArgb(8, Color.Gold.R, Color.Gold.G, Color.Gold.B));
                            heartbeat_animationTime = fullAnimTimes[1];
                            mixCaution.Draw(bg_layer.GetGraphics(), heartbeat_keyframe);
                            break;
                        case Player_ResidentEvil2.PlayerStatus.Danger:
                            bg_layer.Fill(Color.FromArgb(8, Color.Red.R, Color.Red.G, Color.Red.B));
                            heartbeat_animationTime = fullAnimTimes[2];
                            mixDanger.Draw(bg_layer.GetGraphics(), heartbeat_keyframe);
                            break;
                        case Player_ResidentEvil2.PlayerStatus.Dead:
                            bg_layer.Fill(Color.DarkGray);
                            break;
                        default:
                            bg_layer.Fill(Color.DarkSlateBlue);
                            break;
                    }

                    heartbeat_keyframe += (currenttime - previoustime) / 1000.0f;

                    if (heartbeat_keyframe >= heartbeat_animationTime)
                    {
                        heartbeat_keyframe = 0;
                    }

                    return bg_layer;
                }
                else
                {
                    return bg_layer;
                }
            }
            else return bg_layer;
        }

        public override void SetApplication(Application profile)
        {
            ( Control as Control_ResidentEvil2HealthLayer ).SetProfile( profile );
            base.SetApplication( profile );
        }
    }
}