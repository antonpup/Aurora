using Aurora.EffectsEngine;
using Aurora.Profiles.LeagueOfLegends.GSI.Nodes;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.LeagueOfLegends.Layers
{
    public class LoLBackgroundLayerHandlerProperties : LayerHandlerProperties<LoLBackgroundLayerHandlerProperties>
    {
        [JsonIgnore]
        public Dictionary<Champion, Color> ChampionColors => Logic._ChampionColors ?? _ChampionColors ?? new Dictionary<Champion, Color>();
        public Dictionary<Champion, Color> _ChampionColors { get; set; }

        public LoLBackgroundLayerHandlerProperties() : base() { }

        public LoLBackgroundLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            _ChampionColors = DefaultChampionColors.GetDictionary();
        }
    }

    [LayerHandlerMeta(Name = "League of Legends Background")]
    public class LoLBackgroundLayerHandler : LayerHandler<LoLBackgroundLayerHandlerProperties>
    {

        private readonly EffectLayer layer = new EffectLayer();
        private Champion lastChampion = Champion.None;
        private Color lastColor = Color.Transparent;
        private int lastWidth;
        private int lastHeight;

        public override EffectLayer Render(IGameState gamestate)
        {
            var currentChampion = (gamestate as GSI.GameState_LoL)?.Player.Champion ?? Champion.None;
            if (!Properties.ChampionColors.TryGetValue(currentChampion, out var currentColor))
                currentColor = Color.Black;

            //if the player changes champion
            //or if the color is adjusted in the UI
            //or if the canvas size changes due to the layout being changed
            if (currentChampion != lastChampion || currentColor != lastColor || 
              lastWidth != Effects.canvas_width || lastHeight != Effects.canvas_height)
            {
                lastChampion = currentChampion;
                lastColor = currentColor;
                lastHeight = Effects.canvas_height;
                lastWidth = Effects.canvas_width;
                layer.Fill(lastColor);
                //then we fill the layer again
            }
            //otherwise, we can just return the same layer as it's mostly static
            return layer;
        }

        protected override UserControl CreateControl()
        {
            return new LoLBackgroundLayer(this);
        }
    }
}
