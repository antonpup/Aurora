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
    public class PD2StatesLayerHandlerProperties : LayerHandlerProperties2Color<PD2StatesLayerHandlerProperties>
    {
        public Color? _DownedColor { get; set; }

        [JsonIgnore]
        public Color DownedColor { get { return Logic._DownedColor ?? _DownedColor ?? Color.Empty; } }

        public Color? _ArrestedColor { get; set; }

        [JsonIgnore]
        public Color ArrestedColor { get { return Logic._ArrestedColor ?? _ArrestedColor ?? Color.Empty; } }

        public Color? _SwanSongColor { get; set; }

        [JsonIgnore]
        public Color SwanSongColor { get { return Logic._SwanSongColor ?? _SwanSongColor ?? Color.Empty; } }

        public bool? _ShowSwanSong { get; set; }

        [JsonIgnore]
        public bool ShowSwanSong { get { return Logic._ShowSwanSong ?? _ShowSwanSong ?? false; } }

        public float? _SwanSongSpeedMultiplier { get; set; }

        [JsonIgnore]
        public float SwanSongSpeedMultiplier { get { return Logic._SwanSongSpeedMultiplier ?? _SwanSongSpeedMultiplier ?? 0.0F; } }

        public PD2StatesLayerHandlerProperties() : base() { }

        public PD2StatesLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            _DownedColor = Color.White;
            _ArrestedColor = Color.DarkRed;
            _ShowSwanSong = true;
            _SwanSongColor = Color.FromArgb(158, 205, 255);
            _SwanSongSpeedMultiplier = 1.0f;
        }

    }

    public class PD2StatesLayerHandler : LayerHandler<PD2StatesLayerHandlerProperties>
    {
        protected override UserControl CreateControl()
        {
            return new Control_PD2StatesLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer states_layer = new EffectLayer("Payday 2 - States");

            if (state is GameState_PD2)
            {
                GameState_PD2 pd2state = (GameState_PD2)state;

                if (pd2state.Game.State == GameStates.Ingame)
                {
                    if (pd2state.LocalPlayer.State == PlayerState.Incapacitated || pd2state.LocalPlayer.State == PlayerState.Bleed_out || pd2state.LocalPlayer.State == PlayerState.Fatal)
                    {
                        int incapAlpha = (int)(pd2state.LocalPlayer.DownTime > 10 ? 0 : 255 * (1.0D - (double)pd2state.LocalPlayer.DownTime / 10.0D));

                        if (incapAlpha > 255)
                            incapAlpha = 255;
                        else if (incapAlpha < 0)
                            incapAlpha = 0;

                        Color incapColor = Color.FromArgb(incapAlpha, Properties.DownedColor);

                        states_layer.Fill(incapColor).Set(Devices.DeviceKeys.Peripheral, incapColor);
                    }
                    else if (pd2state.LocalPlayer.State == PlayerState.Arrested)
                    {
                        states_layer.Fill(Properties.ArrestedColor).Set(Devices.DeviceKeys.Peripheral, Properties.ArrestedColor);
                    }

                    if (pd2state.LocalPlayer.IsSwanSong && Properties.ShowSwanSong)
                    {
                        double blend_percent = Math.Pow(Math.Cos((Utils.Time.GetMillisecondsSinceEpoch() % 1300L) / 1300.0D * Properties.SwanSongSpeedMultiplier * 2.0D * Math.PI), 2.0D);

                        Color swansongColor = Utils.ColorUtils.MultiplyColorByScalar(Properties.SwanSongColor, blend_percent);

                        EffectLayer swansong_layer = new EffectLayer("Payday 2 - Swansong", swansongColor).Set(Devices.DeviceKeys.Peripheral, swansongColor);

                        states_layer += swansong_layer;
                    }
                }
            }

            return states_layer;
        }

        public override void SetApplication(Application profile)
        {
            (_Control as Control_PD2StatesLayer)?.SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}