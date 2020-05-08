using Aurora.EffectsEngine;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Profiles.CSGO.GSI.Nodes;
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

namespace Aurora.Profiles.CSGO.Layers
{
    public class CSGOFlashbangLayerHandlerProperties : LayerHandlerProperties2Color<CSGOFlashbangLayerHandlerProperties>
    {
        public Color? _FlashbangColor { get; set; }

        [JsonIgnore]
        public Color FlashbangColor { get { return Logic._FlashbangColor ?? _FlashbangColor ?? Color.Empty; } }

        public CSGOFlashbangLayerHandlerProperties() : base() { }

        public CSGOFlashbangLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            this._FlashbangColor = Color.FromArgb(255, 255, 255);
        }

    }

    [Obsolete("This layer is obselete and has been replaced by the Overrides system.")]
    public class CSGOFlashbangLayerHandler : LayerHandler<CSGOFlashbangLayerHandlerProperties>
    {
        protected override UserControl CreateControl()
        {
            return new Control_CSGOFlashbangLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer flashed_layer = new EffectLayer("CSGO - Flashed");

            if (state is GameState_CSGO)
            {
                GameState_CSGO csgostate = state as GameState_CSGO;

                //Update Flashed
                if (csgostate.Player.State.Flashed > 0)
                {
                    Color flash_color = Color.FromArgb(csgostate.Player.State.Flashed, Properties.FlashbangColor);

                    flashed_layer.Fill(flash_color);
                }
            }

            return flashed_layer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_CSGOFlashbangLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}