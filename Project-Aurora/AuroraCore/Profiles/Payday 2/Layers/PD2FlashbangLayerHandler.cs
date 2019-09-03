using Aurora.EffectsEngine;
using Aurora.Profiles.Payday_2.GSI;
using Aurora.Profiles.Payday_2.GSI.Nodes;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System.Drawing;

namespace Aurora.Profiles.Payday_2.Layers
{
    public class PD2FlashbangLayerHandlerProperties : LayerHandlerProperties2Color<PD2FlashbangLayerHandlerProperties>
    {
        public Color? _FlashbangColor { get; set; }

        [JsonIgnore]
        public Color FlashbangColor { get { return Logic._FlashbangColor ?? _FlashbangColor ?? Color.Empty; } }

        public PD2FlashbangLayerHandlerProperties() : base() { }

        public PD2FlashbangLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            this._FlashbangColor = Color.FromArgb(255, 255, 255);
        }

    }

    public class PD2FlashbangLayerHandler : LayerHandler<PD2FlashbangLayerHandlerProperties>
    {
        public PD2FlashbangLayerHandler() : base()
        {
            _ID = "PD2Flashbang";
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer flashed_layer = new EffectLayer("Payday 2 - Flashed");

            if (state is GameState_PD2)
            {
                GameState_PD2 pd2state = state as GameState_PD2;

                //Update Flashed
                if (pd2state.Game.State == GameStates.Ingame && pd2state.LocalPlayer.FlashAmount > 0)
                {
                    Color flash_color = Utils.ColorUtils.MultiplyColorByScalar(Properties.FlashbangColor, pd2state.LocalPlayer.FlashAmount);

                    flashed_layer.Fill(flash_color);
                }
            }

            return flashed_layer;
        }
    }
}