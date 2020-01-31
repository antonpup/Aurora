using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Settings.Layers;
using System.Drawing;
using Aurora.Profiles.LeagueOfLegends.GSI.Nodes;
using Newtonsoft.Json;

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

    public class LoLBackgroundLayerHandler : LayerHandler<LoLBackgroundLayerHandlerProperties>
    {
        public  LoLBackgroundLayerHandler() : base()
        {
            _ID = "LoLBackgroundLayer";
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            return new EffectLayer().Fill(Properties.ChampionColors[(gamestate as GSI.GameState_LoL).Player.Champion]);
        }

        protected override UserControl CreateControl()
        {
            return new LoLBackgroundLayer(this);
        }
    }
}
